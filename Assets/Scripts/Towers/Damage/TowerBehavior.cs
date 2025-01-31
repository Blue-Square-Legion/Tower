using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

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
    [NonSerialized] public List<GameManager.Buff> activeBuffs;
    [NonSerialized] public List<GameManager.Buff> appliedBuffs;
    [NonSerialized] private float delay;
    private TowerPlacement towerPlacement;
    Camera cam;
    [NonSerialized] public bool isSelected;
    public bool isStunned;
    public bool isTaunted;
    public Enemy tauntTarget;

    public GameObject StunPFX;

    [SerializeField] public TowerType towerType;
    [SerializeField] public TowerTargetting.TargetType targetType;
    private TowerDamage currentDamageMethodClass;
    private Player player;
    [NonSerialized] public int upgradeLevel1, upgradeLevel2, upgradeLevel3;
    private UpgradePanel upgradePanel;
    private int upgradeCost1, upgradeCost2, upgradeCost3;
    [NonSerialized] public int sellCost;
    private string upgradeDescription1, upgradeDescription2, upgradeDescription3;
    [SerializeField] TextMeshProUGUI leveltext;
    string[] buffNames;
    private int buffNamesCount;
    public float moneyMultiplier;

    GameObject lastSelectedTower;

    private bool isPath1Restricted;
    private bool isPath2Restricted;
    private bool isPath3Restricted;

    [NonSerialized] public int numOfEnemiesKilled;
    private void Start()
    {
        targetType = TowerTargetting.TargetType.First;
        towerPlacement = TowerPlacement.Instance;
        upgradePanel = UpgradePanel.Instance;
        player = Player.Instance;
        cam = towerPlacement.cam;
        isSelected = true;
        isStunned = false;
        isTaunted = false;
        lastSelectedTower = null;
        activeBuffs = new();
        appliedBuffs = new();

        buffNames = Enum.GetNames(typeof(GameManager.BuffNames));
        buffNamesCount = Enum.GetNames(typeof(GameManager.BuffNames)).Length;
        moneyMultiplier = 1;

        currentDamageMethodClass = GetComponent<TowerDamage>();

        if (currentDamageMethodClass == null)
        {
            Debug.LogError("ERROR: FAILED TO FIND A DAMAGE CLASS ON CURRENT TOWER!");
        }
        else
        {
            currentDamageMethodClass.Init(damage, fireRate);
        }

        delay = 1 / fireRate;

        upgradeLevel1 = 1;
        upgradeLevel2 = 1;
        upgradeLevel3 = 1;

        isPath1Restricted = false;
        isPath2Restricted = false;
        isPath3Restricted = false;

        upgradeDescription1 = GetUpgradeData(0, 1).name;
        upgradeCost1 = GetUpgradeData(0, 1).cost;
        upgradeDescription2 = GetUpgradeData(0, 2).name;
        upgradeCost2 = GetUpgradeData(0, 2).cost;
        upgradeDescription3 = GetUpgradeData(0, 3).name;
        upgradeCost3 = GetUpgradeData(0, 3).cost;

        sellCost = cost / 2;

        leveltext = gameObject.GetComponentInChildren<TextMeshProUGUI>();

        if (leveltext != null) {
            leveltext.text = $"{upgradeLevel1}-{upgradeLevel2}-{upgradeLevel3}"; 
            Debug.Log("Text updated successfully!"); 
        }

        numOfEnemiesKilled = 0;
    }

    //Desyncs the towers from regular game loop to prevent errors
    public void Tick()
    {
        if (isStunned)
        {
            StunPFX.SetActive(true);
        } else
        {
            StunPFX.SetActive(false);
            currentDamageMethodClass.DamageTick(target);
        }
            

        if (target && !isStunned)
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
            UIManager.Instance.UpdateUpgradeScreen(this);
            lastSelectedTower = null;
        }
    }

    public void TickBuffs()
    {
        int activeBuffsCount = activeBuffs.Count;
        for (int i = 0; i < activeBuffsCount; i++)
        {
            if (activeBuffs[i] != null && activeBuffs[i].duration != -123) //Iterates through all buffs that do not have a duration of -123
            {
                if (activeBuffs[i].duration > 0f)
                {
                    if (activeBuffs[i].buffName == GameManager.BuffNames.Stun) //If stun "buff" duration is greater than 0, keep the tower stunned
                    {
                        isStunned = true;
                        activeBuffs[i].duration -= Time.deltaTime;
                    }
                    if (activeBuffs[i].buffName == GameManager.BuffNames.Taunt) //If taunt "buff" duration is greater than 0, keep the tower taunted
                    {
                        isTaunted = true;
                        activeBuffs[i].duration -= Time.deltaTime;
                    }
                }
                else
                {
                    if (activeBuffs[i].buffName == GameManager.BuffNames.Stun) //Unstun tower when stun buff is over
                        isStunned = false;
                    if (activeBuffs[i].buffName == GameManager.BuffNames.Taunt) //Untaunt tower when taunt buff is over
                        isTaunted = false;
                }
                activeBuffs.RemoveAll(x => x.duration <= 0 && x.duration != -123); //Removes all active buffs that do not have a duration of -123 and a duration lower than 0
            }
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
                            case GameManager.BuffNames.Investments:
                                moneyMultiplier /= appliedBuffs[j].modifier;
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
            print("Strongest Buff = " + buff + " | Modifier = " + strongestBuff.Item1);
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
                    case "Investments":
                        moneyMultiplier *= strongestBuff.Item1;
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

    #region Upgrades
    #region Path 1
    public void Upgrade1()
    {
        if (player.GetMoney() >= upgradeCost1)
        {
            Transform rangeObject = transform.Find("Range");
            player.RemoveMoney(upgradeCost1);
            switch (towerType)
            {
                case TowerType.Crossbow:
                    switch (upgradeLevel1)
                    {
                        case 1:
                            //Do upgrade
                            range += 1f;
                            rangeObject.localScale = new Vector3(rangeObject.localScale.x + 2f, rangeObject.localScale.y, rangeObject.localScale.z + 2f);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeCost1 = GetUpgradeCost(upgradeLevel1, 1);
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);

                            if (upgradeLevel2 > 1)
                            {
                                isPath3Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            else if (upgradeLevel3 > 1)
                            {
                                isPath2Restricted = true;
                                upgradeDescription3 = "Path Restricted";
                            }
                            break;
                        case 2:
                            //Do upgrade
                            range += 2f;
                            rangeObject.localScale = new Vector3(rangeObject.localScale.x + 4f, rangeObject.localScale.y, rangeObject.localScale.z + 4f);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeCost1 = GetUpgradeCost(upgradeLevel1, 1);
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);

                            if (upgradeLevel2 == 4 || upgradeLevel3 == 4)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            break;
                        case 3:
                            //Do upgrade
                            range += 3f;
                            rangeObject.localScale = new Vector3(rangeObject.localScale.x + 6f, rangeObject.localScale.y, rangeObject.localScale.z + 6f);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);

                            if (upgradeLevel2 == 3)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            if (upgradeLevel3 == 3)
                            {
                                isPath3Restricted = true;
                                upgradeDescription3 = "Path Restricted";
                            }
                            break;
                    }
                    break;
                case TowerType.Cannon:
                    switch (upgradeLevel1)
                    {
                        case 1:
                            //Do upgrade
                            damage += 1f;
                            transform.GetComponent<MissileDamage>().UpdateDamage(damage);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeCost1 = GetUpgradeCost(upgradeLevel1, 1);
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);

                            if (upgradeLevel2 > 1)
                            {
                                isPath3Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            else if (upgradeLevel3 > 1)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            break;
                        case 2:
                            //Do upgrade
                            damage += 2f;
                            transform.GetComponent<MissileDamage>().UpdateDamage(damage);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeCost1 = GetUpgradeCost(upgradeLevel1, 1);
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);

                            if (upgradeLevel2 == 4 || upgradeLevel3 == 4)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            break;
                        case 3:
                            //Do upgrade
                            damage += 3f;
                            transform.GetComponent<MissileDamage>().UpdateDamage(damage);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);

                            if (upgradeLevel2 == 3)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            if (upgradeLevel3 == 3)
                            {
                                isPath3Restricted = true;
                                upgradeDescription3 = "Path Restricted";
                            }
                            break;
                    }
                    break;
                case TowerType.Flame:
                    switch (upgradeLevel1)
                    {
                        case 1:
                            //Do upgrade
                            fireRate += 0.2f;
                            transform.GetComponent<FireDamage>().UpdateFireRate(fireRate);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeCost1 = GetUpgradeCost(upgradeLevel1, 1);
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);

                            if (upgradeLevel2 > 1)
                            {
                                isPath3Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            else if (upgradeLevel3 > 1)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            break;
                        case 2:
                            //Do upgrade
                            fireRate += 0.3f;
                            transform.GetComponent<FireDamage>().UpdateFireRate(fireRate);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;

                            upgradeCost1 = GetUpgradeCost(upgradeLevel1, 1);
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);

                            if (upgradeLevel2 == 4 || upgradeLevel3 == 4)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            break;
                        case 3:
                            //Do upgrade
                            fireRate += 1f;
                            transform.GetComponent<FireDamage>().UpdateFireRate(fireRate);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);

                            if (upgradeLevel2 == 3)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            if (upgradeLevel3 == 3)
                            {
                                isPath3Restricted = true;
                                upgradeDescription3 = "Path Restricted";
                            }
                            break;
                    }
                    break;
                case TowerType.Mine:
                    switch (upgradeLevel1)
                    {
                        case 1:
                            transform.GetComponent<EconomyBehavior>().bonus = 100;
                            GameManager.Instance.farmBonus += 50;
                            sellCost += upgradeCost1 / 2;
                            upgradeCost1 = GetUpgradeCost(upgradeLevel1, 1);
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);

                            if (upgradeLevel2 > 1)
                            {
                                isPath3Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            else if (upgradeLevel3 > 1)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            break;
                        case 2:
                            transform.GetComponent<EconomyBehavior>().bonus = 200;
                            GameManager.Instance.farmBonus += 100;
                            sellCost += upgradeCost1 / 2;
                            upgradeCost1 = GetUpgradeCost(upgradeLevel1, 1);
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);

                            if (upgradeLevel2 == 4 || upgradeLevel3 == 4)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            break;
                        case 3:
                            transform.GetComponent<EconomyBehavior>().bonus = 400;
                            GameManager.Instance.farmBonus += 300;

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);

                            if (upgradeLevel2 == 3)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            if (upgradeLevel3 == 3)
                            {
                                isPath3Restricted = true;
                                upgradeDescription3 = "Path Restricted";
                            }
                            break;
                    }
                    break;
                case TowerType.Snowball:
                    IceDamage tempIce = transform.gameObject.GetComponent<IceDamage>();
                    switch (upgradeLevel1)
                    {
                        case 1:
                            //Do upgrade
                            range += 1.5f;
                            rangeObject.localScale = new Vector3(rangeObject.localScale.x + 3f, rangeObject.localScale.y, rangeObject.localScale.z + 3f);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeCost1 = GetUpgradeCost(upgradeLevel1, 1);
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);

                            if (upgradeLevel2 > 1)
                            {
                                isPath3Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            else if (upgradeLevel3 > 1)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            break;
                        case 2:
                            //Do upgrade
                            fireRate += 0.25f;
                            transform.GetComponent<IceDamage>().UpdateFireRate(fireRate);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeCost1 = GetUpgradeCost(upgradeLevel1, 1);
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);

                            if (upgradeLevel2 == 4 || upgradeLevel3 == 4)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            break;
                        case 3:
                            //Do upgrade
                            tempIce.UpdateSnowSpeed(tempIce.GetSnowSpeed() - 0.1f);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);

                            if (upgradeLevel2 == 3)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            if (upgradeLevel3 == 3)
                            {
                                isPath3Restricted = true;
                                upgradeDescription3 = "Path Restricted";
                            }
                            break;
                    }
                    break;
                case TowerType.Orb:
                    SupportBehavior support = transform.gameObject.GetComponent<SupportBehavior>();
                    switch (upgradeLevel1)
                    {
                        case 1:
                            //Do upgrade
                            support.RemoveBuffs();
                            support.attackRangeBuff += 0.15f;
                            range += 0.5f;
                            rangeObject.localScale = new Vector3(rangeObject.localScale.x + 1f, rangeObject.localScale.y, rangeObject.localScale.z + 1f);
                            support.UpdateTowersInRange();

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeCost1 = GetUpgradeCost(upgradeLevel1, 1);
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);

                            if (upgradeLevel2 > 1)
                            {
                                isPath3Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            else if (upgradeLevel3 > 1)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            break;
                        case 2:
                            //Hidden
                            //TODO
                            support.UpdateTowersInRange();

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeCost1 = GetUpgradeCost(upgradeLevel1, 1);
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);

                            if (upgradeLevel2 == 4 || upgradeLevel3 == 4)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            break;
                        case 3:
                            //Do upgrade
                            support.fireRateBuff += 1.2f;
                            support.UpdateTowersInRange();

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);

                            if (upgradeLevel2 == 3)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            if (upgradeLevel3 == 3)
                            {
                                isPath3Restricted = true;
                                upgradeDescription3 = "Path Restricted";
                            }
                            break;
                    }
                    break;
                case TowerType.Spikes:
                    switch (upgradeLevel1)
                    {
                        case 1:
                            //Do upgrade
                            fireRate += 0.5f;
                            transform.GetComponent<SpikeDamage>().UpdateFireRate(fireRate);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeCost1 = GetUpgradeCost(upgradeLevel1, 1);
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);

                            if (upgradeLevel2 > 1)
                            {
                                isPath3Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            else if (upgradeLevel3 > 1)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            break;
                        case 2:
                            //Do upgrade
                            fireRate += 0.5f;
                            transform.GetComponent<SpikeDamage>().UpdateFireRate(fireRate);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeCost1 = GetUpgradeCost(upgradeLevel1, 1);
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);

                            if (upgradeLevel2 == 4 || upgradeLevel3 == 4)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            break;
                        case 3:
                            //Do upgrade
                            fireRate += 1f;
                            transform.GetComponent<SpikeDamage>().UpdateFireRate(fireRate);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);

                            if (upgradeLevel2 == 3)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            if (upgradeLevel3 == 3)
                            {
                                isPath3Restricted = true;
                                upgradeDescription3 = "Path Restricted";
                            }
                            break;
                    }
                    break;
            }
            upgradeLevel1++;
            UpdateUpgradePanel();
            upgradePanel.UpdateProgressBar(1, upgradeLevel1);
        }
    }
    #endregion
    #region Path 2
    public void Upgrade2()
    {

        if (player.GetMoney() >= upgradeCost2)
        {
            Transform rangeObject = transform.Find("Range");
            player.RemoveMoney(upgradeCost2);
            switch (towerType)
            {
                case TowerType.Crossbow:
                    switch (upgradeLevel2)
                    {
                        case 1:
                            //Do upgrade
                            damage += 1f;
                            transform.GetComponent<StandardDamage>().UpdateDamage(damage);

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeCost2 = GetUpgradeCost(upgradeLevel2, 2);
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);

                            if (upgradeLevel1 > 1)
                            {
                                isPath3Restricted = true;
                                upgradeDescription3 = "Path Restricted";
                            }
                            else if (upgradeLevel3 > 1)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            break;
                        case 2:
                            //Do upgrade
                            damage += 2f;
                            transform.GetComponent<StandardDamage>().UpdateDamage(damage);

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeCost2 = GetUpgradeCost(upgradeLevel2, 2);
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);

                            if (upgradeLevel1 == 4 || upgradeLevel3 == 4)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            break;
                        case 3:
                            //Do upgrade
                            damage += 7;
                            transform.GetComponent<StandardDamage>().UpdateDamage(damage);

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);

                            if (upgradeLevel1 == 3)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            if (upgradeLevel3 == 3)
                            {
                                isPath3Restricted = true;
                                upgradeDescription3 = "Path Restricted";
                            }
                            break;
                    }
                    break;
                case TowerType.Cannon:
                    switch (upgradeLevel2)
                    {
                        case 1:
                            //Do upgrade
                            fireRate += 0.07f;
                            transform.GetComponent<MissileDamage>().UpdateFireRate(fireRate);

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeCost2 = GetUpgradeCost(upgradeLevel2, 2);
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);

                            if (upgradeLevel1 > 1)
                            {
                                isPath3Restricted = true;
                                upgradeDescription3 = "Path Restricted";
                            }
                            else if (upgradeLevel3 > 1)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            break;
                        case 2:
                            //Do upgrade
                            fireRate += 0.1f;
                            transform.GetComponent<MissileDamage>().UpdateFireRate(fireRate);

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeCost2 = GetUpgradeCost(upgradeLevel2, 2);
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);

                            if (upgradeLevel1 == 4 || upgradeLevel3 == 4)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            break;
                        case 3:
                            //Do upgrade
                            fireRate += 0.2f;
                            transform.GetComponent<MissileDamage>().UpdateFireRate(fireRate);

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);

                            if (upgradeLevel1 == 3)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            if (upgradeLevel3 == 3)
                            {
                                isPath3Restricted = true;
                                upgradeDescription3 = "Path Restricted";
                            }
                            break;
                    }
                    break;
                case TowerType.Flame:
                    switch (upgradeLevel2)
                    {
                        case 1:
                            //Do upgrade
                            range += 1f;
                            Transform fireTrigger = transform.Find("Head").transform.Find("FireTriggerPivot").transform.Find("FireTrigger").transform;
                            fireTrigger.localScale = new Vector3(fireTrigger.localScale.x + 1f, fireTrigger.localScale.y, fireTrigger.localScale.z - 0.5f);
                            fireTrigger.position = new Vector3(fireTrigger.position.x, fireTrigger.position.y, fireTrigger.position.z + 0.5f);
                            rangeObject.localScale = new Vector3(rangeObject.localScale.x + 2f, rangeObject.localScale.y, rangeObject.localScale.z + 2f);

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeCost2 = GetUpgradeCost(upgradeLevel2, 2);
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);

                            if (upgradeLevel1 > 1)
                            {
                                isPath3Restricted = true;
                                upgradeDescription3 = "Path Restricted";
                            }
                            else if (upgradeLevel3 > 1)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            break;
                        case 2:
                            //Do upgrade
                            damage += 0.05f;
                            transform.GetComponent<FireDamage>().UpdateDamage(damage);

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;

                            upgradeCost2 = GetUpgradeCost(upgradeLevel2, 2);
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);

                            if (upgradeLevel1 == 4 || upgradeLevel3 == 4)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            break;
                        case 3:
                            //Do upgrade
                            damage += 0.1f;

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);

                            if (upgradeLevel1 == 3)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            if (upgradeLevel3 == 3)
                            {
                                isPath3Restricted = true;
                                upgradeDescription3 = "Path Restricted";
                            }
                            break;
                    }
                    break;
                case TowerType.Mine:
                    switch (upgradeLevel2)
                    {
                        case 1:
                            //Do upgrade
                            //TODO

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeCost2 = GetUpgradeCost(upgradeLevel2, 2);
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);

                            if (upgradeLevel1 > 1)
                            {
                                isPath3Restricted = true;
                                upgradeDescription3 = "Path Restricted";
                            }
                            else if (upgradeLevel3 > 1)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            break;
                        case 2:
                            //Do upgrade
                            //TODO

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeCost2 = GetUpgradeCost(upgradeLevel2, 2);
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);

                            if (upgradeLevel1 == 4 || upgradeLevel3 == 4)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            break;
                        case 3:
                            //Do upgrade
                            //TODO

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);

                            if (upgradeLevel1 == 3)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            if (upgradeLevel3 == 3)
                            {
                                isPath3Restricted = true;
                                upgradeDescription3 = "Path Restricted";
                            }
                            break;
                    }
                    break;
                case TowerType.Snowball:
                    IceDamage tempIce = transform.gameObject.GetComponent<IceDamage>();
                    switch (upgradeLevel2)
                    {
                        case 1:
                            //Do upgrade
                            tempIce.UpdateSnowSpeedReduction(tempIce.GetSnowSpeedReduction() - 0.05f);

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeCost2 = GetUpgradeCost(upgradeLevel2, 2);
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);

                            if (upgradeLevel1 > 1)
                            {
                                isPath3Restricted = true;
                                upgradeDescription3 = "Path Restricted";
                            }
                            else if (upgradeLevel3 > 1)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            break;
                        case 2:
                            //Do upgrade
                            tempIce.UpdateSnowSpeedReduction(tempIce.GetSnowSpeedReduction() - 0.05f);

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeCost2 = GetUpgradeCost(upgradeLevel2, 2);
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);

                            if (upgradeLevel1 == 4 || upgradeLevel3 == 4)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            break;
                        case 3:
                            //Do upgrade
                            tempIce.UpdateSnowSpeedReduction(tempIce.GetSnowSpeedReduction() - 0.1f);

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);

                            if (upgradeLevel1 == 3)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            if (upgradeLevel3 == 3)
                            {
                                isPath3Restricted = true;
                                upgradeDescription3 = "Path Restricted";
                            }
                            break;
                    }
                    break;
                case TowerType.Orb:
                    SupportBehavior support = transform.gameObject.GetComponent<SupportBehavior>();
                    switch (upgradeLevel2)
                    {
                        case 1:
                            //Do upgrade
                            support.RemoveBuffs();
                            support.attackRangeBuff += 0.15f;
                            range += 0.5f;
                            rangeObject.localScale = new Vector3(rangeObject.localScale.x + 1f, rangeObject.localScale.y, rangeObject.localScale.z + 1f);
                            support.UpdateTowersInRange();

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeCost2 = GetUpgradeCost(upgradeLevel2, 2);
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);

                            if (upgradeLevel1 > 1)
                            {
                                isPath3Restricted = true;
                                upgradeDescription3 = "Path Restricted";
                            }
                            else if (upgradeLevel3 > 1)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            break;
                        case 2:
                            //Hidden
                            //TODO
                            support.UpdateTowersInRange();

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeCost2 = GetUpgradeCost(upgradeLevel2, 2);
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);

                            if (upgradeLevel1 == 4 || upgradeLevel3 == 4)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            break;
                        case 3:
                            //Do upgrade
                            support.fireRateBuff += 1.2f;
                            support.UpdateTowersInRange();

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);

                            if (upgradeLevel1 == 3)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            if (upgradeLevel3 == 3)
                            {
                                isPath3Restricted = true;
                                upgradeDescription3 = "Path Restricted";
                            }
                            break;
                    }
                    break;
                case TowerType.Spikes:
                    switch (upgradeLevel2)
                    {
                        case 1:
                            //Do upgrade
                            transform.localScale += new Vector3(0.25f, 0, 0.25f);

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeCost1 = GetUpgradeCost(upgradeLevel2, 2);
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);

                            if (upgradeLevel1 > 1)
                            {
                                isPath3Restricted = true;
                                upgradeDescription3 = "Path Restricted";
                            }
                            else if (upgradeLevel3 > 1)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            break;
                        case 2:
                            //Do upgrade
                            transform.localScale += new Vector3(0.25f, 0, 0.25f);

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeCost2 = GetUpgradeCost(upgradeLevel2, 2);
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);

                            if (upgradeLevel1 == 4 || upgradeLevel3 == 4)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            break;
                        case 3:
                            //Do upgrade
                            transform.localScale += new Vector3(0.5f, 0, 0.5f);

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);

                            if (upgradeLevel1 == 3)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            if (upgradeLevel3 == 3)
                            {
                                isPath3Restricted = true;
                                upgradeDescription3 = "Path Restricted";
                            }
                            break;
                    }
                    break;
            }
            upgradeLevel2++;
            UpdateUpgradePanel();
            upgradePanel.UpdateProgressBar(2, upgradeLevel2);
        }
    }
    #endregion
    #region Path 3
    public void Upgrade3()
    {
        if (player.GetMoney() >= upgradeCost3)
        {
            Transform rangeObject = transform.Find("Range");
            player.RemoveMoney(upgradeCost3);
            switch (towerType)
            {
                case TowerType.Crossbow:
                    switch (upgradeLevel3)
                    {
                        case 1:
                            //Do upgrade
                            fireRate += 1;
                            transform.GetComponent<StandardDamage>().UpdateFireRate(fireRate);

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeCost3 = GetUpgradeCost(upgradeLevel3, 3);
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);

                            if (upgradeLevel1 > 1)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            else if (upgradeLevel2 > 1)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            break;
                        case 2:
                            //Do upgrade
                            fireRate += 2;
                            transform.GetComponent<StandardDamage>().UpdateFireRate(fireRate);

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeCost3 = GetUpgradeCost(upgradeLevel3, 3);
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);

                            if (upgradeLevel1 == 4 || upgradeLevel2 == 4)
                            {
                                isPath3Restricted = true;
                                upgradeDescription3 = "Path Restricted";
                            }
                            break;
                        case 3:
                            //Do upgrade
                            fireRate += 2;
                            transform.GetComponent<StandardDamage>().UpdateFireRate(fireRate);

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);

                            if (upgradeLevel1 == 3)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            if (upgradeLevel2 == 3)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            break;
                    }
                    break;
                case TowerType.Cannon:
                    switch (upgradeLevel3)
                    {
                        case 1:
                            //Do upgrade
                            transform.GetComponent<MissileDamage>().explosionRadius += 1;

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeCost3 = GetUpgradeCost(upgradeLevel3, 3);
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);

                            if (upgradeLevel1 > 1)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            else if (upgradeLevel2 > 1)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            break;
                        case 2:
                            //Do upgrade
                            transform.GetComponent<MissileDamage>().explosionRadius += 3;

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeCost3 = GetUpgradeCost(upgradeLevel3, 3);
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);

                            if (upgradeLevel1 == 4 || upgradeLevel2 == 4)
                            {
                                isPath3Restricted = true;
                                upgradeDescription3 = "Path Restricted";
                            }
                            break;
                        case 3:
                            //Do upgrade
                            transform.GetComponent<MissileDamage>().explosionRadius += 5;

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);

                            if (upgradeLevel1 == 3)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            if (upgradeLevel2 == 3)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            break;
                    }
                    break;
                case TowerType.Flame:
                    switch (upgradeLevel3)
                    {
                        case 1:
                            //Do upgrade
                            transform.GetComponentInChildren<FireTriggerCollisionDetector>().duration += 1;

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeCost3 = GetUpgradeCost(upgradeLevel3, 3);
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);

                            if (upgradeLevel1 > 1)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            else if (upgradeLevel2 > 1)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            break;
                        case 2:
                            //Do upgrade
                            transform.GetComponentInChildren<FireTriggerCollisionDetector>().duration += 2;

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;

                            upgradeCost3 = GetUpgradeCost(upgradeLevel3, 3);
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);

                            if (upgradeLevel1 == 4 || upgradeLevel2 == 4)
                            {
                                isPath3Restricted = true;
                                upgradeDescription3 = "Path Restricted";
                            }
                            break;
                        case 3:
                            //Do upgrade
                            transform.GetComponentInChildren<FireTriggerCollisionDetector>().duration += 4;

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);

                            if (upgradeLevel1 == 3)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            if (upgradeLevel2 == 3)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            break;
                    }
                    break;
                case TowerType.Mine:
                    switch (upgradeLevel3)
                    {
                        case 1:
                            //Do upgrade
                            transform.GetComponent<EconomyBehavior>().investmentsPercent += 0.05f;
                            transform.GetComponent<EconomyBehavior>().UpdateTowersInRange();

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeCost3 = GetUpgradeCost(upgradeLevel3, 3);
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);

                            if (upgradeLevel1 > 1)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            else if (upgradeLevel2 > 1)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            break;
                        case 2:
                            //Do upgrade
                            transform.GetComponent<EconomyBehavior>().investmentsPercent += 0.05f;
                            transform.GetComponent<EconomyBehavior>().UpdateTowersInRange();

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeCost3 = GetUpgradeCost(upgradeLevel3, 3);
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);

                            if (upgradeLevel1 == 4 || upgradeLevel2 == 4)
                            {
                                isPath3Restricted = true;
                                upgradeDescription3 = "Path Restricted";
                            }
                            break;
                        case 3:
                            //Do upgrade
                            transform.GetComponent<EconomyBehavior>().investmentsPercent += 0.1f;
                            transform.GetComponent<EconomyBehavior>().UpdateTowersInRange();

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);

                            if (upgradeLevel1 == 3)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            if (upgradeLevel2 == 3)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            break;
                    }
                    break;
                case TowerType.Snowball:
                    IceDamage tempIce = transform.gameObject.GetComponent<IceDamage>();
                    switch (upgradeLevel3)
                    {
                        case 1:
                            //Do upgrade
                            tempIce.UpdateSnowSize(tempIce.GetSnowSize() + 0.5f);

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeCost3 = GetUpgradeCost(upgradeLevel3, 3);
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);

                            if (upgradeLevel1 > 1)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            else if (upgradeLevel2 > 1)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            break;
                        case 2:
                            //Do upgrade
                            tempIce.UpdateSnowSize(tempIce.GetSnowSize() + 0.5f);

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeCost3 = GetUpgradeCost(upgradeLevel3, 3);
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);

                            if (upgradeLevel1 == 4 || upgradeLevel2 == 4)
                            {
                                isPath3Restricted = true;
                                upgradeDescription3 = "Path Restricted";
                            }
                            break;
                        case 3:
                            //Do upgrade
                            tempIce.UpdateSnowSize(tempIce.GetSnowSize() + 1f);

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);

                            if (upgradeLevel1 == 3)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            if (upgradeLevel2 == 3)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            break;
                    }
                    break;
                case TowerType.Orb:
                    SupportBehavior support = transform.gameObject.GetComponent<SupportBehavior>();
                    switch (upgradeLevel3)
                    {
                        case 1:
                            //Do upgrade
                            support.RemoveBuffs();
                            support.attackRangeBuff += 0.15f;
                            range += 0.5f;
                            rangeObject.localScale = new Vector3(rangeObject.localScale.x + 1f, rangeObject.localScale.y, rangeObject.localScale.z + 1f);
                            support.UpdateTowersInRange();

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeCost3 = GetUpgradeCost(upgradeLevel3, 3);
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);

                            if (upgradeLevel1 > 1)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            else if (upgradeLevel2 > 1)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            break;
                        case 2:
                            //Hidden
                            //TODO
                            support.UpdateTowersInRange();

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeCost3 = GetUpgradeCost(upgradeLevel3, 3);
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);

                            if (upgradeLevel1 == 4 || upgradeLevel2 == 4)
                            {
                                isPath3Restricted = true;
                                upgradeDescription3 = "Path Restricted";
                            }
                            break;
                        case 3:
                            //Do upgrade
                            support.fireRateBuff += 1.2f;
                            support.UpdateTowersInRange();

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);

                            if (upgradeLevel1 == 3)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            if (upgradeLevel2 == 3)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            break;
                    }
                    break;
                case TowerType.Spikes:
                    switch (upgradeLevel3)
                    {
                        case 1:
                            //Do upgrade
                            damage += 0.05f;
                            transform.GetComponent<SpikeDamage>().UpdateDamage(damage);

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeCost3 = GetUpgradeCost(upgradeLevel3, 3);
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);

                            if (upgradeLevel1 > 1)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            else if (upgradeLevel2 > 1)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            break;
                        case 2:
                            //Do upgrade
                            damage += 0.05f;
                            transform.GetComponent<SpikeDamage>().UpdateDamage(damage);

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeCost3 = GetUpgradeCost(upgradeLevel3, 3);
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);

                            if (upgradeLevel1 == 4 || upgradeLevel2 == 4)
                            {
                                isPath3Restricted = true;
                                upgradeDescription3 = "Path Restricted";
                            }
                            break;
                        case 3:
                            //Do upgrade
                            damage += 0.1f;
                            transform.GetComponent<SpikeDamage>().UpdateDamage(damage);

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);

                            if (upgradeLevel1 == 3)
                            {
                                isPath1Restricted = true;
                                upgradeDescription1 = "Path Restricted";
                            }
                            if (upgradeLevel2 == 3)
                            {
                                isPath2Restricted = true;
                                upgradeDescription2 = "Path Restricted";
                            }
                            break;
                    }
                    break;
            }
            upgradeLevel3++;
            UpdateUpgradePanel();
            upgradePanel.UpdateProgressBar(3, upgradeLevel3);
        }
    }
    #endregion
    #endregion

    public void UpdateUpgradePanel()
    {
        //update text
        leveltext = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        if (leveltext)
        {
            leveltext.text = $"{upgradeLevel1}-{upgradeLevel2}-{upgradeLevel3}";
        }
        upgradePanel.SetTarget(this, (int)targetType);
        upgradePanel.SetSellButton(sellCost);

        upgradePanel.SetText(upgradeDescription1, 1);
        upgradePanel.SetText(upgradeDescription2, 2);
        upgradePanel.SetText(upgradeDescription3, 3);

        upgradePanel.SetCost(upgradeCost1.ToString(), 1, upgradeLevel1);
        upgradePanel.SetCost(upgradeCost2.ToString(), 2, upgradeLevel2);
        upgradePanel.SetCost(upgradeCost3.ToString(), 3, upgradeLevel3);

        upgradePanel.ToggleUpgradeButton(upgradeLevel1 != 4, 1);
        upgradePanel.ToggleUpgradeButton(upgradeLevel2 != 4, 2);
        upgradePanel.ToggleUpgradeButton(upgradeLevel3 != 4, 3);

        upgradePanel.RestrictPaths(isPath1Restricted, isPath2Restricted, isPath3Restricted);
    }

    public void UpdateAllUpgradesScreen()
    {

    }

    public struct UpgradeData
    {
        public string name;
        public string description;
        public int cost;

        public UpgradeData(string name, int cost, string description)
        {
            this.name = name;
            this.description = description;
            this.cost = cost;
        }
    }

    #region Upgrade Initialization
    #region Upgrade 1
    private static readonly Dictionary<TowerType, List<UpgradeData>> upgradeDataMap1 = new()
    {
        //Description and Cost
        {
            TowerType.Crossbow, new List<UpgradeData>
            {
                new UpgradeData("Better Scopes\nIncreases Range", 50, "Increases Range"),
                new UpgradeData("Scopier Scopes\nIncreases Range", 150, "Increases Range"),
                new UpgradeData("Scopiest Scopes\nIncreases Range", 200, "Increases Range"),
                new UpgradeData("Max Level", 0, "")
            }
        },
        {
            TowerType.Cannon, new List<UpgradeData>
            {
                new UpgradeData("Heavier Bombs\nIncreases Damage", 150, "Increases Damage"),
                new UpgradeData("More Powder\nIncreases Damage", 200, "Increases Damage"),
                new UpgradeData("BOOM.\nIncreases Damage", 400, "Increases Damage"),
                new UpgradeData("Max Level", 0, "")
            }
        },
        {
            TowerType.Flame, new List<UpgradeData>
            {
                new UpgradeData("Better Fuel\nIncreases Burn Rate", 150, "Increases Burn Rate"),
                new UpgradeData("Quicker Burns\nIncreases Burn Rate", 400, "Increases Burn Rate"),
                new UpgradeData("Nonstop Burns\nIncreases Burn Rate", 1000, "Increases Burn Rate"),
                new UpgradeData("Max Level", 0, "")
            }
        },
        {
            TowerType.Mine, new List<UpgradeData>
            {
                new UpgradeData("More Money\nIncreases End of Wave Bonus", 300, "Increases End of Wave Bonus"),
                new UpgradeData("Bigger Min\nIncreases End of Wave Bonus", 500, "Increases End of Wave Bonus"),
                new UpgradeData("Chain Company\nIncreases End of Wave Bonus", 750, "Increases End of Wave Bonus"),
                new UpgradeData("Max Level", 0, "")
            }
        },
        {
            TowerType.Snowball, new List<UpgradeData>
            {
                new UpgradeData("Increased Detector\nIncreases Range", 100, "Increases Range"),
                new UpgradeData("Faster firing\nIncreases Attack Speed", 150, "Increases Attack Speed"),
                new UpgradeData("Aerodynamic\nDecreases Snowball Travel Time", 200, "Decreases Snowball Travel Time"),
                new UpgradeData("Max Level", 0, "")
            }
        },
        {
            TowerType.Orb, new List<UpgradeData>
            {
                new UpgradeData("Proximity Sensor (Not Implemented Yet)", 0, "Increases Range and Range Buff"),
                new UpgradeData("Inspiration\nIncreases Range Buff", 300, "Increases Range Buff"),
                new UpgradeData("Inspiration\nIncreases Range Buff", 900, "Increases Range Buff"),
                new UpgradeData("Max Level", 0, "")
            }
        },
        {
            TowerType.Spikes, new List<UpgradeData>
            {
                new UpgradeData("Bigger Spikes\nIncreases Rate of Damage", 150, "Increases Rate of Damage"),
                new UpgradeData("Faster Spikes\nIncreases Rate of Damage", 250, "Increases Rate of Damage"),
                new UpgradeData("Deadly Spikes\nIncreases Rate of Damage", 350, "Increases Rate of Damage"),
                new UpgradeData("Max Level", 0, "")
            }
        }
    };
    #endregion
    #region Upgrade 2
    private static readonly Dictionary<TowerType, List<UpgradeData>> upgradeDataMap2 = new()
    {
        //Description and Cost
        {
            TowerType.Crossbow, new List<UpgradeData>
            {
                new UpgradeData("Sharper Arrows\nIncreases Damage", 100, "Increases Damage"),
                new UpgradeData("Stronger Arrows\nIncreases Damage", 250, "Increases Damage"),
                new UpgradeData("Barbed Arrows\nIncreases Damage", 400, "Increases Damage"),
                new UpgradeData("Max Level", 0, "")
            }
        },
        {
            TowerType.Cannon, new List<UpgradeData>
            {
                new UpgradeData("Faster Reload\nIncreases Attack Speed", 100, "Increases Attack Speed"),
                new UpgradeData("Fastest Reload\nIncreases Attack Speed" + "Cost", 220, "Increases Attack Speed"),
                new UpgradeData("Automated Reload\nIncreases Attack Speed" + "Cost", 400, "Increases Attack Speed"),
                new UpgradeData("Max Level", 0, "")
            }
        },
        {
            TowerType.Flame, new List<UpgradeData>
            {
                new UpgradeData("Better Propellent\nIncreases Range", 100, "Increases Range"),
                new UpgradeData("Burny Burns\nIncreases Range", 500, "Increases Damage"),
                new UpgradeData("Hellfire\nIncreases Range", 1200, "Increases Damage"),
                new UpgradeData("Max Level", 0, "")
            }
        },
        {
            TowerType.Mine, new List<UpgradeData>
            {
                new UpgradeData("Interest\nExtra Money based on current money", 0, "Extra money based on current money at end of wave (Only highest Value Applied)"),
                new UpgradeData("Better Deals\nIncreases Interest Effect", 0, "Increases Interest Effect"),
                new UpgradeData("Compound Interest\nIncreases Interest Effect", 0, "Increases Interest Effect"),
                new UpgradeData("Max Level", 0, "")
            }
        },
        {
            TowerType.Snowball, new List<UpgradeData>
            {
                new UpgradeData("Thicker Snow\nIncreases Slow Effect", 200, "Increases Slow Effect"),
                new UpgradeData("Freezing Snow\nIncreases Slow Effect", 300, "Increases Slow Effect"),
                new UpgradeData("Frostbite\nIncreases Slow Effect", 650, "Increases Slow Effect"),
                new UpgradeData("Max Level", 0, "")
            }
        },
        {
            TowerType.Orb, new List<UpgradeData>
            {
                new UpgradeData("Focus\nGrants Damage Buff to nearby Towers", 0, "Grants Damage Buff to nearby towers"),
                new UpgradeData("Concentration\nIncreases Damage Buff", 300, "Increases Damage Buff"),
                new UpgradeData("Unification\nIncreases Damage Buff", 1100, "Increases Damage Buff"),
                new UpgradeData("Max Level", 0, "")
            }
        },
        {
            TowerType.Spikes, new List<UpgradeData>
            {
                new UpgradeData("More Spikes\nIncreases Size", 200, "Increases Size"),
                new UpgradeData("Even More Spikes\nIncreases Size", 400, "Increases Size"),
                new UpgradeData("Carpet of Spikes\nIncreases Size", 650, "Increases Size"),
                new UpgradeData("Max Level", 0, "")
            }
        }
    };
    #endregion
    #region Upgrade 3
    private static readonly Dictionary<TowerType, List<UpgradeData>> upgradeDataMap3 = new()
    {
        //Description and Cost
        {
            TowerType.Crossbow, new List<UpgradeData>
            {
                new UpgradeData("Faster Firing\nIncreases Attack Speed", 150, "Increases Attack Speed"),
                new UpgradeData("Faster Mechanism\nIncreases Attack Speed", 200, "Increases Attack Speed"),
                new UpgradeData("Automatic Firing\nIncreases Attack Speed", 450, "Increases Attack Speed"),
                new UpgradeData("Max Level", 0, "")
            }
        },
        {
            TowerType.Cannon, new List<UpgradeData>
            {
                new UpgradeData("Bombier Bomb\nIncreases Explosion Radius", 170, "Increases Explosion Radius"),
                new UpgradeData("Compressed Charge\nIncreases Explosion Radius", 280, "Increases Explosion Radius"),
                new UpgradeData("Atomic Explosion\nIncreases Explosion Radius", 490, "Increases Explosion Radius"),
                new UpgradeData("Max Level", 0, "")
            }
        },
        {
            TowerType.Flame, new List<UpgradeData>
            {
                new UpgradeData("Longer Lasting Burns\nIncreases Burn Duration", 250, "Increases Burn Duration"),
                new UpgradeData("Hell's Itch\nIncreases Burn Duration", 550, "Increases Burn Duration"),
                new UpgradeData("Third Degree Burns\nIncreases Burn Duration", 900, "Increases Burn Duration"),
                new UpgradeData("Max Level", 0, "")
            }
        },
        {
            TowerType.Mine, new List<UpgradeData>
            {
                new UpgradeData("Investments\nGrants Money Earned Buff to nearby towers", 500, "Grants Money Earned Buff to nearby towers"),
                new UpgradeData("Stocks\nGrants Money Earned Buff to nearby towers", 750, "Increases Money Earned Buff"),
                new UpgradeData("Majority Owner\nGrants Money Earned Buff to nearby towers", 1000, "Increases Money Earned Buff"),
                new UpgradeData("Max Level", 0, "")
            }
        },
        {
            TowerType.Snowball, new List<UpgradeData>
            {
                new UpgradeData("Bigger Snowball\nIncreases Snow Area", 200, "Increases Snow Area"),
                new UpgradeData("Biggest Snowball\nIncreases Snow Area", 250, "Increases Snow Area"),
                new UpgradeData("Carpet Bomb\nIncreases Snow Area", 500, "Increases Snow Area"),
                new UpgradeData("Max Level", 0, "")
            }
        },
        {
            TowerType.Orb, new List<UpgradeData>
            {
                new UpgradeData("Morale Boost\nGrants Attack Speed Buff to nearby towers", 0, "Grants Attack Speed Buff to nearby towers"),
                new UpgradeData("Inspiration\nIncreases Attack Speed Buff", 300, "Increases Attack Speed Buff"),
                new UpgradeData("Inspiration II\nIncreases Attack Speed Buff", 300, "Increases Attack Speed Buff"),
                new UpgradeData("Max Level", 0, "")
            }
        },
        {
            TowerType.Spikes, new List<UpgradeData>
            {
                new UpgradeData("Spikier Spikes\nIncreases Damage", 250, "Increases Damage"),
                new UpgradeData("Sharper Spikes\nIncreases Damage", 300, "Increases Damage"),
                new UpgradeData("LEGOS\nIncreases Damage", 400, "Increases Damage"),
                new UpgradeData("Max Level", 0, "")
            }
        }
    };
    #endregion
    #endregion


    public void SetTargetType(int typeIndex)
    {
        if (Enum.IsDefined(typeof(TowerTargetting.TargetType), typeIndex))
        {
            targetType = (TowerTargetting.TargetType)typeIndex;
        }
        else
        {
            Debug.LogError("Invalid target type index: " + typeIndex);
        }
    }

    public UpgradeData GetUpgradeData(int level, int path)
    {
        switch(path)
        {
            case 1:
                if (level < 0 || level >= upgradeDataMap1[towerType].Count)
                {
                    return new UpgradeData("Invalid Level", 0, "");
                }
                return upgradeDataMap1[towerType][level];
                case 2:
                if (level < 0 || level >= upgradeDataMap2[towerType].Count)
                {
                    return new UpgradeData("Invalid Level", 0, "");
                }
                return upgradeDataMap2[towerType][level];
            case 3:
                if (level < 0 || level >= upgradeDataMap3[towerType].Count)
                {
                    return new UpgradeData("Invalid Level", 0, "");
                }
                return upgradeDataMap3[towerType][level];
        }
        return new UpgradeData("Invalid Path", 0, "");
    }

    public int GetMaxUpgradeLevel(int path)
    {
        switch(path)
        {
            case 1:
                return upgradeDataMap1[towerType].Count - 1;
            case 2:
                return upgradeDataMap2[towerType].Count - 1;
            case 3:
                return upgradeDataMap3[towerType].Count - 1;
        }
        Debug.LogWarning("Invalid Path");
        return -1;
        
    }
    public string GetUpgradeName(int level, int path)
    {
        return GetUpgradeData(level, path).name;
    }
    public int GetUpgradeCost(int level, int path)
    {
        return GetUpgradeData(level, path).cost;
    }

    public string GetUpgradeDescription(int level, int path)
    {
        return GetUpgradeData(level, path).description;
    }

    public enum TowerType
    {
        Crossbow,
        Flame,
        Cannon,
        Mine,
        Snowball,
        Orb,
        Spikes
    }

    public void SetTauntTarget(Enemy enemy)
    {
        tauntTarget = enemy;
    }

    public void CleanseDebuffs()
    {
        activeBuffs.RemoveAll(x => x.isDebuff); //Removes all debuffs
        isStunned = false;
        isTaunted = false;
        tauntTarget = null;
    }
}