using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    private Queue<ApplyBuffData> buffAddQueue;
    private Queue<ApplyBuffData> buffRemoveQueue;
    public Queue<Tuple<int, int>> enemyQueueToSpawn;    //Tuple<EnemyID, SpawnPointID>
    public Queue<Enemy> enemyQueueToRemove;
    public Queue<TowerBehavior> towerQueueToRemove;
    public bool endLoop;
    [SerializeField] private Transform nodeParent;
    [NonSerialized] public List<TowerBehavior> builtTowers;
    [NonSerialized] public Vector3[] nodePositions;
    [NonSerialized] public float[] nodeDistances;
    [SerializeField] TextMeshProUGUI waveText;
    [SerializeField] Player player;

    public bool autoStart;

    EnemySpawner enemySpawner;
    public bool waveActive;
    private bool endOfWave;
    int currentWave;
    int[] nextSpawnPoints;
    public GameObject SelectedTower;
    public int farmBonus;
    public bool showPaths;

    void Start()
    {
        enemySpawner = EnemySpawner.Instance;
        endLoop = false;
        endOfWave = true;
        enemyQueueToSpawn = new();
        enemyQueueToRemove = new();
        towerQueueToRemove = new();
        damageData = new();
        effectQueue = new();
        buffAddQueue = new();
        buffRemoveQueue = new();
        builtTowers = new List<TowerBehavior>();
        enemySpawner.Init();
        nextSpawnPoints = new int[] {0};
        autoStart = false;
        waveActive = false;
        showPaths = false;

        UIManager.Instance.UpdateAutoStartText("Auto-start:\n" + autoStart);

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
        StartCoroutine(GameLoop());
        EnemySpawner.Instance.DeactivateAllSpawnIndicators();
        EnemySpawner.Instance.ActivateSpawnIndicators(nextSpawnPoints);
    }

    public void EnqueueWave()
    {
        if (waveActive)
        {
            print("there is already an active wave");
            return;
        }
        waveActive = true;
        StartCoroutine(Wave(currentWave));
        currentWave++;
        UpdateWaveText();
        endOfWave = true;
    }


    //Add new waves here

    /*
     * template for new wave:
       case *waveNumber*:
                for (int i = 0; i < *numberOfEnemies*; i++)
                {
                    EnqueueEnemy(*enemyID*,*spawnID*);
                    yield return new WaitForSeconds(*timeBetweenSpawn*f);
                }
                nextSpawnPoints = new int[] { *IDs of all spawnpoints used for next wave. Leave blank if is last wave* };
                break;
     */
    IEnumerator Wave(int wave)
    {
        switch (wave)
        {
            case 0:
                for (int i = 0; i < 5; i++)
                {
                    EnqueueEnemy(Enemy.EnemyType.Basic, 1,0);
                    yield return new WaitForSeconds(1);
                }
                nextSpawnPoints = new int[] { 1 };
                break;
            case 1:
                for (int i = 0; i < 9; i++)
                {
                    EnqueueEnemy(Enemy.EnemyType.Basic, 1, 0);
                    yield return new WaitForSeconds(1);
                }
                nextSpawnPoints = new int[] { 2 };
                break;
            case 2:
                for (int i = 0; i < 5; i++)
                {
                    EnqueueEnemy(Enemy.EnemyType.Fast, 1, 0);
                    yield return new WaitForSeconds(0.5f);
                }
                nextSpawnPoints = new int[] { 0 };
                break;
            case 3:

                for (int i = 0; i < 5; i++)
                {
                    EnqueueEnemy(Enemy.EnemyType.Basic,1, 0);
                    yield return new WaitForSeconds(1);
                    EnqueueEnemy(Enemy.EnemyType.Ghost, 1, 0);
                    yield return new WaitForSeconds(1);
                }
                nextSpawnPoints = new int[] { 1 };
                break;
            case 4:
                for (int i = 0; i < 6; i++)
                {
                    EnqueueEnemy(Enemy.EnemyType.Basic, 1, 0);
                    yield return new WaitForSeconds(1f);
                    EnqueueEnemy(Enemy.EnemyType.Basic, 1, 0);
                    yield return new WaitForSeconds(1f);
                    EnqueueEnemy(Enemy.EnemyType.Basic, 1, 0);
                    yield return new WaitForSeconds(1f);
                    EnqueueEnemy(Enemy.EnemyType.Fast, 1, 0);
                    yield return new WaitForSeconds(0.5f);
                    EnqueueEnemy(Enemy.EnemyType.Fast, 1, 0);
                    yield return new WaitForSeconds(.5f);
                }
                nextSpawnPoints = new int[] { 2 };
                break;
            case 5:
                for (int i = 0; i < 10; i++)
                {
                    EnqueueEnemy(Enemy.EnemyType.Slow, 1,2);
                    yield return new WaitForSeconds(1);
                }
                nextSpawnPoints = new int[] { 0 };
                break;
            case 6:
                for (int i = 0; i < 6; i++)
                {
                    EnqueueEnemy(Enemy.EnemyType.Slow, 1, 0);
                    yield return new WaitForSeconds(1);
                }
                for (int i = 0; i < 50; i++)
                {
                    EnqueueEnemy(Enemy.EnemyType.Fast, 1 ,1);
                    yield return new WaitForSeconds(.25f);
                }
                nextSpawnPoints = new int[] { 1 };
                break;
            case 7:
                for (int i = 0; i < 10; i++)
                {
                    EnqueueEnemy(Enemy.EnemyType.Slow, 1,2);
                    yield return new WaitForSeconds(1f);
                    EnqueueEnemy(Enemy.EnemyType.Slow, 1,2);
                    yield return new WaitForSeconds(0.25f);
                    EnqueueEnemy(Enemy.EnemyType.Slow, 1,2);
                    yield return new WaitForSeconds(0.5f);
                    EnqueueEnemy(Enemy.EnemyType.Slow,1, 2);
                    yield return new WaitForSeconds(0.1f);
                }
                nextSpawnPoints = new int[] { 2 };
                break;
            case 8:
                for (int i = 0; i < 50; i++)
                {
                    EnqueueEnemy(Enemy.EnemyType.Fast, 1,0);
                    yield return new WaitForSeconds(0.15f);
                }
                nextSpawnPoints = new int[] { 0 };
                break;
            case 9:
                for (int i = 0; i < 100; i++)
                {
                    EnqueueEnemy(Enemy.EnemyType.Slow, 1,1);
                    yield return new WaitForSeconds(.5f);
                }
                for (int i = 0; i < 50; i++)
                {
                    EnqueueEnemy(Enemy.EnemyType.Fast, 1,2);
                    EnqueueEnemy(Enemy.EnemyType.Basic, 1,2);
                    yield return new WaitForSeconds(0.1f);
                }
                nextSpawnPoints = new int[] { 1 };
                break;
            default:
                currentWave = 0;
                StartCoroutine(Wave(currentWave));
                break;
        }
    }

    private void UpdateWaveText()
    {
        if (waveText != null)
        {
            waveText.text = "Wave " + currentWave;
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
                int queueSize = enemyQueueToSpawn.Count;
                for (int i = 0; i < queueSize; i++)
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
                    EnqueueEnemyToRemove(enemySpawner.spawnedEnemies[i]);
                    player.DoDamage((int) enemySpawner.spawnedEnemies[i].currentHealth);
                }
            }

            //Tick Towers
            foreach (TowerBehavior tower in builtTowers)
            {
                tower.target = TowerTargetting.GetTarget(tower, tower.targetType);
                tower.Tick();
            }

            //Tick Tower Buffs
            foreach (TowerBehavior tower in builtTowers)
            {
                tower.target = TowerTargetting.GetTarget(tower, tower.targetType);
                tower.TickBuffs();
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
                    Enemy targetedEnemy = currentDamageData.targetedEnemy;
                    targetedEnemy.currentHealth = Mathf.Round((targetedEnemy.currentHealth - 
                            (currentDamageData.totalDamage / currentDamageData.resistance)) * 100f) / 100f; //Removes floating point errors

                    targetedEnemy.GetComponentInChildren<HealthBar>().UpdateHealth((int) targetedEnemy.currentHealth);
                    if (targetedEnemy.currentHealth <= 0)
                    {
                        player.GiveMoney(targetedEnemy.moneyToPlayer);
                        EnqueueEnemyToRemove(currentDamageData.targetedEnemy);
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

            //all enemies defeated
            if (enemySpawner.spawnedEnemies.Count == 0 && endOfWave)
            {
                waveActive = false;
                EnemySpawner.Instance.DeactivateAllSpawnIndicators();
                EnemySpawner.Instance.ActivateSpawnIndicators(nextSpawnPoints);
                print("Wave ended");
                print(nextSpawnPoints[0]);
                WaveBonus(currentWave);
                endOfWave = false;

                //auto start next wave if enabled
                if (autoStart)
                {
                    waveActive = true;
                    StartCoroutine(Wave(currentWave));
                    currentWave++;
                    UpdateWaveText();
                    endOfWave = true;
                }

            }

            //Apply Buffs
            if (buffAddQueue.Count > 0)
            {
                int buffAddSize = buffAddQueue.Count;
                for (int i = 0; i < buffAddSize; i++)
                {
                    ApplyBuffData currentBuffData = buffAddQueue.Dequeue();

                    currentBuffData.towerToAffect.activeBuffs.Add(currentBuffData.buffToApply);
                    currentBuffData.towerToAffect.ApplyBuffs();
                }
            }

            //Removes Buffs
            if (buffRemoveQueue.Count > 0)
            {
                int buffRemoveSize = buffRemoveQueue.Count;
                for (int i = 0; i < buffRemoveSize; i++)
                {
                    ApplyBuffData currentBuffData = buffRemoveQueue.Dequeue();

                    TowerBehavior towerToAffect = currentBuffData.towerToAffect;

                    //Finds an identical buff and removes it
                    for (int j = 0; j < towerToAffect.activeBuffs.Count; j++)
                    {
                        if (towerToAffect.activeBuffs[j].buffName == currentBuffData.buffToApply.buffName
                            && towerToAffect.activeBuffs[j].modifier == currentBuffData.buffToApply.modifier
                            && towerToAffect.activeBuffs[j].duration == currentBuffData.buffToApply.duration)
                        {
                            towerToAffect.activeBuffs.RemoveAt(j);
                            currentBuffData.towerToAffect.ApplyBuffs();
                            break;
                        }
                    }
                }
            }

            //Remove Towers
            if (towerQueueToRemove.Count > 0)
            {
                int removeSize = towerQueueToRemove.Count;
                GameObject tempTower = towerQueueToRemove.Peek().gameObject;
                for (int i = 0; i < removeSize; i++)
                {
                    builtTowers.Remove(towerQueueToRemove.Dequeue());
                }
                Destroy(tempTower);
            }

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
    public void EnqueueEnemy(Enemy.EnemyType type, int level, int spawnPointNumber)
    {
        int enemyID = level;

        //Converts type to level - this system is under the assumption that 10 or more variants of this type do not exist.
        switch(type)
        {
            case Enemy.EnemyType.Basic:
                enemyID += 10;
                break;
            case Enemy.EnemyType.Fast:
                enemyID += 20;
                break;
            case Enemy.EnemyType.Slow:
                enemyID += 30;
                break;
            case Enemy.EnemyType.Ghost:
                enemyID += 40;
                break;
            case Enemy.EnemyType.Boss1:
                enemyID += 50;
                break;
        }

        enemyQueueToSpawn.Enqueue(new Tuple<int, int>(enemyID, spawnPointNumber));
    }

    public void EnqueueEnemyToRemove(Enemy enemyToRemove)
    {
        enemyQueueToRemove.Enqueue(enemyToRemove);
    }

    public void EnqueueTowerToRemove(TowerBehavior towerToRemove)
    {
        towerQueueToRemove.Enqueue(towerToRemove);
    }

    public void EnqueueBuffToApply(ApplyBuffData buffData)
    {
        buffAddQueue.Enqueue(buffData);
    }
    
    public void EnqueueBuffToRemove(ApplyBuffData buffData)
    {
        buffRemoveQueue.Enqueue(buffData);
    }

    public class Effect
    {
        public EffectNames effectName;
        public float damage;
        public float duration;
        public float damageRate;
        public float damageDelay;
        public float modifier;
        public Effect(EffectNames effectName, float damage, float duration, float damageRate, float modifier)
        {
            this.effectName = effectName;
            this.damage = damage;
            this.duration = duration;
            this.damageRate = damageRate;
            this.modifier = modifier;
        }
    }

    public struct ApplyEffectData
    {
        public Effect effectToApply;
        public Enemy enemyToAffect;
        public ApplyEffectData(Effect effectToApply, Enemy enemyToAffect)
        {
            this.effectToApply = effectToApply;
            this.enemyToAffect = enemyToAffect;
        }
    }

    public class Buff
    {
        public BuffNames buffName;
        public float modifier;
        public float duration; //NOTE: A duration of -123 will not disappear until removed manually
        public Buff(BuffNames buffName, float modifier, float duration)
        {
            this.buffName = buffName;
            this.modifier = modifier;
            this.duration = duration;
        }
    }

    public struct ApplyBuffData
    {
        public Buff buffToApply;
        public TowerBehavior towerToAffect;
        public ApplyBuffData(Buff buffToApply, TowerBehavior towerToAffect)
        {
            this.buffToApply = buffToApply;
            this.towerToAffect = towerToAffect;
        }
    }

    public struct EnemyDamageData
    {
        public Enemy targetedEnemy;
        public float totalDamage;
        public float resistance;
        public EnemyDamageData(Enemy targettedEnemy,  float totalDamage, float resistance)
        {
            this.targetedEnemy = targettedEnemy;
            this.totalDamage = totalDamage;
            this.resistance = resistance;
        }
    }

    public enum EffectNames
    {
        Burn,
        Slow
    }

    public enum BuffNames
    {
        SupportBonusRange,
        SupportBonusDamage,
        SupportBonusAttackSpeed,
        Stun
    }

    public void ToggleAutoStart()
    {
        autoStart = !autoStart;
        UIManager.Instance.UpdateAutoStartText("Auto-start:\n"+ autoStart);
        if (!autoStart) return;
        if (enemySpawner.spawnedEnemies.Count == 0 && !waveActive)
        {
            waveActive = true;
            StartCoroutine(Wave(currentWave));
            currentWave++;
            UpdateWaveText();
            endOfWave = true;
        }

    }

    public void ToggleShowPaths()
    {
        showPaths = !showPaths;
        UIManager.Instance.UpdateShowPathsText("Show paths:\n" + showPaths);
    }
}