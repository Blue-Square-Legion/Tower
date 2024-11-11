using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using static UnityEngine.Splines.SplineInstantiate;

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
    private Queue<ApplyEffectData> effectQueue;
    public Queue<int> enemyQueueToSpawn;
    public Queue<Enemy> enemyQueueToRemove;
    public bool endLoop;
    [SerializeField] private Transform nodeParent;
    [NonSerialized] public List<TowerBehavior> builtTowers;
    [NonSerialized] public Vector3[] nodePositions;
    [NonSerialized] public float[] nodeDistances;
    [SerializeField] public Transform SpawnPoint;
    [SerializeField] public Transform NextSpawnPoint;

    [SerializeField] Player player;

    EnemySpawner enemySpawner;
    private bool waveActive = false;
    private bool endOfWave;
    int currentWave;
    int selectedSpawnpoint;
    int nextSpawnpoint;
    public GameObject SelectedTower;
    public int farmBonus;

    void Start()
    {
        enemySpawner = EnemySpawner.Instance;
        endLoop = false;
        endOfWave = false;
        enemyQueueToSpawn = new();
        enemyQueueToRemove = new();
        damageData = new();
        effectQueue = new();
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

        currentWave = 0;
        farmBonus = 0;
        selectedSpawnpoint = 0;
        nextSpawnpoint = 1;
        SpawnPoint = EnemySpawner.Instance.SpawnPoints[selectedSpawnpoint];
        NextSpawnPoint = EnemySpawner.Instance.SpawnPoints[nextSpawnpoint];
        NextSpawnPoint.GetChild(0).gameObject.SetActive(true);
        StartCoroutine(GameLoop());
    }

    public void EnqueueWave()
    {
        if (waveActive) return;
        waveActive = true;
        StartCoroutine(Wave(currentWave));
        currentWave++;
        SpawnPoint.GetChild(0).gameObject.SetActive(false);
        NextSpawnPoint.GetChild(0).gameObject.SetActive(true);
        selectedSpawnpoint++;
        nextSpawnpoint++;
        if (selectedSpawnpoint > EnemySpawner.Instance.SpawnPoints.Length-1)
            selectedSpawnpoint = 0;
        SpawnPoint = EnemySpawner.Instance.SpawnPoints[selectedSpawnpoint];

        if (nextSpawnpoint > EnemySpawner.Instance.SpawnPoints.Length - 1)
            nextSpawnpoint = 0;
        NextSpawnPoint = EnemySpawner.Instance.SpawnPoints[nextSpawnpoint];
        endOfWave = true;
    }

    IEnumerator Wave(int wave)
    {
        switch (wave)
        {
            case 0:
                EnqueueEnemy(1);
                yield return new WaitForSeconds(1);
                EnqueueEnemy(1);
                yield return new WaitForSeconds(1);
                EnqueueEnemy(1);
                yield return new WaitForSeconds(1);
                EnqueueEnemy(1);
                yield return new WaitForSeconds(1);
                EnqueueEnemy(1);
                yield return new WaitForSeconds(1);
                break;
            case 1:
                for (int i = 0; i < 9; i++)
                {
                    EnqueueEnemy(1);
                    yield return new WaitForSeconds(1);
                }
                break;
            case 2:
                for (int i = 0; i < 5; i++)
                {
                    EnqueueEnemy(2);
                    yield return new WaitForSeconds(0.5f);
                }
                break;
            case 3:

                for (int i = 0; i < 5; i++)
                {
                    EnqueueEnemy(1);
                    yield return new WaitForSeconds(1);
                    EnqueueEnemy(2);
                    yield return new WaitForSeconds(1);
                }
                break;
            case 4:
                for (int i = 0; i < 6; i++)
                {
                    EnqueueEnemy(1);
                    yield return new WaitForSeconds(1f);
                    EnqueueEnemy(1);
                    yield return new WaitForSeconds(1f);
                    EnqueueEnemy(1);
                    yield return new WaitForSeconds(1f);
                    EnqueueEnemy(2);
                    yield return new WaitForSeconds(0.5f);
                    EnqueueEnemy(2);
                    yield return new WaitForSeconds(.5f);
                }
                break;
            case 5:
                for (int i = 0; i < 10; i++)
                {
                    EnqueueEnemy(3);
                    yield return new WaitForSeconds(1);
                }
                break;
            case 6:
                for (int i = 0; i < 6; i++)
                {
                    EnqueueEnemy(3);
                    yield return new WaitForSeconds(1);
                }
                for (int i = 0; i < 50; i++)
                {
                    EnqueueEnemy(2);
                    yield return new WaitForSeconds(.25f);
                }
                break;
            case 7:
                for (int i = 0; i < 10; i++)
                {
                    EnqueueEnemy(3);
                    yield return new WaitForSeconds(1f);
                    EnqueueEnemy(2);
                    yield return new WaitForSeconds(0.25f);
                    EnqueueEnemy(1);
                    yield return new WaitForSeconds(0.5f);
                    EnqueueEnemy(3);
                    yield return new WaitForSeconds(0.1f);
                }
                break;
            case 8:
                for (int i = 0; i < 50; i++)
                {
                    EnqueueEnemy(2);
                    yield return new WaitForSeconds(0.15f);
                }
                break;
            case 9:
                for (int i = 0; i < 100; i++)
                {
                    EnqueueEnemy(3);
                    yield return new WaitForSeconds(.5f);
                }
                for (int i = 0; i < 50; i++)
                {
                    EnqueueEnemy(2);
                    EnqueueEnemy(1);
                    yield return new WaitForSeconds(0.1f);
                }
                break;
            default:
                currentWave = 0;
                StartCoroutine(Wave(currentWave));
                break;
        }
    }

    private void WaveBonus(int wave)
    {
        int waveBonus = 0;
        switch(wave)
        {
            case 0:
            case 1:
                waveBonus = 100;
                break;
            case 2:
            case 3:
            case 4:
            case 5:
                waveBonus = 150;
                break;
            case 6:
            case 7:
            case 8:
                waveBonus = 200;
                break;
            case 9:
                waveBonus = 250;
                break;
        }
        player.GiveMoney(waveBonus + farmBonus);
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
                    Enemy enemy = enemySpawner.spawnEnemy(enemyQueueToSpawn.Dequeue());
                    enemy.GetComponent<NavMeshMovement>().ResetDestination();
                }
            }

            //Move Enemies

            for (int i = 0; i < enemySpawner.spawnedEnemies.Count; i++)
            {
                //enemySpawner.spawnedEnemies[i].nodeIndex = nodeIndicies[i];
                if (enemySpawner.spawnedEnemies[i].navMeshMovement.agent != null && enemySpawner.spawnedEnemies[i].navMeshMovement.ReachedEnd() 
                    && !enemySpawner.spawnedEnemies[i].isConfused)
                {
                    //Enemy Reached the end of the map
                    EnqueEnemyToRemove(enemySpawner.spawnedEnemies[i]);
                    player.DoDamage((int) enemySpawner.spawnedEnemies[i].currentHealth);
                }
            }

            //nodesToUse.Dispose();
            //enemySpeeds.Dispose();
            //nodeIndicies.Dispose();
            //enemyAccess.Dispose();

            //Tick Towers
            foreach (TowerBehavior tower in builtTowers)
            {
                tower.target = TowerTargetting.GetTarget(tower, TowerTargetting.TargetType.First);
                tower.Tick();
            }

            //Apply Effects
            if (effectQueue.Count > 0)
            {
                int effectSize = effectQueue.Count;
                for (int i = 0; i < effectSize; i++)
                {
                    ApplyEffectData currentDamageData = effectQueue.Dequeue();

                    Effect effectDuplicate = currentDamageData.enemyToAffect.activeEffects.Find(x => x.effectName == currentDamageData.effectToApply.effectName);
                    if (effectDuplicate == null)
                    {
                        currentDamageData.enemyToAffect.activeEffects.Add(currentDamageData.effectToApply);
                    }
                    else
                    {
                        effectDuplicate.duration = currentDamageData.effectToApply.duration;
                    }
                }
            }

            //Tick Enemies
            foreach(Enemy currentEnemy in enemySpawner.spawnedEnemies)
            {
                currentEnemy.Tick();
            }


            //Damage Enemies
            if (damageData.Count > 0)
            {
                int damageSize = damageData.Count;
                for (int i = 0; i < damageSize; i++)
                {
                    EnemyDamageData currentDamageData = damageData.Dequeue();
                    currentDamageData.targettedEnemy.currentHealth -= currentDamageData.totalDamage / currentDamageData.resistance;
                    currentDamageData.targettedEnemy.GetComponentInChildren<HealthBar>().UpdateHealth((int) currentDamageData.targettedEnemy.currentHealth);
                    if (currentDamageData.targettedEnemy.currentHealth <= 0)
                    {
                        player.GiveMoney(currentDamageData.targettedEnemy.moneyToPlayer);
                        EnqueEnemyToRemove(currentDamageData.targettedEnemy);
                    }
                        
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

            if (enemySpawner.spawnedEnemies.Count == 0 && endOfWave)
            {
                waveActive = false;
                SpawnPoint.GetChild(0).gameObject.SetActive(false);
                NextSpawnPoint.GetChild(0).gameObject.SetActive(true);
                WaveBonus(currentWave);
                endOfWave = false;
            }
                

            //Remove Towers

            yield return null;
        }
    }

    public void EnqueueAffectToApply(ApplyEffectData effectData)
    {
        effectQueue.Enqueue(effectData);
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

    public class Effect
    {
        public Effect(EffectNames effectName, float damage, float duration, float damageRate, float modifier)
        {
            this.effectName = effectName;
            this.damage = damage;
            this.duration = duration;
            this.damageRate = damageRate;
            this.modifier = modifier;
        }
        public EffectNames effectName;
        public float damage;
        public float duration;
        public float damageRate;
        public float damageDelay;
        public float modifier;
    }

    public struct ApplyEffectData
    {
        public ApplyEffectData(Effect effectToApply, Enemy enemyToAffect)
        {
            this.effectToApply = effectToApply;
            this.enemyToAffect = enemyToAffect;
        }
        public Effect effectToApply;
        public Enemy enemyToAffect;
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

    public enum EffectNames
    {
        Fire
    }
}