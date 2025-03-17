using System;
using UnityEngine;

public class BarracksBehavior : MonoBehaviour
{
    [NonSerialized] public int upgradeLevel1;
    [NonSerialized] public int upgradeLevel2;
    [NonSerialized] public int upgradeLevel3;

    [SerializeField] private GameObject prefab1;
    [SerializeField] private GameObject prefab2;
    [SerializeField] private GameObject prefab3;
    [SerializeField] private GameObject prefab4;
    [SerializeField] private GameObject prefab5;
    [SerializeField] private GameObject prefab6;

    private TowerBehavior towerBehavior;
    void Start()
    {
        upgradeLevel1 = 1;
        upgradeLevel2 = 1;
        upgradeLevel3 = 1;

        towerBehavior = GetComponent<TowerBehavior>();
    }

    public void UpdateUpgrades()
    {
        upgradeLevel1 = towerBehavior.upgradeLevel1;
        upgradeLevel2 = towerBehavior.upgradeLevel2;
        upgradeLevel3 = towerBehavior.upgradeLevel3;
    }

    public void SummonUnit()
    {
        if (upgradeLevel1 > upgradeLevel2 && upgradeLevel1 > upgradeLevel3)
        {
            if (upgradeLevel2 >  upgradeLevel3)
            {
                Instantiate(prefab1, transform.position, transform.rotation, transform);
            }
            else
            {
                Instantiate(prefab2, transform.position, transform.rotation, transform);
            }
        }
        else if (upgradeLevel2 > upgradeLevel1 && upgradeLevel2 > upgradeLevel3)
        {
            if (upgradeLevel1 > upgradeLevel3)
            {
                Instantiate(prefab3, transform.position, transform.rotation, transform);
            }
            else
            {
                Instantiate(prefab4, transform.position, transform.rotation, transform);
            }
        }
        else if (upgradeLevel3 > upgradeLevel1 && upgradeLevel3 > upgradeLevel2)
        {
            if (upgradeLevel1 > upgradeLevel2)
            {
                Instantiate(prefab5, transform.position, transform.rotation, transform);
            }
            else
            {
                Instantiate(prefab6, transform.position, transform.rotation, transform);
            }
        }
    }
}