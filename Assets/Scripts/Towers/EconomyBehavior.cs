using System.Collections.Generic;
using UnityEngine;

public class EconomyBehavior : TowerDamage
{
    public int bonus;
    public float interestPercent;
    public float investmentsPercent;
    private List<TowerBehavior> towersInRange;

    public override void Init(float damage, float fireRate)
    {
        gameManager = GameManager.Instance;
        bonus = 50;
        gameManager.farmBonus += bonus;
        towersInRange = new();
    }

    public void UpdateTowersInRange()
    {
        //Find all tower objects within range
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, transform.GetComponent<TowerBehavior>().range, Vector3.forward, 0, LayerMask.NameToLayer("Tower"));

        //Clears list
        towersInRange.Clear();
        int count = hits.Length;
        for (int i = 0; i < count; i++)
        {
            //Filters out everything that is not a Tower and filters out itself
            if (hits[i].collider.gameObject.TryGetComponent(out TowerBehavior towerBehavior) && hits[i].collider != gameObject.GetComponent<Collider>())
                towersInRange.Add(towerBehavior); //Adds object to list
        }
        print(investmentsPercent);
        if (investmentsPercent > 1) //Checks if investment percent was set correctly (so you don't lose money) and that it is higher than its base value
        {
            int towersInRangeCount = towersInRange.Count;
            for (int i = 0; i < towersInRangeCount; i++)
            {
                GameManager.Buff buff = new GameManager.Buff(GameManager.BuffNames.Investments, investmentsPercent, -123, false);
                GameManager.ApplyBuffData buffData = new GameManager.ApplyBuffData(buff, towersInRange[i]);
                gameManager.EnqueueBuffToApply(buffData);
            }
        }
    }

    public void RemoveBuffs()
    {
        //Removes Buffs
        if (investmentsPercent > 1)
        {
            int towersInRangeCount = towersInRange.Count;
            for (int i = 0; i < towersInRangeCount; i++)
            {
                GameManager.Buff buff = new GameManager.Buff(GameManager.BuffNames.Investments, investmentsPercent, -123, false);
                GameManager.ApplyBuffData buffData = new GameManager.ApplyBuffData(buff, towersInRange[i]);
                gameManager.EnqueueBuffToRemove(buffData);
            }
        }
    }

    public override void DamageTick(Enemy target)
    {

    }
}