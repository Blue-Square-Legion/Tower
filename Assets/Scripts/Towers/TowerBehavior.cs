using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static Cinemachine.DocumentationSortingAttribute;

public class TowerBehavior : MonoBehaviour
{
    [NonSerialized] public Enemy target;

    [SerializeField] Transform towerPivot;
    [SerializeField] LayerMask towerInfoMask;
    public LayerMask enemiesLayer;
    public float damage;
    public float fireRate;
    public float range;
    public int cost;
    public List<GameManager.Buff> activeBuffs;
    public List<GameManager.Buff> appliedBuffs;
    private float delay;
    private TowerPlacement towerPlacement;
    Camera cam;
    public bool isSelected;

    [SerializeField] TowerType towerType;
    [SerializeField] public TowerTargetting.TargetType targetType;
    private IDamageMethod currentDamageMethodClass;
    public Canvas canvas;
    private Text towerLevelText;
    private Player player;
    public int upgradeLevel;
    private UpgradePanel upgradePanel;
    private int upgradeCost;
    [NonSerialized] public int sellCost;
    private string upgradeDescription;

    string[] buffNames;
    private int buffNamesCount;

    GameObject lastSelectedTower;

    private void Start()
    {
        targetType = TowerTargetting.TargetType.First;
        towerPlacement = TowerPlacement.Instance;
        upgradePanel = UpgradePanel.Instance;
        player = Player.Instance;
        cam = towerPlacement.cam;
        isSelected = true;
        lastSelectedTower = null;
        activeBuffs = new();
        appliedBuffs = new();

        buffNames = Enum.GetNames(typeof(GameManager.BuffNames));
        buffNamesCount = Enum.GetNames(typeof(GameManager.BuffNames)).Length;
        
        //InstantiateTowerLevelText();
        currentDamageMethodClass = GetComponent<IDamageMethod>();

        if (currentDamageMethodClass == null )
        {
            Debug.LogError("ERROR: FAILED TO FIND A DAMAGE CLASS ON CURRENT TOWER!");
        }
        else
        {
            currentDamageMethodClass.Init(damage, fireRate);
        }

        delay = 1 / fireRate;

        upgradeLevel = 0;

        switch(towerType)
        {
            case TowerType.Basic:
                upgradeCost = 50;
                upgradeDescription = "Better Scopes\nIncreased Range";
                break;
            case TowerType.Bomb:
                upgradeCost = 100;
                upgradeDescription = "More Powerful Bombs\nIncreased Damage";
                break;
            case TowerType.Flame:
                upgradeCost = 100;
                upgradeDescription = "More Combustive Fuel\nIncreased Damage";
                break;
            case TowerType.Economy:
                upgradeCost = 300;
                upgradeDescription = "More Money\nGives a bit more money";
                break;
            case TowerType.Ice:
                upgradeCost = 100;
                upgradeDescription = "Increased detector\nIncreased Range";
                break;
            case TowerType.Support:
                upgradeCost = 100;
                upgradeDescription = "Command\nFurther Boosts Attack Range of nearby towers";
                break;
        }
        sellCost = cost / 2;
    }

