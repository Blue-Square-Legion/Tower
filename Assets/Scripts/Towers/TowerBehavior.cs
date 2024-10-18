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
    private float delay;
    private TowerPlacement towerPlacement;
    Camera cam;
    public bool isSelected;

    [SerializeField] TowerType towerType;

    private IDamageMethod currentDamageMethodClass;

    private Player player;
    private int upgradeLevel;
    private UpgradePanel upgradePanel;
    private int upgradeCost;
    [NonSerialized] public int sellCost;
    private string upgradeDescription;

    GameObject lastSelectedTower;

    private void Start()
    {
        towerPlacement = TowerPlacement.Instance;
        upgradePanel = UpgradePanel.Instance;
        player = Player.Instance;
        cam = towerPlacement.cam;
        isSelected = true;
        lastSelectedTower = null;

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
        }
        sellCost = cost / 2;
    }

    //Desyncs the towers from regular game loop to prevent errors
    public void Tick()
    {
        currentDamageMethodClass.damageTick(target);

        if (target != null)
        {
            towerPivot.transform.rotation = Quaternion.LookRotation(target.transform.position - transform.position);
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
            
        gameObject.transform.Find("Base").transform.Find("Range").gameObject.SetActive(isSelected);

        if (lastSelectedTower != null)
        {
            //print(lastSelectedTower.transform.Find("Base").transform.Find("Range").gameObject.activeSelf);
            upgradePanel.SetUpgradePanel(lastSelectedTower.transform.Find("Base").transform.Find("Range").gameObject.activeInHierarchy);
            UpdateUpgradePanel();
            lastSelectedTower = null;
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
            Transform rangeObject = transform.Find("Base").transform.Find("Range");
            player.RemoveMoney(upgradeCost);
            switch (towerType)
            {
                case TowerType.Basic:
                    switch (upgradeLevel)
                    {
                        case 0:
                            //Do upgrade
                            range += 1f;
                            rangeObject.localScale = new Vector3(rangeObject.localScale.x + 1.5f, rangeObject.localScale.y, rangeObject.localScale.z + 1.5f);

                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;
                            upgradeCost = 150;
                            upgradeDescription = "Better Arrows\nIncreased Damage";
                            break;
                        case 1:
                            //Do upgrade
                            damage += 1;
                            transform.GetComponent<StandardDamage>().UpdateDamage(damage);
                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;
                            upgradeCost = 300;
                            upgradeDescription = "Faster mechanism\nIncreased fire rate";
                            break;
                        case 2:
                            //Do upgrade
                            fireRate += 1;
                            transform.GetComponent<StandardDamage>().UpdateFireRate(fireRate);

                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;
                            upgradeCost = 300;
                            upgradeDescription = "Scopier Scopes\nIncreased Range";
                            break;
                        case 3:
                            //Do upgrade
                            range += 1.7f;
                            rangeObject.localScale = new Vector3(rangeObject.localScale.x + 2f, rangeObject.localScale.y, rangeObject.localScale.z + 2f);

                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;
                            upgradeCost = 500;
                            upgradeDescription = "Best Efficiency\nIncreased fire rate";
                            break;
                        case 4:
                            //Do Upgrade
                            fireRate *= 2;
                            transform.GetComponent<StandardDamage>().UpdateFireRate(fireRate);

                            //No more upgrades
                            sellCost += upgradeCost / 2;
                            upgradeDescription = "Max Level";
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
                            upgradeCost = 200;
                            upgradeDescription = "Bigger Bombs\nIncreased Explosion Radius";
                            break;
                        case 1:
                            //Do upgrade
                            transform.GetComponentInChildren<MissileCollisionDetector>().explosionRadius += 1;

                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;
                            upgradeCost = 300;
                            upgradeDescription = "Binoculars\nIncreased range";
                            break;
                        case 2:
                            //Do upgrade
                            range += .25f;
                            rangeObject.localScale = new Vector3(rangeObject.localScale.x + .5f, rangeObject.localScale.y, rangeObject.localScale.z + .5f);

                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;
                            upgradeCost = 500;
                            upgradeDescription = "Bigger Bombs\nIncreased Damage\nIncreased Explosion Radius";
                            break;
                        case 3:
                            //Do upgrade
                            damage += 1f;
                            transform.GetComponentInChildren<MissileCollisionDetector>().explosionRadius += 1;
                            transform.GetComponent<MissileDamage>().UpdateDamage(damage);


                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;
                            upgradeCost = 500;
                            upgradeDescription = "Second Bomb Dropper\nIncreased fire rate";
                            break;
                        case 4:
                            //Do Upgrade
                            fireRate *= 2;
                            transform.GetComponent<MissileDamage>().UpdateFireRate(fireRate);

                            //No more upgrades
                            sellCost += upgradeCost / 2;
                            upgradeDescription = "Max Level";
                            break;
                    }
                    break;
                case TowerType.Flame:
                    switch (upgradeLevel)
                    {
                        case 0:
                            //Do upgrade
                            damage += 0.25f;
                            transform.GetComponent<FlameThrowerDamage>().UpdateDamage(damage);
                            transform.GetComponentInChildren<FireTriggerCollisionDetector>().duration += 3;
                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;
                            upgradeCost = 0;
                            upgradeDescription = "Stronger Propellent\nIncreased Range";
                            break;
                        case 1:
                            //Do upgrade
                            range += 1f;
                            Transform fireTrigger = transform.Find("Head").transform.Find("FireTriggerPivot").transform.Find("FireTrigger").transform;
                            fireTrigger.localScale = new Vector3(fireTrigger.localScale.x + 1f, fireTrigger.localScale.y, fireTrigger.localScale.z - 0.5f);
                            fireTrigger.position = new Vector3(fireTrigger.position.x, fireTrigger.position.y, fireTrigger.position.z + 0.5f);
                            rangeObject.localScale = new Vector3(rangeObject.localScale.x + 2f, rangeObject.localScale.y, rangeObject.localScale.z + 4f);
                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;
                            upgradeCost = 0;
                            upgradeDescription = "Better Fuel\nIncreased slow effect";
                            break;
                        case 2:
                            //Do upgrade
                            transform.GetComponentInChildren<FireTriggerCollisionDetector>().speedModifier -= 0.1f;

                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;
                            upgradeCost = 0;
                            upgradeDescription = "Long Lasting Burns\nIncreased Duration";
                            break;
                        case 3:
                            //Do upgrade
                            transform.GetComponentInChildren<FireTriggerCollisionDetector>().duration += 5;

                            //Set up for next upgrade
                            sellCost += upgradeCost / 2;
                            upgradeCost = 0;
                            upgradeDescription = "Even Stronger Fuel\nIncreased Slow Effect";
                            break;
                        case 4:
                            //Do Upgrade
                            transform.GetComponentInChildren<FireTriggerCollisionDetector>().speedModifier -= 0.1f;

                            //No more upgrades
                            sellCost += upgradeCost / 2;
                            upgradeDescription = "Max Level";
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
        upgradePanel.SetTarget(this);
        upgradePanel.SetUpgradeButton(upgradeCost);
        upgradePanel.SetSellButton(sellCost);
        upgradePanel.SetText(upgradeDescription);
        upgradePanel.ToggleUpgradeButton(upgradeLevel != 5);
    }

    public enum TowerType
    {
        Basic,
        Flame,
        Bomb

    }
}