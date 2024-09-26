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

    [SerializeField] Transform SpawnPoint;

    private bool waveActive = false;

    public  bool isInitialized = false;
    GameManager gameManager;
    public void Init()
    {
        if (!isInitialized) //Error Check. Does not allow this to be loaded twice (causes way too many glitches and bugs)
        {
            gameManager = GameManager.Instance;
            enemyPrefab = new();
            enemyObjectPools = new();
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

    public Enemy spawnEnemy(int enemyID)
    {
        Enemy spawnedEnemy = null;

        if (enemyPrefab.ContainsKey(enemyID))
        {
            Queue<Enemy> ReferencedQueue = enemyObjectPools[enemyID];

            if (ReferencedQueue.Count > 0)
            {
                //Dequeue enemy and initialize it
                spawnedEnemy = ReferencedQueue.Dequeue();
                spawnedEnemy.Init();

                spawnedEnemy.gameObject.SetActive(true);
            }
            else
            {
                //Instantiate new insatnce of enemy and initialize
                GameObject newEnemy = Instantiate(enemyPrefab[enemyID], gameManager.nodePositions[0], Quaternion.identity);
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
        spawnedEnemy.ID = enemyID;
        return spawnedEnemy;
    }

    public void RemoveEnemy(Enemy enemyToRemove)
    {
        enemyObjectPools[enemyToRemove.ID].Enqueue(enemyToRemove); //Makes enemy idle and inactive - extremely efficient
        enemyToRemove.gameObject.SetActive(false);
        spawnedEnemiesTransform.Remove(enemyToRemove.transform);
        spawnedEnemies.Remove(enemyToRemove);
    }
}