using System.Collections.Generic;
using UnityEngine;
using AudioSystem;
using System;
using TMPro;
using static TowerBehavior;

public class Enemy : MonoBehaviour
{
    public EnemyType type;
    public int maxHealth;
    public int moneyToPlayer;
    public int manaToPlayer;
    public float currentHealth;
    public float speed;
    [NonSerialized] public bool isAlive;
    public float currentSpeed { get; private set; }
    public bool isInvisible;

    [Header("Damage Resistances")]

    [Tooltip("How much resistance they have against sharp damage")]
    public float sharpDamageResistance;

    [Tooltip("How much resistance they have against fire damage")]
    public float fireDamageResistance;

    [Tooltip("How much resistance they have against ice damage")]
    public float iceDamageResistance;

    [Tooltip("How much resistance they have against explosion damage")]
    public float explosionDamageResistance;

    [NonSerialized] public int ID;
    [NonSerialized] public int nodeIndex;
    [NonSerialized] public List<GameManager.EnemyBuff> activeBuffs;
    [NonSerialized] public List<GameManager.EnemyBuff> appliedBuffs;
    GameManager gameManager;
    [NonSerialized] public bool isConfused;
    [SerializeField] AudioData audioMovement;
    [SerializeField] AudioData audioDead;
    [SerializeField] TextMeshProUGUI enemyText;
    [NonSerialized] public TowerBehavior lastDamagingTower;
    string[] buffNames;
    private int buffNamesCount;

    [NonSerialized] public bool isSlowed, isStunned;
    bool isBurning;
    float burnDamage;
    float burnDelay;
    float currentBurnDelay;
    TowerBehavior burnSource;

    private AudioEmitter audioEmitterMove;
    [NonSerialized] public NavMeshMovement navMeshMovement;
    public void Init()
    {
        gameManager = GameManager.Instance;
        isAlive = true;
        currentHealth = maxHealth;
        navMeshMovement = GetComponent<NavMeshMovement>();
        navMeshMovement.SetSpeed(speed);
        currentSpeed = speed;
        activeBuffs = new();
        appliedBuffs = new();
        buffNames = Enum.GetNames(typeof(GameManager.EnemyBuffNames));
        buffNamesCount = Enum.GetNames(typeof(GameManager.EnemyBuffNames)).Length;
        nodeIndex = 0;
        moneyToPlayer = 10;
        manaToPlayer = 10;
        isConfused = false;
        isSlowed = false;
        isStunned = false;
        isBurning = false;
        isInvisible = false;
        currentBurnDelay = 0;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        gameObject.GetComponentInChildren<HealthBar>().UpdateHealth((int)currentHealth);
        if (currentHealth <= 0)
        {
            AudioManager.Instance.CreateAudio()
                .WithAudioData(audioDead)
                .WithPosition(gameObject.transform.position)
                .Play();
            GameManager.Instance.EnqueueEnemyToRemove(this);
            Player.Instance.GiveMoney(moneyToPlayer);
            Player.Instance.RegenMana(manaToPlayer);
        }
    }

    public void Tick()
    {
        int activeBuffsCount = activeBuffs.Count;

        if (isBurning)
        {
            if (currentBurnDelay > 0)
            {
                currentBurnDelay -= Time.deltaTime;
            }
            else
            {
                lastDamagingTower = burnSource;
                gameManager.EnqueueDamageData(new GameManager.EnemyDamageData(this, burnDamage, GameManager.DamageTypes.Fire, burnSource));
                currentBurnDelay = burnDelay;
            }
        }

        for (int i = 0; i < activeBuffs.Count; i++)
        {
            if (activeBuffs[i] != null && activeBuffs[i].duration != -123) //Iterates through all buffs that do not have a duration of -123
            {
                activeBuffs[i].duration -= Time.deltaTime;

                //Removes all active buffs that do not have a duration of -123 and a duration lower than 0
                if (activeBuffs.RemoveAll(x => x.duration <= 0 && x.duration != -123) > 0)
                {
                    ApplyBuffs();
                }
            }
        }

        //Plays audio clip again once the clip ends
        if (audioEmitterMove == null || !audioEmitterMove.IsPlaying())
        {
            audioEmitterMove = AudioManager.Instance.CreateAudio().WithAudioData(audioMovement).WithPosition(gameObject.transform.position).Play();
        }
    }

