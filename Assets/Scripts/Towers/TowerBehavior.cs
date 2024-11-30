using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

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
    private bool isStunned;

    [SerializeField] public TowerType towerType;
    [SerializeField] public TowerTargetting.TargetType targetType;
    private IDamageMethod currentDamageMethodClass;
    private Player player;
    [NonSerialized] public int upgradeLevel1, upgradeLevel2, upgradeLevel3;
    private UpgradePanel upgradePanel;
    private int upgradeCost1, upgradeCost2, upgradeCost3;
    [NonSerialized] public int sellCost;
    private string upgradeDescription1, upgradeDescription2, upgradeDescription3;

    string[] buffNames;
    private int buffNamesCount;
    public float moneyMultiplier;

    GameObject lastSelectedTower;

    private void Start()
    {
        targetType = TowerTargetting.TargetType.First;
        towerPlacement = TowerPlacement.Instance;
        upgradePanel = UpgradePanel.Instance;
        player = Player.Instance;
        cam = towerPlacement.cam;
        isSelected = true;
        isStunned = false;
        lastSelectedTower = null;
        activeBuffs = new();
        appliedBuffs = new();

        buffNames = Enum.GetNames(typeof(GameManager.BuffNames));
        buffNamesCount = Enum.GetNames(typeof(GameManager.BuffNames)).Length;
        moneyMultiplier = 1;

        currentDamageMethodClass = GetComponent<IDamageMethod>();

        if (currentDamageMethodClass == null)
        {
            Debug.LogError("ERROR: FAILED TO FIND A DAMAGE CLASS ON CURRENT TOWER!");
        }
        else
        {
            currentDamageMethodClass.Init(damage, fireRate);
        }

        delay = 1 / fireRate;

        upgradeLevel1 = 0;
        upgradeLevel2 = 0;
        upgradeLevel3 = 0;

        switch (towerType)
        {
            case TowerType.Basic:
                upgradeCost1 = 50;
                upgradeDescription1 = GetUpgradeData(0, 1).name;
                upgradeCost2 = 100;
                upgradeDescription2 = "Sharper Arrows\n" + "Cost: " + 100;
                upgradeCost2 = 150;
                upgradeDescription3 = "Faster Firing\n" + "Cost: " + 150;
                break;
            case TowerType.Bomb:
                upgradeCost1 = 200;
                upgradeDescription1 = "Heavier Bombs\n" + "Cost: " + 200;
                upgradeCost2 = 200;
                upgradeDescription2 = "Faster Reload\n" + "Cost: " + 200;
                upgradeCost3 = 250;
                upgradeDescription3 = "Bombier Bomb\n" + "Cost: " + 250;
                break;
            case TowerType.Flame:
                upgradeCost1 = 200;
                upgradeDescription1 = "Better Fuel\n" + "Cost: " + 200;
                upgradeCost2 = 100;
                upgradeDescription2 = "Stronger Propellent\n" + "Cost: " + 100;
                upgradeCost3 = 300;
                upgradeDescription3 = "Longer lasting burns\n" + "Cost: " + 300;
                break;
            case TowerType.Economy:
                upgradeCost1 = 300;
                upgradeDescription1 = "More Money\n" + "Cost: " + 300;
                upgradeCost2 = 0;
                upgradeDescription2 = "Interest (Not Implemented)\n" + "Cost: " + 0;
                upgradeCost3 = 500;
                upgradeDescription3 = "Investments\n" + "Cost: " + 500; //All towers in range gain more money for defeating an enemy
                break;
            case TowerType.Ice:
                upgradeCost1 = 100;
                upgradeDescription1 = "Increased detector\n" + "Cost: " + 100;
                upgradeCost2 = 250;
                upgradeDescription2 = "Thicker Snow\n" + "Cost: " + 250;
                upgradeCost3 = 200;
                upgradeDescription3 = "Bigger snowball\n" + "Cost: " + 200;
                break;
            case TowerType.Support:
                upgradeCost1 = 100;
                upgradeDescription1 = "Command\n" + "Cost: " + 100;
                upgradeCost2 = 100;
                upgradeDescription2 = "Increase Damage buff\n" + "Cost: " + 100;
                upgradeCost3 = 100;
                upgradeDescription3 = "Increase attack speed buff\n" + "Cost: " + 100;
                break;
            case TowerType.Spikes:
                upgradeCost1 = 150;
                upgradeDescription1 = "Bigger Spikes\n" + "Cost: " + 150;
                upgradeCost2 = 200;
                upgradeDescription2 = "More Spikes\n" + "Cost: " + 200;
                upgradeCost3 = 250;
                upgradeDescription3 = "Spikier Spikes\n" + "Cost: " + 250;
                break;
        }
        sellCost = cost / 2;
    }

    //Desyncs the towers from regular game loop to prevent errors
    public void Tick()
    {
        if (!isStunned)
            currentDamageMethodClass.damageTick(target);

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
                }
                else
                {
                    if (activeBuffs[i].buffName == GameManager.BuffNames.Stun) //Unstun tower when stun buff is over
                        isStunned = false;
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
                case TowerType.Basic:
                    switch (upgradeLevel1)
                    {
                        case 0:
                            //Do upgrade
                            range += 1f;
                            rangeObject.localScale = new Vector3(rangeObject.localScale.x + 2f, rangeObject.localScale.y, rangeObject.localScale.z + 2f);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeCost1 = GetUpgradeCost(upgradeLevel1, 1);
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);
                            break;
                        case 1:
                            //Do upgrade
                            range += 2f;
                            rangeObject.localScale = new Vector3(rangeObject.localScale.x + 4f, rangeObject.localScale.y, rangeObject.localScale.z + 4f);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeCost1 = GetUpgradeCost(upgradeLevel1, 1);
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);
                            break;
                        case 2:
                            //Do upgrade
                            range += 3f;
                            rangeObject.localScale = new Vector3(rangeObject.localScale.x + 6f, rangeObject.localScale.y, rangeObject.localScale.z + 6f);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);
                            break;
                    }
                    break;
                case TowerType.Bomb:
                    switch (upgradeLevel1)
                    {
                        case 0:
                            //Do upgrade
                            damage += 1f;
                            transform.GetComponent<MissileDamage>().UpdateDamage(damage);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeCost1 = GetUpgradeCost(upgradeLevel1, 1);
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);
                            break;
                        case 1:
                            //Do upgrade
                            damage += 3f;
                            transform.GetComponent<MissileDamage>().UpdateDamage(damage);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeCost1 = GetUpgradeCost(upgradeLevel1, 1);
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);
                            break;
                        case 2:
                            //Do upgrade
                            damage += 4f;
                            transform.GetComponent<MissileDamage>().UpdateDamage(damage);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);
                            break;
                    }
                    break;
                case TowerType.Flame:
                    switch (upgradeLevel1)
                    {
                        case 0:
                            //Do upgrade
                            fireRate += 0.2f;
                            transform.GetComponent<FireDamage>().UpdateFireRate(fireRate);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeCost1 = GetUpgradeCost(upgradeLevel1, 1);
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);
                            break;
                        case 1:
                            //Do upgrade
                            fireRate += 0.3f;
                            transform.GetComponent<FireDamage>().UpdateFireRate(fireRate);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;

                            upgradeCost1 = GetUpgradeCost(upgradeLevel1, 1);
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);

                            break;
                        case 2:
                            //Do upgrade
                            fireRate += 1f;
                            transform.GetComponent<FireDamage>().UpdateFireRate(fireRate);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);
                            break;
                    }
                    break;
                case TowerType.Economy:
                    switch (upgradeLevel1)
                    {
                        case 0:
                            transform.GetComponent<EconomyBehavior>().bonus = 100;
                            GameManager.Instance.farmBonus += 50;
                            sellCost += upgradeCost1 / 2;
                            upgradeCost1 = GetUpgradeCost(upgradeLevel1, 1);
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);
                            break;
                        case 1:
                            transform.GetComponent<EconomyBehavior>().bonus = 200;
                            GameManager.Instance.farmBonus += 100;
                            sellCost += upgradeCost1 / 2;
                            upgradeCost1 = GetUpgradeCost(upgradeLevel1, 1);
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);
                            break;
                        case 2:
                            transform.GetComponent<EconomyBehavior>().bonus = 400;
                            GameManager.Instance.farmBonus += 300;

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);
                            break;
                    }
                    break;
                case TowerType.Ice:
                    IceDamage tempIce = transform.gameObject.GetComponent<IceDamage>();
                    switch (upgradeLevel1)
                    {
                        case 0:
                            //Do upgrade
                            range += 1.5f;
                            rangeObject.localScale = new Vector3(rangeObject.localScale.x + 3f, rangeObject.localScale.y, rangeObject.localScale.z + 3f);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeCost1 = GetUpgradeCost(upgradeLevel1, 1);
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);
                            break;
                        case 1:
                            //Do upgrade
                            fireRate += 0.25f;
                            transform.GetComponent<IceDamage>().UpdateFireRate(fireRate);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeCost1 = GetUpgradeCost(upgradeLevel1, 1);
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);
                            break;
                        case 2:
                            //Do upgrade
                            tempIce.UpdateSnowSpeed(tempIce.GetSnowSpeed() - 0.1f);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);
                            break;
                    }
                    break;
                case TowerType.Support:
                    SupportBehavior support = transform.gameObject.GetComponent<SupportBehavior>();
                    switch (upgradeLevel1)
                    {
                        case 0:
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
                            break;
                        case 1:
                            //Hidden
                            //TODO
                            support.UpdateTowersInRange();

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeCost1 = GetUpgradeCost(upgradeLevel1, 1);
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);
                            break;
                        case 2:
                            //Do upgrade
                            support.fireRateBuff += 1.2f;
                            support.UpdateTowersInRange();

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);
                            break;
                    }
                    break;
                case TowerType.Spikes:
                    switch (upgradeLevel1)
                    {
                        case 0:
                            //Do upgrade
                            fireRate += 0.5f;
                            transform.GetComponent<SpikeDamage>().UpdateFireRate(damage);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeCost1 = GetUpgradeCost(upgradeLevel1, 1);
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);
                            break;
                        case 1:
                            //Do upgrade
                            //Do upgrade
                            fireRate += 0.5f;
                            transform.GetComponent<SpikeDamage>().UpdateFireRate(damage);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeCost1 = GetUpgradeCost(upgradeLevel1, 1);
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);
                            break;
                        case 2:
                            //Do upgrade
                            fireRate += 1f;
                            transform.GetComponent<SpikeDamage>().UpdateFireRate(damage);

                            //Set up for next upgrade
                            sellCost += upgradeCost1 / 2;
                            upgradeDescription1 = GetUpgradeName(upgradeLevel1, 1);
                            break;
                    }
                    break;
            }
            upgradeLevel1++;
            UpdateUpgradePanel();
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
                case TowerType.Basic:
                    switch (upgradeLevel2)
                    {
                        case 0:
                            //Do upgrade
                            damage += 1f;
                            transform.GetComponent<StandardDamage>().UpdateDamage(damage);

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeCost2 = GetUpgradeCost(upgradeLevel2, 2);
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);
                            break;
                        case 1:
                            //Do upgrade
                            damage += 2f;
                            transform.GetComponent<StandardDamage>().UpdateDamage(damage);

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeCost2 = GetUpgradeCost(upgradeLevel2, 2);
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);
                            break;
                        case 2:
                            //Do upgrade
                            damage += 7;
                            transform.GetComponent<StandardDamage>().UpdateDamage(damage);

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);
                            break;
                    }
                    break;
                case TowerType.Bomb:
                    switch (upgradeLevel2)
                    {
                        case 0:
                            //Do upgrade
                            fireRate += 0.07f;
                            transform.GetComponent<MissileDamage>().UpdateFireRate(fireRate);

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeCost2 = GetUpgradeCost(upgradeLevel2, 2);
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);
                            break;
                        case 1:
                            //Do upgrade
                            fireRate += 0.1f;
                            transform.GetComponent<MissileDamage>().UpdateFireRate(fireRate);

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeCost2 = GetUpgradeCost(upgradeLevel2, 2);
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);
                            break;
                        case 2:
                            //Do upgrade
                            fireRate += 0.2f;
                            transform.GetComponent<MissileDamage>().UpdateFireRate(fireRate);

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);
                            break;
                    }
                    break;
                case TowerType.Flame:
                    switch (upgradeLevel2)
                    {
                        case 0:
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
                            break;
                        case 1:
                            //Do upgrade
                            damage += 0.05f;
                            transform.GetComponent<FireDamage>().UpdateDamage(damage);

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;

                            upgradeCost2 = GetUpgradeCost(upgradeLevel2, 2);
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);
                            break;
                        case 2:
                            //Do upgrade
                            damage += 0.1f;

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);
                            break;
                    }
                    break;
                case TowerType.Economy:
                    switch (upgradeLevel2)
                    {
                        case 0:
                            //Do upgrade
                            //TODO

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeCost2 = GetUpgradeCost(upgradeLevel2, 2);
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);
                            break;
                        case 1:
                            //Do upgrade
                            //TODO

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeCost2 = GetUpgradeCost(upgradeLevel2, 2);
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);
                            break;
                        case 2:
                            //Do upgrade
                            //TODO

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);
                            break;
                    }
                    break;
                case TowerType.Ice:
                    IceDamage tempIce = transform.gameObject.GetComponent<IceDamage>();
                    switch (upgradeLevel2)
                    {
                        case 0:
                            //Do upgrade
                            tempIce.UpdateSnowSpeedReduction(tempIce.GetSnowSpeedReduction() - 0.05f);

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeCost2 = GetUpgradeCost(upgradeLevel2, 2);
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);
                            break;
                        case 1:
                            //Do upgrade
                            tempIce.UpdateSnowSpeedReduction(tempIce.GetSnowSpeedReduction() - 0.05f);

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeCost2 = GetUpgradeCost(upgradeLevel2, 2);
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);
                            break;
                        case 2:
                            //Do upgrade
                            tempIce.UpdateSnowSpeedReduction(tempIce.GetSnowSpeedReduction() - 0.1f);

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);
                            break;
                    }
                    break;
                case TowerType.Support:
                    SupportBehavior support = transform.gameObject.GetComponent<SupportBehavior>();
                    switch (upgradeLevel2)
                    {
                        case 0:
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
                            break;
                        case 1:
                            //Hidden
                            //TODO
                            support.UpdateTowersInRange();

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeCost2 = GetUpgradeCost(upgradeLevel2, 2);
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);
                            break;
                        case 2:
                            //Do upgrade
                            support.fireRateBuff += 1.2f;
                            support.UpdateTowersInRange();

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);
                            break;
                    }
                    break;
                case TowerType.Spikes:
                    switch (upgradeLevel2)
                    {
                        case 0:
                            //Do upgrade
                            transform.localScale += new Vector3(0.25f, 0, 0.25f);

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeCost1 = GetUpgradeCost(upgradeLevel2, 2);
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);
                            break;
                        case 1:
                            //Do upgrade
                            transform.localScale += new Vector3(0.25f, 0, 0.25f);

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeCost2 = GetUpgradeCost(upgradeLevel2, 2);
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);
                            break;
                        case 2:
                            //Do upgrade
                            transform.localScale += new Vector3(0.5f, 0, 0.5f);

                            //Set up for next upgrade
                            sellCost += upgradeCost2 / 2;
                            upgradeDescription2 = GetUpgradeName(upgradeLevel2, 2);
                            break;
                    }
                    break;
            }
            upgradeLevel2++;
            UpdateUpgradePanel();
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
                case TowerType.Basic:
                    switch (upgradeLevel3)
                    {
                        case 0:
                            //Do upgrade
                            fireRate += 1;
                            transform.GetComponent<StandardDamage>().UpdateFireRate(fireRate);

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeCost3 = GetUpgradeCost(upgradeLevel3, 3);
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);
                            break;
                        case 1:
                            //Do upgrade
                            fireRate += 2;
                            transform.GetComponent<StandardDamage>().UpdateFireRate(fireRate);

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeCost3 = GetUpgradeCost(upgradeLevel3, 3);
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);
                            break;
                        case 2:
                            //Do upgrade
                            fireRate += 2;
                            transform.GetComponent<StandardDamage>().UpdateFireRate(fireRate);

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);
                            break;
                    }
                    break;
                case TowerType.Bomb:
                    switch (upgradeLevel3)
                    {
                        case 0:
                            //Do upgrade
                            transform.GetComponent<MissileDamage>().explosionRadius += 1;

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeCost3 = GetUpgradeCost(upgradeLevel3, 3);
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);
                            break;
                        case 1:
                            //Do upgrade
                            transform.GetComponent<MissileDamage>().explosionRadius += 3;

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeCost3 = GetUpgradeCost(upgradeLevel3, 3);
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);
                            break;
                        case 2:
                            //Do upgrade
                            transform.GetComponent<MissileDamage>().explosionRadius += 5;

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);
                            break;
                    }
                    break;
                case TowerType.Flame:
                    switch (upgradeLevel3)
                    {
                        case 0:
                            //Do upgrade
                            transform.GetComponentInChildren<FireTriggerCollisionDetector>().duration += 1;

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeCost3 = GetUpgradeCost(upgradeLevel3, 3);
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);
                            break;
                        case 1:
                            //Do upgrade
                            transform.GetComponentInChildren<FireTriggerCollisionDetector>().duration += 2;

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;

                            upgradeCost3 = GetUpgradeCost(upgradeLevel3, 3);
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);

                            break;
                        case 2:
                            //Do upgrade
                            transform.GetComponentInChildren<FireTriggerCollisionDetector>().duration += 4;

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);
                            break;
                    }
                    break;
                case TowerType.Economy:
                    switch (upgradeLevel3)
                    {
                        case 0:
                            //Do upgrade
                            transform.GetComponent<EconomyBehavior>().investmentsPercent += 0.05f;
                            transform.GetComponent<EconomyBehavior>().UpdateTowersInRange();

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeCost3 = GetUpgradeCost(upgradeLevel3, 3);
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);
                            break;
                        case 1:
                            //Do upgrade
                            transform.GetComponent<EconomyBehavior>().investmentsPercent += 0.05f;
                            transform.GetComponent<EconomyBehavior>().UpdateTowersInRange();

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeCost3 = GetUpgradeCost(upgradeLevel3, 3);
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);
                            break;
                        case 2:
                            //Do upgrade
                            transform.GetComponent<EconomyBehavior>().investmentsPercent += 0.1f;
                            transform.GetComponent<EconomyBehavior>().UpdateTowersInRange();

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);
                            break;
                    }
                    break;
                case TowerType.Ice:
                    IceDamage tempIce = transform.gameObject.GetComponent<IceDamage>();
                    switch (upgradeLevel3)
                    {
                        case 0:
                            //Do upgrade
                            tempIce.UpdateSnowSize(tempIce.GetSnowSize() + 0.5f);

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeCost3 = GetUpgradeCost(upgradeLevel3, 3);
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);
                            break;
                        case 1:
                            //Do upgrade
                            tempIce.UpdateSnowSize(tempIce.GetSnowSize() + 0.5f);

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeCost3 = GetUpgradeCost(upgradeLevel3, 3);
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);
                            break;
                        case 2:
                            //Do upgrade
                            tempIce.UpdateSnowSize(tempIce.GetSnowSize() + 1f);

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);
                            break;
                    }
                    break;
                case TowerType.Support:
                    SupportBehavior support = transform.gameObject.GetComponent<SupportBehavior>();
                    switch (upgradeLevel3)
                    {
                        case 0:
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
                            break;
                        case 1:
                            //Hidden
                            //TODO
                            support.UpdateTowersInRange();

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeCost3 = GetUpgradeCost(upgradeLevel3, 3);
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);
                            break;
                        case 2:
                            //Do upgrade
                            support.fireRateBuff += 1.2f;
                            support.UpdateTowersInRange();

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);
                            break;
                    }
                    break;
                case TowerType.Spikes:
                    switch (upgradeLevel3)
                    {
                        case 0:
                            //Do upgrade
                            damage += 0.05f;
                            transform.GetComponent<SpikeDamage>().UpdateDamage(damage);

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeCost3 = GetUpgradeCost(upgradeLevel3, 3);
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);
                            break;
                        case 1:
                            //Do upgrade
                            damage += 0.05f;
                            transform.GetComponent<SpikeDamage>().UpdateDamage(damage);

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeCost3 = GetUpgradeCost(upgradeLevel3, 3);
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);
                            break;
                        case 2:
                            //Do upgrade
                            damage += 0.1f;
                            transform.GetComponent<SpikeDamage>().UpdateDamage(damage);

                            //Set up for next upgrade
                            sellCost += upgradeCost3 / 2;
                            upgradeDescription3 = GetUpgradeName(upgradeLevel3, 3);
                            break;
                    }
                    break;
            }
            upgradeLevel3++;
            UpdateUpgradePanel();
        }
    }
    #endregion
    #endregion

    public void UpdateUpgradePanel()
    {
        upgradePanel.SetTarget(this, (int)targetType);
        upgradePanel.SetSellButton(sellCost);
        upgradePanel.SetText(upgradeDescription1, 1);
        upgradePanel.SetText(upgradeDescription2, 2);
        upgradePanel.SetText(upgradeDescription3, 3);
        upgradePanel.ToggleUpgradeButton(upgradeLevel1 != 3, 1);
        upgradePanel.ToggleUpgradeButton(upgradeLevel2 != 3, 2);
        upgradePanel.ToggleUpgradeButton(upgradeLevel3 != 3, 3);
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
            TowerType.Basic, new List<UpgradeData>
            {
                new UpgradeData("Scopier Scopes\n" + "Cost: " + 150, 150, "Increases Range"),
                new UpgradeData("Scopiest Scopes\n" + "Cost: " + 400, 400, "Increases Range"),
                new UpgradeData("Max Level", 0, "Increases Range")
            }
        },
        {
            TowerType.Bomb, new List<UpgradeData>
            {
                new UpgradeData("More Powder\n" + "Cost: " + 350, 250, "Increases Damage"),
                new UpgradeData("BOOM.\n" + "Cost: " + 800, 800, "Increases Damage"),
                new UpgradeData("Max Level", 0, "Increases Damage")
            }
        },
        {
            TowerType.Flame, new List<UpgradeData>
            {
                new UpgradeData("Quicker Burns\n" + "Cost: " + 900, 900, "Increases Burn Rate"),
                new UpgradeData("Nonstop Burns\n" + "Cost: " + 2000, 2000, "Increases Burn Rate"),
                new UpgradeData("Max Level", 0, "Increases Burn Rate")
            }
        },
        {
            TowerType.Economy, new List<UpgradeData>
            {
                new UpgradeData("Bigger Mine\n" + "Cost: " + 500, 500, "Increases End of Wave Bonus"),
                new UpgradeData("Chain Company\n" + "Cost: " + 750, 750, "Increases End of Wave Bonus"),
                new UpgradeData("Max Level", 0, "Increases End of Wave Bonus")
            }
        },
        {
            TowerType.Ice, new List<UpgradeData>
            {
                new UpgradeData("Faster firing\n" + "Cost: " + 200, 200, "Increases Range"),
                new UpgradeData("Aerodynamic\n" + "Cost: " + 250, 250, "Increases Attack Speed"),
                new UpgradeData("Max Level", 0, "Decreases Snowball travel time")
            }
        },
        {
            TowerType.Support, new List<UpgradeData>
            {
                new UpgradeData("Proximity Sensor (Not Implemented Yet)\n" + "Cost: " + 0, 0, "Increases Range and Range Buff"),
                new UpgradeData("Inspiration\n" + "Cost: " + 300, 300, "Increases Range Buff"),
                new UpgradeData("Max Level", 0, "")
            }
        },
        {
            TowerType.Spikes, new List<UpgradeData>
            {
                new UpgradeData("Faster Spikes\n" + "Cost: " + 300, 300, "Increases Rate of Damage"),
                new UpgradeData("Deadly Spikes\n" + "Cost: " + 450, 450, "Increases Rate of Damage"),
                new UpgradeData("Max Level", 0, "Increases Rate of Damage")
            }
        }
    };
    #endregion
    #region Upgrade 2
    private static readonly Dictionary<TowerType, List<UpgradeData>> upgradeDataMap2 = new()
    {
        //Description and Cost
        {
            TowerType.Basic, new List<UpgradeData>
            {
                new UpgradeData("Stronger Arrows\n" + "Cost: " + 450, 450, "Increases Damage"),
                new UpgradeData("Barbed Arrows\n" + "Cost: " + 1000, 1000, "Increases Damage"),
                new UpgradeData("Max Level", 0, "Increases Damage")
            }
        },
        {
            TowerType.Bomb, new List<UpgradeData>
            {
                new UpgradeData("Fastest Reload\n" + "Cost: " + 400, 400, "Increases Attack Speed"),
                new UpgradeData("Automated Reload\n" + "Cost: " + 500, 500, "Increases Attack Speed"),
                new UpgradeData("Max Level", 0, "Increases Attack Speed")
            }
        },
        {
            TowerType.Flame, new List<UpgradeData>
            {
                new UpgradeData("Burny Burns\n" + "Cost: " + 500, 500, "Increases Range"),
                new UpgradeData("Hellfire\n" + "Cost: " + 1200, 1200, "Increases Damage"),
                new UpgradeData("Max Level", 0, "Increases Damage")
            }
        },
        {
            TowerType.Economy, new List<UpgradeData>
            {
                new UpgradeData("Better Deals (Not Implemented)\n" + "Cost: " + 0, 0, "Gives additional End of Wave Bonus based off current money (Only highest Value Applied)"),
                new UpgradeData("Compound Interest (Not Implemented)\n" + "Cost: " + 0, 0, "Increases Interest Effect"),
                new UpgradeData("Max Level", 0, "Increases Interest Effect")
            }
        },
        {
            TowerType.Ice, new List<UpgradeData>
            {
                new UpgradeData("Freezing Snow\n" + "Cost: " + 400, 400, "Increases Slow Effect"),
                new UpgradeData("Frostbite\n" + "Cost: " + 650, 650, "Increases Slow Effect"),
                new UpgradeData("Max Level", 0, "Increases Slow Effect")
            }
        },
        {
            TowerType.Support, new List<UpgradeData>
            {
                new UpgradeData("Proximity Sensor (Not Implemented Yet)\n" + "Cost: " + 0, 0, "Grants Damage Buff to nearby towers"),
                new UpgradeData("Inspiration\n" + "Cost: " + 300, 300, "Increases Damage Buff"),
                new UpgradeData("Max Level", 0, "Increases Damage Buff")
            }
        },
        {
            TowerType.Spikes, new List<UpgradeData>
            {
                new UpgradeData("Even More Spikes\n" + "Cost: " + 400, 400, "Increases size"),
                new UpgradeData("Carpet of Spikes\n" + "Cost: " + 650, 650, "Increases size"),
                new UpgradeData("Max Level", 0, "Increases size")
            }
        }
    };
    #endregion
    #region Upgrade 3
    private static readonly Dictionary<TowerType, List<UpgradeData>> upgradeDataMap3 = new()
    {
        //Description and Cost
        {
            TowerType.Basic, new List<UpgradeData>
            {
                new UpgradeData("Faster Mechanism\n" + "Cost: " + 300, 300, "Increases Attack Speed"),
                new UpgradeData("Automatic Firing\n" + "Cost: " + 850, 850, "Increases Attack Speed"),
                new UpgradeData("Max Level", 0, "Increases Attack Speed")
            }
        },
        {
            TowerType.Bomb, new List<UpgradeData>
            {
                new UpgradeData("Compressed Charge\n" + "Cost: " + 600, 600, "Increases Explosion Radius"),
                new UpgradeData("Atomic Explosion\n" + "Cost: " + 1300, 1300, "Increases Explosion Radius"),
                new UpgradeData("Max Level", 0, "Increases Explosion Radius")
            }
        },
        {
            TowerType.Flame, new List<UpgradeData>
            {
                new UpgradeData("Hell's Itch\n" + "Cost: " + 600, 600, "Increases Burn Duration"),
                new UpgradeData("Third Degree Burns\n" + "Cost: " + 1200, 1200, "Increases Burn Duration"),
                new UpgradeData("Max Level", 0, "Increases Burn Duration")
            }
        },
        {
            TowerType.Economy, new List<UpgradeData>
            {
                new UpgradeData("Stocks\n" + "Cost: " + 750, 750, "Grants Money Earned Buff to nearby towers"),
                new UpgradeData("Majority Owner\n" + "Cost: " + 1000, 1000, "Increases Money Earned Buff"),
                new UpgradeData("Max Level", 0, "Increases Money Earned Buff")
            }
        },
        {
            TowerType.Ice, new List<UpgradeData>
            {
                new UpgradeData("Biggest Snowball\n" + "Cost: " + 250, 250, "Increases snow area"),
                new UpgradeData("Carpet Bomb\n" + "Cost: " + 500, 500, "Increases snow area"),
                new UpgradeData("Max Level", 0, "Increases snow area")
            }
        },
        {
            TowerType.Support, new List<UpgradeData>
            {
                new UpgradeData("Proximity Sensor (Not Implemented Yet)\n" + "Cost: " + 0, 0, "Grants Attack Speed Buff to nearby towers"),
                new UpgradeData("Inspiration\n" + "Cost: " + 300, 300, "Grants Detect Invisible to nearby towers"),
                new UpgradeData("Max Level", 0, "Increases Attack Speed Buff to nearby Towers")
            }
        },
        {
            TowerType.Spikes, new List<UpgradeData>
            {
                new UpgradeData("Sharper Spikes\n" + "Cost: " + 300, 300, "Increases Damage"),
                new UpgradeData("LEGOS\n" + "Cost: " + 400, 400, "Increases Damage"),
                new UpgradeData("Max Level", 0, "Increases Damage")
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
        Basic,
        Flame,
        Bomb,
        Economy,
        Ice,
        Support,
        Spikes
    }
}