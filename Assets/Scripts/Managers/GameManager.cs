using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using AudioSystem;

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

    [NonSerialized] public Queue<EnemyDamageData> damageData;
    private Queue<ApplyBuffData> buffAddQueue;
    private Queue<ApplyBuffData> buffRemoveQueue;
    private Queue<ApplyEnemyBuffData> enemyBuffAddQueue;
    private Queue<ApplyEnemyBuffData> enemyBuffRemoveQueue;
    [NonSerialized] public Queue<Tuple<int, int, bool>> enemyQueueToSpawn;    //Tuple<EnemyID, SpawnPointID, isinvisible>
    [NonSerialized] public Queue<Enemy> enemyQueueToRemove;
    [NonSerialized] public Queue<TowerBehavior> towerQueueToRemove;
    [NonSerialized] public bool endLoop;
    [SerializeField] private Transform nodeParent;
    [NonSerialized] public List<TowerBehavior> builtTowers;
    [NonSerialized] public Vector3[] nodePositions;
    [NonSerialized] public float[] nodeDistances;
    [SerializeField] TextMeshProUGUI waveText;
    [SerializeField] Player player;

    [NonSerialized] public bool autoStart;

    EnemySpawner enemySpawner;
    [NonSerialized] public bool waveActive;
    private bool endOfWave;
    int currentWave;
    int[] nextSpawnPoints;
    [NonSerialized] public GameObject SelectedTower;
    [NonSerialized] public float farmBonus;
    [NonSerialized] public bool showPaths;

    private bool beginWave;

    [NonSerialized] public float interestPercent;

    [SerializeField] private Sprite autoStartWaveOn;
    [SerializeField] private Sprite autoStartWaveOff;
    [SerializeField] private Sprite showPathOn;
    [SerializeField] private Sprite showPathOff;

    [SerializeField] AudioData audioData;

    public DrawPath[] PathIndicators;

    void Start()
    {
        enemySpawner = EnemySpawner.Instance;
        endLoop = false;
        endOfWave = true;
        enemyQueueToSpawn = new();
        enemyQueueToRemove = new();
        towerQueueToRemove = new();
        damageData = new();
        buffAddQueue = new();
        buffRemoveQueue = new();
        enemyBuffAddQueue = new();
        enemyBuffRemoveQueue = new();
        builtTowers = new List<TowerBehavior>();
        enemySpawner.Init();
        nextSpawnPoints = new int[] {0};
        autoStart = false;
        waveActive = false;
        showPaths = false;
        beginWave = false;
        interestPercent = 1;

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
        ActivatePathIndicators(nextSpawnPoints);

        AudioManager.Instance.CreateAudio()
            .WithAudioData(audioData)
            .WithPosition(gameObject.transform.position)
            .Play();
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
                    EnqueueEnemy(Enemy.EnemyType.Basic, 1, 0, false);
                    yield return new WaitForSeconds(1);
                }
                nextSpawnPoints = new int[] { 0 };
                break;
            case 1:
                for (int i = 0; i < 9; i++)
                {
                    EnqueueEnemy(Enemy.EnemyType.Basic, 1, 0, false);
                    yield return new WaitForSeconds(1);
                }
                nextSpawnPoints = new int[] { 0 };
                break;
            case 2:
                for (int i = 0; i < 5; i++)
                {
                    EnqueueEnemy(Enemy.EnemyType.Fast, 1, 0, false);
                    yield return new WaitForSeconds(0.5f);
                }
                nextSpawnPoints = new int[] { 0 };
                break;
            case 3:

                for (int i = 0; i < 5; i++)
                {
                    EnqueueEnemy(Enemy.EnemyType.Basic,1, 0, false);
                    yield return new WaitForSeconds(1);
                    EnqueueEnemy(Enemy.EnemyType.Spider, 1, 0, false);
                    yield return new WaitForSeconds(1);
                }
                nextSpawnPoints = new int[] { 0 };
                break;
            case 4:
                for (int i = 0; i < 6; i++)
                {
                    EnqueueEnemy(Enemy.EnemyType.Basic, 1, 0, false);
                    yield return new WaitForSeconds(1f);
                    EnqueueEnemy(Enemy.EnemyType.Basic, 1, 0, false);
                    yield return new WaitForSeconds(1f);
                    EnqueueEnemy(Enemy.EnemyType.Basic, 1, 0, false);
                    yield return new WaitForSeconds(1f);
                    EnqueueEnemy(Enemy.EnemyType.Fast, 1, 0, false);
                    yield return new WaitForSeconds(0.5f);
                    EnqueueEnemy(Enemy.EnemyType.Fast, 1, 0, false);
                    yield return new WaitForSeconds(.5f);
                }
                nextSpawnPoints = new int[] { 2 };
                break;
            case 5:
                for (int i = 0; i < 10; i++)
                {
                    EnqueueEnemy(Enemy.EnemyType.Slow, 1,2, false);
                    yield return new WaitForSeconds(1);
                }
                nextSpawnPoints = new int[] { 0,1 };
                break;
            case 6:
                for (int i = 0; i < 6; i++)
                {
                    EnqueueEnemy(Enemy.EnemyType.Slow, 1, 0, false);
                    yield return new WaitForSeconds(1);
                }
                for (int i = 0; i < 50; i++)
                {
                    EnqueueEnemy(Enemy.EnemyType.Fast, 1 ,1, false);
                    yield return new WaitForSeconds(.25f);
                }
                nextSpawnPoints = new int[] { 2 };
                break;
            case 7:
                for (int i = 0; i < 10; i++)
                {
                    EnqueueEnemy(Enemy.EnemyType.Slow, 1,2, false);
                    yield return new WaitForSeconds(1f);
                    EnqueueEnemy(Enemy.EnemyType.Slow, 1,2, false);
                    yield return new WaitForSeconds(0.25f);
                    EnqueueEnemy(Enemy.EnemyType.Slow, 1,2, false);
                    yield return new WaitForSeconds(0.5f);
                    EnqueueEnemy(Enemy.EnemyType.Slow,1, 2, false);
                    yield return new WaitForSeconds(0.1f);
                }
                nextSpawnPoints = new int[] { 0 };
                break;
            case 8:
                for (int i = 0; i < 50; i++)
                {
                    EnqueueEnemy(Enemy.EnemyType.Fast, 1,0, false);
                    yield return new WaitForSeconds(0.15f);
                }
                nextSpawnPoints = new int[] { 1,2 };
                break;
            case 9:
                for (int i = 0; i < 100; i++)
                {
                    EnqueueEnemy(Enemy.EnemyType.Slow, 1,1, false);
                    yield return new WaitForSeconds(.5f);
                }
                for (int i = 0; i < 50; i++)
                {
                    EnqueueEnemy(Enemy.EnemyType.Fast, 1,2, false);
                    EnqueueEnemy(Enemy.EnemyType.Basic, 1,2, false);
                    yield return new WaitForSeconds(0.1f);
                }
                nextSpawnPoints = new int[] { };
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

            /**
             
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
             */

            //Apply Enemy Buffs
            if (enemyBuffAddQueue.Count > 0)
            {
                int enemyBuffAddSize = enemyBuffAddQueue.Count;
                for (int i = 0; i < enemyBuffAddSize; i++)
                {
                    ApplyEnemyBuffData currentEnemyBuffData = enemyBuffAddQueue.Dequeue();

                    //Attempts to finds an identical buff. If found, do not add it to the list of buffs. Instead, increase its duration
                    EnemyBuff enemyBuffDuplicate = currentEnemyBuffData.enemyToAffect.activeBuffs.Find(
                        x => x.buffName == currentEnemyBuffData.buffToApply.buffName
                        && x.modifier == currentEnemyBuffData.buffToApply.modifier
                        && x.damage == currentEnemyBuffData.buffToApply.damage
                        && x.damageRate == currentEnemyBuffData.buffToApply.damageRate);
                    if (enemyBuffDuplicate == null)
                    {
                        currentEnemyBuffData.enemyToAffect.activeBuffs.Add(currentEnemyBuffData.buffToApply);
                    }
                    else
                    {
                        //If the duplicate buff has a longer duration, change the buff's duration into the new buff
                        if (enemyBuffDuplicate.duration < currentEnemyBuffData.buffToApply.duration)
                            enemyBuffDuplicate.duration = currentEnemyBuffData.buffToApply.duration;
                    }

                    currentEnemyBuffData.enemyToAffect.ApplyBuffs();
                }
            }

            //Removes Enemy Buffs
            if (enemyBuffRemoveQueue.Count > 0)
            {
                int enemyBuffRemoveSize = enemyBuffRemoveQueue.Count;
                for (int i = 0; i < enemyBuffRemoveSize; i++)
                {
                    ApplyEnemyBuffData currentBuffData = enemyBuffRemoveQueue.Dequeue();

                    Enemy enemyToAffect = currentBuffData.enemyToAffect;

                    //Finds an identical buff and removes it
                    for (int j = 0; j < enemyToAffect.activeBuffs.Count; j++)
                    {
                        if (enemyToAffect.activeBuffs[j].buffName == currentBuffData.buffToApply.buffName
                            && enemyToAffect.activeBuffs[j].modifier == currentBuffData.buffToApply.modifier
                            && enemyToAffect.activeBuffs[j].duration == currentBuffData.buffToApply.duration)
                        {
                            enemyToAffect.activeBuffs.RemoveAt(j);
                            currentBuffData.enemyToAffect.ApplyBuffs();
                            break;
                        }
                    }
                }
            }

            //Tick Enemies
            foreach (Enemy currentEnemy in enemySpawner.spawnedEnemies)
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
                    switch(currentDamageData.damageType)
                    {
                        /* Damage Formula:
                         * Damage Received = Total Damage * (100 / (100 + resistance))
                         * For every 100 resistance, their effective HP is doubled.
                         * Additionally, negative resistances are
                         * For example, if the enemy had a resistance of 100, they would take 0.5 damage
                         *              if the enemy had a resistance of -100, they would take 2.0 damage
                         */
                        
                        case DamageTypes.Sharp:

                            //Checks if the enemy should take more or less damage
                            if (targetedEnemy.sharpDamageResistance >= 0)
                            {
                                //Enemy Takes reduced damage
                                targetedEnemy.currentHealth = Mathf.Round((targetedEnemy.currentHealth -
                                    currentDamageData.totalDamage * (100 / (100 + targetedEnemy.sharpDamageResistance))) * 100f) / 100f; //Rounding removes floating point errors
                            }
                            else
                            {
                                //Enemy Takes More damage
                                targetedEnemy.currentHealth = Mathf.Round((targetedEnemy.currentHealth -
                                    currentDamageData.totalDamage * ((100 - targetedEnemy.sharpDamageResistance) / 100)) * 100f) / 100f; //Rounding removes floating point errors
                            }
                            break;
                        case DamageTypes.Fire:
                            //Checks if the enemy should take more or less damage
                            if (targetedEnemy.fireDamageResistance >= 0)
                            {
                                //Enemy Takes reduced damage
                                targetedEnemy.currentHealth = Mathf.Round((targetedEnemy.currentHealth -
                                    currentDamageData.totalDamage * (100 / (100 + targetedEnemy.fireDamageResistance))) * 100f) / 100f; //Rounding removes floating point errors
                            }
                            else
                            {
                                //Enemy Takes More damage
                                targetedEnemy.currentHealth = Mathf.Round((targetedEnemy.currentHealth -
                                    currentDamageData.totalDamage * ((100 - targetedEnemy.fireDamageResistance) / 100)) * 100f) / 100f; //Rounding removes floating point errors
                            }
                            break;
                        case DamageTypes.Ice:
                            //Checks if the enemy should take more or less damage
                            if (targetedEnemy.iceDamageResistance >= 0)
                            {
                                //Enemy Takes reduced damage
                                targetedEnemy.currentHealth = Mathf.Round((targetedEnemy.currentHealth -
                                    currentDamageData.totalDamage * (100 / (100 + targetedEnemy.iceDamageResistance))) * 100f) / 100f; //Rounding removes floating point errors
                            }
                            else
                            {
                                //Enemy Takes More damage
                                targetedEnemy.currentHealth = Mathf.Round((targetedEnemy.currentHealth -
                                    currentDamageData.totalDamage * ((100 - targetedEnemy.iceDamageResistance) / 100)) * 100f) / 100f; //Rounding removes floating point errors
                            }
                            break;
                        case DamageTypes.Explosion:
                            //Checks if the enemy should take more or less damage
                            if (targetedEnemy.explosionDamageResistance >= 0)
                            {
                                //Enemy Takes reduced damage
                                targetedEnemy.currentHealth = Mathf.Round((targetedEnemy.currentHealth -
                                    currentDamageData.totalDamage * (100 / (100 + targetedEnemy.explosionDamageResistance))) * 100f) / 100f; //Rounding removes floating point errors
                            }
                            else
                            {
                                //Enemy Takes More damage
                                targetedEnemy.currentHealth = Mathf.Round((targetedEnemy.currentHealth -
                                    currentDamageData.totalDamage * ((100 - targetedEnemy.explosionDamageResistance) / 100)) * 100f) / 100f; //Rounding removes floating point errors
                            }
                            break;
                        case DamageTypes.True:
                            targetedEnemy.currentHealth -= currentDamageData.totalDamage; //True Damage ignores all resistances
                            break;
                    }

                    targetedEnemy.GetComponentInChildren<HealthBar>().UpdateHealth((int) targetedEnemy.currentHealth);
                    if (targetedEnemy.currentHealth <= 0)
                    {
                        if (targetedEnemy.lastDamagingTower != null)
                        {
                            player.GiveMoney(targetedEnemy.moneyToPlayer * targetedEnemy.lastDamagingTower.moneyMultiplier); // Gives money to player
                            if (targetedEnemy.lastDamagingTower != null)
                            {
                                targetedEnemy.lastDamagingTower.numOfEnemiesKilled++; //Increases tower kill count
                                UpgradePanel.Instance.UpdateKillCount(targetedEnemy.lastDamagingTower.numOfEnemiesKilled);
                            }
                        }
                        else
                            player.GiveMoney(targetedEnemy.moneyToPlayer); // If enemy was defeated by a non-tower, gives normal amount of money
                        player.RegenMana(targetedEnemy.manaToPlayer);
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
                ActivatePathIndicators(nextSpawnPoints);
                print("Wave ended");
                print(nextSpawnPoints[0]);
                WaveBonus(currentWave);
                endOfWave = false;

                //auto start next wave if enabled
                if (autoStart)
                {
                    if (beginWave)
                        StartCoroutine(Wave(currentWave));
                    waveActive = true;
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

                    //Attempts to finds an identical buff. If found, do not add it to the list of buffs. Instead, increase its duration
                    Buff buffDuplicate = currentBuffData.towerToAffect.activeBuffs.Find(
                        x => x.buffName == currentBuffData.buffToApply.buffName
                        && x.modifier == currentBuffData.buffToApply.modifier);
                    if (buffDuplicate == null)
                    {
                        currentBuffData.towerToAffect.activeBuffs.Add(currentBuffData.buffToApply);
                    }
                    else
                    {
                        //If the duplicate buff has a longer duration, change the buff's duration into the new buff
                        if (buffDuplicate.duration < currentBuffData.buffToApply.duration)
                            buffDuplicate.duration = currentBuffData.buffToApply.duration;
                    }
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

    public void EnqueueDamageData(EnemyDamageData damageData)
    {
        this.damageData.Enqueue(damageData);
    }

    /// <summary>
    /// Enqueues enemies to spawn when the game allows it to
    /// </summary>
    /// <param name="enemyID"></param>
    public void EnqueueEnemy(Enemy.EnemyType type, int level, int spawnPointNumber, bool isInvisible)
    {
        int enemyID = level;

        //Converts type to level - this system is under the assumption that 10 or more variants of this type do not exist.
        switch (type)
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
            case Enemy.EnemyType.Spider:
                enemyID += 40;
                break;
            case Enemy.EnemyType.Boss1:
                enemyID += 50;
                break;
            case Enemy.EnemyType.Buffer:
                enemyID += 60;
                break;
            case Enemy.EnemyType.Boss2:
                enemyID += 70;
                break;
            case Enemy.EnemyType.Stealth:
                enemyID += 80;
                break;
            default:
                print("ERROR: FAILED TO GRAB ENEMY DATA");
                break;
        }

        enemyQueueToSpawn.Enqueue(new Tuple<int, int, bool>(enemyID, spawnPointNumber, isInvisible));

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

    public void EnqueueEnemyBuffToApply(ApplyEnemyBuffData buffData)
    {
        enemyBuffAddQueue.Enqueue(buffData);
    }

    public void EnqueueEnemyBuffToRemove(ApplyEnemyBuffData buffData)
    {
        enemyBuffRemoveQueue.Enqueue(buffData);
    }
    
    public class Buff
    {
        public BuffNames buffName;
        public float modifier;
        public float duration; //NOTE: A duration of -123 will not disappear until removed manually
        public bool isDebuff;
        public Buff(BuffNames buffName, float modifier, float duration, bool isDebuff)
        {
            this.buffName = buffName;
            this.modifier = modifier;
            this.duration = duration;
            this.isDebuff = isDebuff;
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

    public class EnemyBuff
    {
        public EnemyBuffNames buffName;
        public float damage;
        public float damageRate;
        public float modifier;
        public float duration; //NOTE: A duration of -123 will not disappear until removed manually
        public bool isDebuff;
        public TowerBehavior source;

        public EnemyBuff(EnemyBuffNames buffName, float damage, float damageRate, float modifier, float duration, bool isDebuff, TowerBehavior source)
        {
            this.damage = damage;
            this.damageRate = damageRate;
            this.buffName = buffName;
            this.modifier = modifier;
            this.duration = duration;
            this.isDebuff = isDebuff;
            this.source = source;
        }
    }

    public struct ApplyEnemyBuffData
    {
        public EnemyBuff buffToApply;
        public Enemy enemyToAffect;

        public ApplyEnemyBuffData(EnemyBuff buffToApply, Enemy enemyToAffect)
        {
            this.buffToApply = buffToApply;
            this.enemyToAffect = enemyToAffect;
        }
    }

    public struct EnemyDamageData
    {
        public Enemy targetedEnemy;
        public float totalDamage;
        public DamageTypes damageType;
        public TowerBehavior damageOrigin;
        public EnemyDamageData(Enemy targettedEnemy,  float totalDamage, DamageTypes damageType, TowerBehavior damageOrigin)
        {
            this.targetedEnemy = targettedEnemy;
            this.totalDamage = totalDamage;
            this.damageType = damageType;
            this.damageOrigin = damageOrigin;
        }
    }

    public enum BuffNames
    {
        SupportBonusRange,
        SupportBonusDamage,
        SupportBonusAttackSpeed,
        Stun,
        Investments,
        Taunt
    }

    public enum EnemyBuffNames
    {
        ResistanceEnemyBuffer,
        SpeedEnemyBuffer,
        Burn,
        Slow,
        Confuse
    }

    public void ToggleAutoStart(Image img)
    {
        autoStart = !autoStart;
        beginWave = autoStart;

        if (autoStart)
        {
            img.sprite = autoStartWaveOn;
        }
        else
        {
            img.sprite = autoStartWaveOff;
        }    
    }

    public void ToggleShowPaths(Image img)
    {
        showPaths = !showPaths;

        if (showPaths)
        {
            img.sprite = showPathOn;
        }
        else
        {
            img.sprite = showPathOff;
        }
    }

    //toggle next path indicator
    public void ActivatePathIndicators(int[] spawnPoints)
    {
        foreach (DrawPath ind in PathIndicators)
        {
            ind.currentWave = false;
        }

        foreach (int id in spawnPoints)
        {
            PathIndicators[id].currentWave = true;
            print("activated path indicator: " + id);
        }
    }

    public enum DamageTypes
    {
        Sharp,
        Fire,
        Ice,
        Explosion,
        True
    }
}