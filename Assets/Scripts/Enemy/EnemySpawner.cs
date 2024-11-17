using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Create Enemy Data")]
public class EnemySpawner : MonoBehaviour
{
    #region Singleton
    private static EnemySpawner instance;

    public static EnemySpawner Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType(typeof(EnemySpawner)) as EnemySpawner;
            return instance;
        }
        set
        {
            instance = value;
        }
    }
    #endregion

    public  List<Enemy> spawnedEnemies;
    public  List<Transform> spawnedEnemiesTransform;

    //Connects enemy IDs to thier gameobject
    public  Dictionary<int, GameObject> enemyPrefab;

    //Creates Object Pooling
    public  Dictionary<int, Queue<Enemy>> enemyObjectPools;

    public Dictionary<Transform, Enemy> enemyTransformDictionary;

    public Transform[] SpawnPoints;

    public  bool isInitialized = false;
    [SerializeField] private Transform enemiesFolder;
    GameManager gameManager;
    public void Init()
    {
        if (!isInitialized) //Error Check. Does not allow this to be loaded twice (causes way too many glitches and bugs)
        {
            gameManager = GameManager.Instance;
            enemyPrefab = new();
            enemyObjectPools = new();
            enemyTransformDictionary = new();
            spawnedEnemies = new();
            spawnedEnemiesTransform = new();

            //Loads Enemy Data on runtime
            EnemyData[] enemies = Resources.LoadAll<EnemyData>("Enemies");

            //Loads enemy data into Dictionaries
            foreach (EnemyData enemy in enemies)
            {
                enemyPrefab.Add(enemy.enemyID, enemy.enemyPrefab);
                enemyObjectPools.Add(enemy.enemyID, new Queue<Enemy>());
            }

            isInitialized = true;
        }
        else
            print("FAILSAFE ACTIVATED. FAILED TO INITIALZE \"ENEMY SPAWNER\" DUE TO IT ALREADY BEING INITIALZED!");
    }

    public Enemy spawnEnemy(Tuple<int,int> tuple)
    {
        Enemy spawnedEnemy = null;

        int enemyID = tuple.Item1;
        int spawnPointID = tuple.Item2;

        if (enemyPrefab.ContainsKey(enemyID))
        {
            Queue<Enemy> ReferencedQueue = enemyObjectPools[enemyID];

            if (ReferencedQueue.Count > 0)
            {
                //Dequeue enemy and initialize it
                spawnedEnemy = ReferencedQueue.Dequeue();
                spawnedEnemy.Init();
                spawnedEnemy.transform.position = SpawnPoints[spawnPointID].position;
                spawnedEnemy.GetComponentInChildren<HealthBar>().UpdateHealth(spawnedEnemy.maxHealth);
                spawnedEnemy.gameObject.SetActive(true);
            }
            else
            {
                //Instantiate new insatnce of enemy and initialize
                newEnemy.transform.parent = enemiesFolder;
                GameObject newEnemy = Instantiate(enemyPrefab[enemyID], SpawnPoints[spawnPointID].position, Quaternion.identity);
                spawnedEnemy = newEnemy.GetComponent<Enemy>();
                spawnedEnemy.Init();
            }
        }
        else
        {
            print("ERROR: ENEMY WITH ID OF \"" + enemyID + "\" DOES NOT EXIST!");
            return null;
        }

        if (!spawnedEnemies.Contains(spawnedEnemy)) spawnedEnemies.Add(spawnedEnemy);
        if (!spawnedEnemiesTransform.Contains(spawnedEnemy.transform)) spawnedEnemiesTransform.Add(spawnedEnemy.transform);
        if (!enemyTransformDictionary.ContainsKey(spawnedEnemy.transform)) enemyTransformDictionary.Add(spawnedEnemy.transform, spawnedEnemy);
        spawnedEnemy.ID = enemyID;
        return spawnedEnemy;
    }

    public void RemoveEnemy(Enemy enemyToRemove)
    {
        enemyObjectPools[enemyToRemove.ID].Enqueue(enemyToRemove); //Makes enemy idle and inactive - extremely efficient
        enemyToRemove.gameObject.SetActive(false);

        enemyTransformDictionary.Remove(enemyToRemove.transform);
        spawnedEnemiesTransform.Remove(enemyToRemove.transform);
        spawnedEnemies.Remove(enemyToRemove);
    }

    //deactivates all arrows above the spawning portals
    public void DeactivateAllSpawnIndicators()
    {
        for (int i = 0; i < SpawnPoints.Length; i++)
        {
            SpawnPoints[i].GetChild(0).gameObject.SetActive(false);
        }
        print("deactivated spawn indicators");
    }

    //toggle indicator above specific spawning portal
    public void ActivateSpawnIndicators(int[] spawnPoints)
    {
        foreach (int id in spawnPoints)
        {
            SpawnPoints[id].GetChild(0).gameObject.SetActive(true);
            print("activated spawn indicator: " + id);
        }
    }
}