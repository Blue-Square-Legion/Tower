using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType(typeof(GameManager)) as GameManager;
            return instance;
        }
        set 
        {
            instance = value;
        }
    }
    #endregion

    public Queue<EnemyDamageData> damageData;
    public Queue<int> enemyQueueToSpawn;
    public Queue<Enemy> enemyQueueToRemove;
    public bool endLoop;
    [SerializeField] private Transform nodeParent;
    [NonSerialized] public List<TowerBehavior> builtTowers;
    [NonSerialized] public Vector3[] nodePositions;
    [NonSerialized] public float[] nodeDistances;

    EnemySpawner enemySpawner;
    void Start()
    {
        enemySpawner = EnemySpawner.Instance;
        endLoop = false;
        enemyQueueToSpawn = new();
        enemyQueueToRemove = new();
        damageData = new();
        builtTowers = new List<TowerBehavior>();
        enemySpawner.Init();

        int numOfNodes = nodeParent.childCount;
        nodePositions = new Vector3[numOfNodes];

        for (int i = 0; i < numOfNodes; i++)
        {
            nodePositions[i] = nodeParent.GetChild(i).position;
        }

        nodeDistances = new float[numOfNodes - 1];
        for (int i = 0; i < numOfNodes - 1; i++)
        {
            nodeDistances[i] = Vector3.Distance(nodePositions[i], nodePositions[i + 1]);
        }

        StartCoroutine(GameLoop());
        InvokeRepeating("summonTest", 0f, 1f);
    }

    public void summonTest()
    {
        EnqueueEnemy(1);
    }

    IEnumerator GameLoop()
    {
        //Organizes game loop to avoid errors caused by threading and editing game data while simotaneously accessing it
            while (!endLoop)
            {
            //Spawn Enemies
            if (enemyQueueToSpawn.Count > 0)
            {
                int queueSuze = enemyQueueToSpawn.Count;
                for (int i = 0; i < queueSuze; i++)
                {
                    enemySpawner.spawnEnemy(enemyQueueToSpawn.Dequeue());
                }
            }

            //Spawn Towers

            //Move Enemies
            NativeArray<Vector3> nodesToUse = new(nodePositions, Allocator.TempJob);
            NativeArray<float> enemySpeeds = new(enemySpawner.spawnedEnemies.Count, Allocator.TempJob);
            NativeArray<int> nodeIndicies = new(enemySpawner.spawnedEnemies.Count, Allocator.TempJob);
            TransformAccessArray enemyAccess = new(enemySpawner.spawnedEnemiesTransform.ToArray(), 2);

            for (int i = 0; i < enemySpawner.spawnedEnemies.Count; i++)
            {
                enemySpeeds[i] = enemySpawner.spawnedEnemies[i].speed;
                nodeIndicies[i] = enemySpawner.spawnedEnemies[i].nodeIndex;
            }

            MoveEnemies moveEnemies = new MoveEnemies
            {
                nodePositions = nodesToUse,
                enemySpeed = enemySpeeds,
                nodeIndex = nodeIndicies,
                deltaTime = Time.deltaTime
            };

            JobHandle moveJobHandle =  moveEnemies.Schedule(enemyAccess);
            moveJobHandle.Complete();

            for (int i = 0; i < enemySpawner.spawnedEnemies.Count; i++)
            {
                enemySpawner.spawnedEnemies[i].nodeIndex = nodeIndicies[i];

                if (enemySpawner.spawnedEnemies[i].nodeIndex == nodePositions.Length)
                {
                    //Enemy Reached the end of the map
                    EnqueEnemyToRemove(enemySpawner.spawnedEnemies[i]);
                }
            }

            nodesToUse.Dispose();
            enemySpeeds.Dispose();
            nodeIndicies.Dispose();
            enemyAccess.Dispose();

            //Tick Towers
            foreach(TowerBehavior tower in builtTowers)
            {
                tower.target = TowerTargetting.GetTarget(tower, TowerTargetting.TargetType.First);
                tower.Tick();
            }


            //Apply Effects

            //Damage Enemies
            if (damageData.Count > 0)
            {
                int damageSize = damageData.Count;
                for (int i = 0; i < damageSize; i++)
                {
                    EnemyDamageData currentDamageData = damageData.Dequeue();
                    currentDamageData.targettedEnemy.currentHealth -= currentDamageData.totalDamage / currentDamageData.resistance;

                    if (currentDamageData.targettedEnemy.currentHealth <= 0)
                        EnqueEnemyToRemove(currentDamageData.targettedEnemy);
                }
            }


            //Remove Enemies
            if (enemyQueueToRemove.Count > 0)
            {
                int removeSize = enemyQueueToRemove.Count;
                for (int i = 0; i < removeSize; i++)
                {
                    enemySpawner.RemoveEnemy(enemyQueueToRemove.Dequeue());
                }
            }

            //Remove Towers

            yield return null;
        }
    }

    public void EnqueueDamageData(EnemyDamageData damageData)
    {
        this.damageData.Enqueue(damageData);
    }

    /// <summary>
    /// Enqueues enemies to spawn when the game allows it to
    /// </summary>
    /// <param name="enemyID"></param>
    public void EnqueueEnemy(int enemyID)
    {
        enemyQueueToSpawn.Enqueue(enemyID);
    }

    public void EnqueEnemyToRemove(Enemy enemyToRemove)
    {
        enemyQueueToRemove.Enqueue(enemyToRemove);
    }

    public struct EnemyDamageData
    {
        public EnemyDamageData(Enemy targettedEnemy,  float totalDamage, float resistance)
        {
            this.targettedEnemy = targettedEnemy;
            this.totalDamage = totalDamage;
            this.resistance = resistance;
        }

        public Enemy targettedEnemy;
        public float totalDamage;
        public float resistance;
    }

    public struct MoveEnemies : IJobParallelForTransform
    {
        [NativeDisableParallelForRestriction]
        public NativeArray<Vector3> nodePositions;
        public NativeArray<float> enemySpeed;
        public NativeArray<int> nodeIndex;


        public float deltaTime;

        public void Execute(int index, TransformAccess transform)
        {
            if (nodeIndex[index] < nodePositions.Length) //Failsafe
            {
                Vector3 positionToMoveTo = nodePositions[nodeIndex[index]];

                transform.position = Vector3.MoveTowards(transform.position, positionToMoveTo, enemySpeed[index] * deltaTime);

                if (transform.position == positionToMoveTo)
                {
                    nodeIndex[index]++;
                }
            }
        }
    }
}