    public void ApplyBuffs()
    {
        //Removes Applied Buffs
        for (int i = 0; i < buffNamesCount; i++)
        {
            //Removes previous buffs (if any)
            int appliedBuffsCount = appliedBuffs.Count;
            for (int j = 0; j < appliedBuffs.Count; j++)
            {
                if (appliedBuffs[j].buffName.ToString().Equals(buffNames[i]))
                {
                    if (appliedBuffs[j].modifier != 0) //Prevents divide by 0 error
                    {
                        switch (appliedBuffs[j].buffName)
                        {
                            case GameManager.EnemyBuffNames.ResistanceEnemyBuffer:
                                sharpDamageResistance -= appliedBuffs[j].modifier;
                                fireDamageResistance -= appliedBuffs[j].modifier;
                                iceDamageResistance -= appliedBuffs[j].modifier;
                                explosionDamageResistance -= appliedBuffs[j].modifier;
                                break;
                            case GameManager.EnemyBuffNames.SpeedEnemyBuffer:
                                SetSpeed(currentSpeed / appliedBuffs[j].modifier);
                                break;
                            case GameManager.EnemyBuffNames.Slow:
                                SetSpeed(currentSpeed / appliedBuffs[j].modifier);
                                isSlowed = false;
                                isStunned = false;
                                break;
                            case GameManager.EnemyBuffNames.Confuse:
                                navMeshMovement.FlipDirection(1);
                                break;
                        }
                    }
                }
            }
        }
        //Clears Applied Buffs
        appliedBuffs.Clear();

        //Gets the amount of buffs on the tower
        int activeBuffsCount = activeBuffs.Count;

        for (int i = 0; i < buffNamesCount; i++) //Iterates through every buff in the game
        {
            //(float, float, float, float) strongestBuff = (0, 0, Mathf.NegativeInfinity, 0); // (Damage, Modifier, duration, damage rate)
            GameManager.EnemyBuff strongestBuff = new GameManager.EnemyBuff(GameManager.EnemyBuffNames.ResistanceEnemyBuffer,
                0, 0, 0, Mathf.NegativeInfinity, false, null);
            GameManager.EnemyBuff buff = null; //Stores the strongest buff
            for (int j = 0; j < activeBuffsCount; j++) //Iterates through every buff on the tower
            {
                if (activeBuffs[j].buffName.ToString().Equals(buffNames[i])) //If the buff names match
                {
                    //TODO
                    if (activeBuffs[j].modifier > strongestBuff.modifier
                        || activeBuffs[j].damage > strongestBuff.damage) //Compares the modifier of the buff. Strongest modifier is the strongest buff
                    {
                        strongestBuff.damage = activeBuffs[j].damage;
                        strongestBuff.modifier = activeBuffs[j].modifier;
                        strongestBuff.duration = activeBuffs[j].duration;
                        strongestBuff.damageRate = activeBuffs[j].damageRate;
                        buff = activeBuffs[j];
                    }
                    //If the modifies are equal, compares the duration. Longest duration is the stronger buff
                    else if (activeBuffs[j].modifier == strongestBuff.modifier && activeBuffs[j].duration > strongestBuff.duration)
                    {
                        strongestBuff.damage = activeBuffs[j].damage;
                        strongestBuff.modifier = activeBuffs[j].modifier;
                        strongestBuff.duration = activeBuffs[j].duration;
                        strongestBuff.damageRate = activeBuffs[j].damageRate;
                        buff = activeBuffs[j];
                    }
                }
            }

            if (buff != null)
            {
                //Applies Buffs
                switch (buffNames[i])
                {
                    case "ResistanceEnemyBuffer":
                        appliedBuffs.Add(buff);
                        sharpDamageResistance += strongestBuff.modifier;
                        fireDamageResistance += strongestBuff.modifier;
                        iceDamageResistance += strongestBuff.modifier;
                        explosionDamageResistance += strongestBuff.modifier;
                        break;
                    case "SpeedEnemyBuffer":
                        appliedBuffs.Add(buff);
                        SetSpeed(currentSpeed * strongestBuff.modifier);
                        break;
                    case "Burn":
                        appliedBuffs.Add(buff);
                        isBurning = true;
                        burnDamage = strongestBuff.damage;
                        burnDelay = 1f / strongestBuff.damageRate;
                        break;
                    case "Slow":
                        appliedBuffs.Add(buff);
                        SetSpeed(currentSpeed * strongestBuff.modifier);

                        if (strongestBuff.modifier == 0.0001f)
                            isStunned = true;
                        else
                            isSlowed = true;
                        break;
                    case "Confuse":
                        appliedBuffs.Add(buff);
                        navMeshMovement.FlipDirection(0);
                        break;
                }
            }
        }
    }