    /* 
     private void InstantiateTowerLevelText()
    {
        Debug.Log("InstantiateTowerLevelText method called");

        if (towerLevelTextPrefab == null || canvas == null)
        {
            Debug.LogError("towerLevelTextPrefab or canvas is not assigned in the Inspector");
            return;
        }

        GameObject towerLevelTextInstance = Instantiate(towerLevelTextPrefab, canvas.transform);
        Debug.Log("towerLevelTextPrefab instantiated");

        TowerLevelDisplay floatingTextOverlay = towerLevelTextInstance.GetComponent<TowerLevelDisplay>();
        if (floatingTextOverlay == null)
        {
            Debug.LogError("FloatingTextOverlay component not found on the instantiated prefab");
            return;
        }

        floatingTextOverlay.target = transform; // Correct field assignment
        floatingTextOverlay.canvas = canvas; // Ensure the canvas is assigned
        Debug.Log("floatingTextOverlay.target assigned to " + transform.name);

        towerLevelText = towerLevelTextInstance.GetComponent<Text>();
        if (towerLevelText == null)
        {
            Debug.LogError("Text component not found on the instantiated prefab");
        }
        else
        {
            Debug.Log("Text component assigned: " + towerLevelText.text);
        }
    }
    */

//Desyncs the towers from regular game loop to prevent errors
public void Tick()
    {
        currentDamageMethodClass.damageTick(target); 
        
        if (target != null)
        {
            // Calculate the direction to the target
            Vector3 direction = target.transform.position - transform.position;

            // Set the y component to 0 to ignore vertical differences
            direction.y = 0;

            // Calculate the rotation
            if (direction != Vector3.zero) // Ensure direction is not zero to avoid errors
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                towerPivot.transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y + 180, 0);
            }
        }
        // Create a pointer event for UI detection
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        // List of raycast results for UI
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        // If any UI elements were hit, ignore the raycast for game objects
        if (results.Count > 0)
        {
            return; // Exit if UI is clicked
        }

        //Ray casts from screen to mouse
        Ray camRay = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        //Gets data from raycast
        if (Physics.Raycast(camRay, out hitInfo, 100f, towerInfoMask))
        {
            //If tower was clicked
            if (Input.GetMouseButtonDown(0))
            {
                if (Input.GetMouseButtonDown(0) && hitInfo.collider.gameObject == gameObject)
                {
                    isSelected = !isSelected;
                    if (isSelected)
                    {
                        lastSelectedTower = hitInfo.collider.gameObject;
                        GameManager.Instance.SelectedTower = gameObject;
                    }
                    else
                    {
                        upgradePanel.SetUpgradePanel(false);
                    }
                }  
                else
                {
                    isSelected = false;
                }
                    
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            isSelected = false;
            upgradePanel.SetUpgradePanel(false);
        }
            
        gameObject.transform.Find("Range").gameObject.SetActive(isSelected);

        if (lastSelectedTower != null)
        {
            upgradePanel.SetUpgradePanel(lastSelectedTower.transform.Find("Range").gameObject.activeInHierarchy);
            UpdateUpgradePanel();
            lastSelectedTower = null;
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
                            case GameManager.BuffNames.SupportBonusRange:
                                range /= appliedBuffs[j].modifier;
                                break;
                            case GameManager.BuffNames.SupportBonusAttackSpeed:
                                fireRate /= appliedBuffs[j].modifier;
                                break;
                            case GameManager.BuffNames.SupportBonusDamage:
                                damage /= appliedBuffs[j].modifier;
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
            (float, float) strongestBuff = (0, Mathf.NegativeInfinity); // (Modifier, duration)
            GameManager.Buff buff = null; //Stores the strongest buff
            for (int j = 0; j < activeBuffsCount; j++) //Iterates through every buff on the tower
            {
                if (activeBuffs[j].buffName.ToString().Equals(buffNames[i])) //If the buff names match
                {
                    if (activeBuffs[j].modifier > strongestBuff.Item1) //Compares the modifier of the buff. Strongest modifier is the strongest buff
                    {
                        strongestBuff.Item1 = activeBuffs[j].modifier;
                        strongestBuff.Item2 = activeBuffs[j].duration;
                        buff = activeBuffs[j];
                    }
                    else if (activeBuffs[j].modifier == strongestBuff.Item1 && activeBuffs[j].duration > strongestBuff.Item2) //If the modifies are equal, compares the duration. Longest duration is the stronger buff
                    {
                        strongestBuff.Item1 = activeBuffs[j].modifier;
                        strongestBuff.Item2 = activeBuffs[j].duration;
                        buff = activeBuffs[j];
                    }
                }
            }
            print("Strongest Buff " + buffNames[i] + " - Modification: " + strongestBuff.Item1);
            if (buff != null)
            {
                //Applies Buffs
                switch (buffNames[i])
                {
                    case "SupportBonusRange":
                        range *= strongestBuff.Item1;
                        appliedBuffs.Add(buff);
                        break;
                    case "SupportBonusAttackSpeed":
                        fireRate *= strongestBuff.Item1;
                        appliedBuffs.Add(buff);
                        break;
                    case "SupportBonusDamage":
                        damage *= strongestBuff.Item1;
                        appliedBuffs.Add(buff);
                        break;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(towerPivot.position, range);
        if (target != null)
            Gizmos.DrawWireCube(target.transform.position, new Vector3(.5f, .5f, .5f));
    }

    public void Upgrade()
    {
        if (player.GetMoney() >= upgradeCost)
        {
            Transform rangeObject = transform.Find("Range");
            player.RemoveMoney(upgradeCost);
            switch (towerType)
            {
                case TowerType.Basic:
                    switch (upgradeLevel)
                    {
                        case 0:
                            //Do upgrade
                            range += 1f;
                            rangeObject.localScale = new Vector3(rangeObject.localScale.x + 2f, rangeObject.localScale.y, rangeObject.localScale.z + 2f);

                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;
                            upgradeCost = GetUpgradeCost(upgradeLevel);
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);
                            break;
                        case 1:
                            //Do upgrade
                            damage += 1;
                            transform.GetComponent<StandardDamage>().UpdateDamage(damage);
                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;
                            upgradeCost = GetUpgradeCost(upgradeLevel);
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);
                            break;
                        case 2:
                            //Do upgrade
                            fireRate += 1;
                            transform.GetComponent<StandardDamage>().UpdateFireRate(fireRate);

                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;
                            upgradeCost = GetUpgradeCost(upgradeLevel);
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);
                            break;
                        case 3:
                            //Do upgrade
                            range += 1.7f;
                            rangeObject.localScale = new Vector3(rangeObject.localScale.x + 3.4f, rangeObject.localScale.y, rangeObject.localScale.z + 3.4f);

                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;
                            upgradeCost = GetUpgradeCost(upgradeLevel);
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);
                            break;
                        case 4:
                            //Do Upgrade
                            fireRate *= 2;
                            transform.GetComponent<StandardDamage>().UpdateFireRate(fireRate);

                            //No more upgrades
                            sellCost += upgradeCost / 2;
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);
                            break;
                    }
                    break;
                case TowerType.Bomb:
                    switch (upgradeLevel)
                    {
                        case 0:
                            //Do upgrade
                            damage += 1f;
                            transform.GetComponent<MissileDamage>().UpdateDamage(damage);

                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;
                            upgradeCost = GetUpgradeCost(upgradeLevel);
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);
                            break;
                        case 1:
                            //Do upgrade
                            transform.GetComponent<MissileDamage>().explosionRadius += 1;

                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;
                            upgradeCost = GetUpgradeCost(upgradeLevel);
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);
                            break;
                        case 2:
                            //Do upgrade
                            range += .25f;
                            rangeObject.localScale = new Vector3(rangeObject.localScale.x + .5f, rangeObject.localScale.y, rangeObject.localScale.z + .5f);

                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;
                            upgradeCost = 500;
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);
                            break;
                        case 3:
                            //Do upgrade
                            damage += 1f;
                            transform.GetComponent<MissileDamage>().explosionRadius += 1;
                            transform.GetComponent<MissileDamage>().UpdateDamage(damage);


                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;
                            upgradeCost = GetUpgradeCost(upgradeLevel);
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);
                            break;
                        case 4:
                            //Do Upgrade
                            fireRate *= 2;
                            transform.GetComponent<MissileDamage>().UpdateFireRate(fireRate);

                            //No more upgrades
                            sellCost += upgradeCost / 2;
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);
                            break;
                    }
                    break;
                case TowerType.Flame:
                    switch (upgradeLevel)
                    {
                        case 0:
                            //Do upgrade
                            damage += 0.05f;
                            transform.GetComponent<FireDamage>().UpdateDamage(damage);
                            transform.GetComponentInChildren<FireTriggerCollisionDetector>().duration += 1;
                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;
                            upgradeCost = GetUpgradeCost(upgradeLevel);
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);
                            break;
                        case 1:
                            //Do upgrade
                            range += 1f;
                            Transform fireTrigger = transform.Find("Head").transform.Find("FireTriggerPivot").transform.Find("FireTrigger").transform;
                            fireTrigger.localScale = new Vector3(fireTrigger.localScale.x + 1f, fireTrigger.localScale.y, fireTrigger.localScale.z - 0.5f);
                            fireTrigger.position = new Vector3(fireTrigger.position.x, fireTrigger.position.y, fireTrigger.position.z + 0.5f);
                            rangeObject.localScale = new Vector3(rangeObject.localScale.x + 2f, rangeObject.localScale.y, rangeObject.localScale.z + 2f);
                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;

                            upgradeCost = GetUpgradeCost(upgradeLevel);
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);

                            break;
                        case 2:
                            //Do upgrade
                            damage += 0.1f;

                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;
                            upgradeCost = GetUpgradeCost(upgradeLevel);
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);
                            break;
                        case 3:
                            //Do upgrade
                            transform.GetComponentInChildren<FireTriggerCollisionDetector>().duration += 2;

                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;

                            upgradeCost = GetUpgradeCost(upgradeLevel);
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);

                            break;
                        case 4:
                            //Do Upgrade
                            fireRate += 1;

                            //No more upgrades
                            sellCost += upgradeCost / 2;
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);
                            break;
                    }
                    break;
                case TowerType.Economy:
                    switch (upgradeLevel)
                    {
                        case 0:
                            transform.GetComponent<EconomyBehavior>().bonus = 100;
                            GameManager.Instance.farmBonus += 50;
                            sellCost += upgradeCost / 2;
                            upgradeCost = GetUpgradeCost(upgradeLevel);
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);
                            break;
                        case 1:
                            transform.GetComponent<EconomyBehavior>().bonus = 200;
                            GameManager.Instance.farmBonus += 100;
                            sellCost += upgradeCost / 2;
                            upgradeCost = GetUpgradeCost(upgradeLevel);
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);
                            break;
                        case 2:
                            transform.GetComponent<EconomyBehavior>().bonus = 400;
                            GameManager.Instance.farmBonus += 300;
                            sellCost += upgradeCost / 2;
                            upgradeCost = GetUpgradeCost(upgradeLevel);
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);
                            break;
                        case 3:
                            transform.GetComponent<EconomyBehavior>().bonus = 500;
                            GameManager.Instance.farmBonus += 100;
                            sellCost += upgradeCost / 2;
                            upgradeCost = GetUpgradeCost(upgradeLevel);
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);
                            break;
                        case 4:
                            transform.GetComponent<EconomyBehavior>().bonus = 750;
                            GameManager.Instance.farmBonus += 250;
                            sellCost += upgradeCost / 2;

                            //No more upgrades
                            sellCost += upgradeCost / 2;
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);
                            break;
                    }
                    break;
                case TowerType.Ice:
                    IceDamage tempIce = transform.gameObject.GetComponent<IceDamage>();
                    switch (upgradeLevel)
                    {
                        case 0:
                            //Do upgrade
                            range += 1f;
                            rangeObject.localScale = new Vector3(rangeObject.localScale.x + 2f, rangeObject.localScale.y, rangeObject.localScale.z + 2f);

                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;
                            upgradeCost = GetUpgradeCost(upgradeLevel);
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);
                            break;
                        case 1:

                            fireRate += 0.25f;
                            transform.GetComponent<IceDamage>().UpdateFireRate(fireRate);

                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;
                            upgradeCost = GetUpgradeCost(upgradeLevel);
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);
                            break;
                        case 2:
                            tempIce.UpdateSnowSpeed(tempIce.GetSnowSpeed() - 0.1f);

                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;
                            upgradeCost = GetUpgradeCost(upgradeLevel);
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);
                            break;
                        case 3:
                            tempIce.UpdateSnowDuration(tempIce.GetSnowDuration() + 0.5f);

                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;
                            upgradeCost = GetUpgradeCost(upgradeLevel);
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);
                            break;
                        case 4:
                            tempIce.UpdateSnowSize(tempIce.GetSnowSize() + 1f);

                            //No more upgrades
                            sellCost += upgradeCost / 2;
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);
                            break;
                    }
                    break;
                case TowerType.Support:
                    SupportBehavior support = transform.gameObject.GetComponent<SupportBehavior>();
                    switch (upgradeLevel)
                    {
                        case 0:
                            //Do upgrade
                            support.RemoveBuffs();
                            support.attackRangeBuff += 0.15f;
                            range += 0.5f;
                            rangeObject.localScale = new Vector3(rangeObject.localScale.x + 1f, rangeObject.localScale.y, rangeObject.localScale.z + 1f);
                            support.UpdateTowersInRange();
                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;
                            upgradeCost = GetUpgradeCost(upgradeLevel);
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);
                            break;
                        case 1:
                            //Hidden
                            //TODO
                            support.UpdateTowersInRange();

                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;
                            upgradeCost = GetUpgradeCost(upgradeLevel);
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);
                            break;
                        case 2:
                            //Do upgrade
                            support.fireRateBuff += 1.2f;
                            support.UpdateTowersInRange();

                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;
                            upgradeCost = GetUpgradeCost(upgradeLevel);
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);
                            break;
                        case 3:
                            //Do upgrade
                            support.damageBuff += 1.25f;
                            range += 0.5f;
                            rangeObject.localScale = new Vector3(rangeObject.localScale.x + 1f, rangeObject.localScale.y, rangeObject.localScale.z + 1f);
                            support.UpdateTowersInRange();

                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;
                            upgradeCost = GetUpgradeCost(upgradeLevel);
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);
                            break;
                        case 4:
                            //Do Upgrade
                            support.attackRangeBuff += 0.25f;
                            support.fireRateBuff += 0.1f;
                            support.damageBuff += .15f;
                            support.UpdateTowersInRange();

                            //No more upgrades
                            sellCost += upgradeCost / 2;
                            upgradeDescription = GetUpgradeDescription(upgradeLevel);
                            break;
                    }
                    break;
            }
            upgradeLevel++;
            UpdateUpgradePanel();
        }
    }


    public void UpdateUpgradePanel()
    {
        upgradePanel.SetTarget(this, (int)targetType);
        upgradePanel.SetUpgradeButton(upgradeCost);
        upgradePanel.SetSellButton(sellCost);
        upgradePanel.SetText(upgradeDescription);
        upgradePanel.ToggleUpgradeButton(upgradeLevel != 5);
    }

    public struct UpgradeData
    {
        public string description;
        public int cost;

        public UpgradeData(string description, int cost)
        {
            this.description = description;
            this.cost = cost;
        }
    }


    private static readonly Dictionary<TowerType, List<UpgradeData>> upgradeDataMap = new Dictionary<TowerType, List<UpgradeData>>()
    {
        //Description and Cost
        {
            TowerType.Basic, new List<UpgradeData>
            {
                new UpgradeData("Better Arrows\nIncreased Damage", 150),
                new UpgradeData("Faster mechanism\nIncreased fire rate", 300),
                new UpgradeData("Scopier Scopes\nIncreased Range", 300),
                new UpgradeData("Best Efficiency\nIncreased fire rate", 500),
                new UpgradeData("Max Level", 0)
            }
        },
        {
            TowerType.Bomb, new List<UpgradeData>
            {
                new UpgradeData("Bigger Bombs\nIncreased Explosion Radius", 200),
                new UpgradeData("Binoculars\nIncreased range", 300),
                new UpgradeData("Bigger Bombs\nIncreased Damage\nIncreased Explosion Radius", 500),
                new UpgradeData("Second Bomb Dropper\nIncreased fire rate", 500),
                new UpgradeData("Max Level", 0)
            }
        },
        {
            TowerType.Flame, new List<UpgradeData>
            {
                new UpgradeData("Stronger Propellent\nIncreased Range", 150),
                new UpgradeData("Better Fuel\nIncreased slow effect", 200),
                new UpgradeData("Long Lasting Burns\nIncreased Duration", 250),
                new UpgradeData("Even Stronger Fuel\nIncreased Slow Effect", 400),
                new UpgradeData("Max Level", 0)
            }
        },
        {
            TowerType.Economy, new List<UpgradeData>
            {
                new UpgradeData("Upgrade 1\nIncreased Money", 500),
                new UpgradeData("Upgrade 2\nEven more Money", 750),
                new UpgradeData("Upgrade 3\nExtra Money", 1000),
                new UpgradeData("Future's Market\nGrants a large sum of money", 1750),
                new UpgradeData("Max Level", 0)
            }
        },
        {
            TowerType.Ice, new List<UpgradeData>
            {
                new UpgradeData("Better Firing System\nIncreased Fire Rate", 150),
                new UpgradeData("Colder Snow\nSlows Enemies More", 350),
                new UpgradeData("Melt Resistant Snow\nIncreased Duration", 500),
                new UpgradeData("Larger Snowballs\nIncreased Snow Area", 600),
                new UpgradeData("Max Level", 0)
            }
        },
        {
            TowerType.Support, new List<UpgradeData>
            {
                new UpgradeData("Proximity Sensor (Not Implemented Yet)\nAll Towers in range can see Invisible Enemies", 0),
                new UpgradeData("Inspiration\nAll Towers in range gain a attack speed buff", 300),
                new UpgradeData("Command Center\nAll Towers in range do increased damage. Slightly Increased Range", 500),
                new UpgradeData("Motivation\nIncreases the effectiveness of this tower's buffs", 1000),
                new UpgradeData("Max Level", 0)
            }
        }
    };


    public void SetTargetType(int typeIndex)
    {
        Debug.Log("typeIndex" + typeIndex);
        if (Enum.IsDefined(typeof(TowerTargetting.TargetType), typeIndex))
        {
            targetType = (TowerTargetting.TargetType)typeIndex;
        }
        else
        {
            Debug.LogError("Invalid target type index: " + typeIndex);
        }
    }



    public UpgradeData GetUpgradeData(int level) {
        if (level < 0 || level >= upgradeDataMap[towerType].Count)
        {
            return new UpgradeData("Invalid Level", 0);
        }
        return upgradeDataMap[towerType][level]; 
    }

    public int GetMaxUpgradeLevel() { 

        return upgradeDataMap[towerType].Count - 1;

    }
    public string GetUpgradeDescription(int level) { 

        return GetUpgradeData(level).description; 

    }
    public int GetUpgradeCost(int level) { 

        return GetUpgradeData(level).cost; 

    }

    public enum TowerType
    {
        Basic,
        Flame,
        Bomb,
        Economy,
        Ice,
        Support
    }
}