    public void SetSpeed(float speed)
    {
        currentSpeed = speed;
        navMeshMovement.SetSpeed(currentSpeed);
    }

    public void SetInvisibility(bool invisible)
    {

        isInvisible = invisible;
        enemyText = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        if (isInvisible)
        {

            if (enemyText)
            {
                enemyText.text = $"Invisible";
            }

        }
        else
        {
            if (enemyText)
            {
                switch (type)
                {
                    case EnemyType.Basic:
                        enemyText.text = $"Basic";
                        break;
                    case EnemyType.Fast:
                        enemyText.text = $"Fast";
                        break;
                    case EnemyType.Slow:
                        enemyText.text = $"Slow";
                        break;
                    case EnemyType.Spider:
                        enemyText.text = $"Spider";
                        break;
                    case EnemyType.Boss1:
                        enemyText.text = $"Boss 1";
                        break;
                    case EnemyType.Buffer:
                        enemyText.text = $"Buffer";
                        break;
                    case EnemyType.Boss2:
                        enemyText.text = $"Boss 2";
                        break;
                    case EnemyType.Stealth:
                        enemyText.text = $"Stealth";
                        break;
                }
            }

        }


    }


    #region Enemy Data
    public struct EnemyData
    {
        public string name;
        public string description;
        public int cost;
        public string abilities;
        public string variants;
        public string weaknesses;
        public string resistance;
        public int hp;
        public int atk;
        public int def;
        public int spd;

        public EnemyData(
            string name,
            int cost,
            string description,
            string abilities,
            string variants,
            string weaknesses,
            string resistance,
            int hp,
            int atk,
            int def,
            int spd)
        {
            this.name = name;
            this.description = description;
            this.cost = cost;
            this.abilities = abilities;
            this.variants = variants;
            this.weaknesses = weaknesses;
            this.resistance = resistance;
            this.hp = hp;
            this.atk = atk;
            this.def = def;
            this.spd = spd;
        }
    }

    private static readonly Dictionary<EnemyType, EnemyData> EnemyInfo = new()
    {
        {
            EnemyType.Basic, new EnemyData(
                "Basic Robot",
                50,
                "A standard robot with balanced attributes.",
                "Basic Attack, Basic Defense",
                "Basic",
                "Low Speed",
                "Basic Resistance",
                100,
                50,
                50,
                30)
        },
        {
            EnemyType.Fast, new EnemyData(
                "Roller Robot",
                150,
                "A fast robot with high speed and moderate attack.",
                "Speed Boost, Quick Strike",
                "Fast",
                "Low Defense",
                "High Speed",
                80,
                70,
                40,
                90)
        },
        {
            EnemyType.Slow, new EnemyData(
                "Tank Robot",
                150,
                "A heavily armored robot with high defense and hit points.",
                "Strong Defense, Heavy Attack",
                "Tank",
                "Low Speed",
                "High Armor",
                150,
                80,
                100,
                20)
        },
        {
            EnemyType.Spider, new EnemyData(
                "Spider Robot",
                300,
                "text text text text text text text text text text",
                "text text text text",
                "Spider",
                "Low Attack",
                "High Agility",
                70,
                45,
                55,
                85)
        },
        {
            EnemyType.Buffer, new EnemyData(
                "Stealth Robot",
                100,
                "text text text text text text text text text text",
                "text text text text",
                "Stealth",
                "Low HP",
                "High Evasion",
                60,
                65,
                35,
                80)
        },
        {
            EnemyType.Stealth, new EnemyData(
                "Walker Robot",
                0,
                "text text text text text text text text text text",
                "text text text text",
                "Walker",
                "None",
                "Balanced",
                90,
                60,
                60,
                60)
        },
        {
            EnemyType.Boss1, new EnemyData(
                "Boss Robot",
                150,
                "text text text text text text text text text text",
                "text text text text",
                "Boss",
                "None",
                "High Strength",
                200,
                100,
                100,
                40)
        }
    };

    public EnemyData GetEnemyData(EnemyType type)
    {
        return EnemyInfo[type];
    }


    #endregion

    public void ReachedEnd()
    {

    }

    public enum EnemyType
    {
        Basic,
        Fast,
        Slow,
        Spider,
        Buffer,
        Stealth,
        Boss1,
        Boss2
    }
}