using System.Collections.Generic;
using UnityEngine;

public class SupportBehavior : MonoBehaviour, IDamageMethod
{
    GameManager gameManager;
    [SerializeField] TowerBehavior parent;
    public float attackRangeBuff;
    public float fireRateBuff;
    public float damageBuff;
    private List<TowerBehavior> towersInRange;
    public void Init(float damage, float fireRate)
    {
        gameManager = GameManager.Instance;
        towersInRange = new();
    }

    public void Built()
    {
        UpdateTowersInRange();
    }

    public void UpdateTowersInRange()
    {
        //Find all tower objects within range
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, parent.range, Vector3.forward, 0, LayerMask.NameToLayer("Tower"));

        //Clears list
        towersInRange.Clear();
        int count = hits.Length;
        for (int i = 0; i < count; i++)
        {
            //Filters out everything that is not a Tower and filters out itself
            if (hits[i].collider.gameObject.TryGetComponent(out TowerBehavior towerBehavior) && hits[i].collider != gameObject.GetComponent<Collider>())
                towersInRange.Add(towerBehavior); //Adds object to list
        }
        
        int towersInRangeCount = towersInRange.Count;
        for (int i = 0; i < towersInRangeCount; i++)
        {
            GameManager.Buff buff = new GameManager.Buff(GameManager.BuffNames.SupportBonusRange, attackRangeBuff, -123);
            GameManager.ApplyBuffData buffData = new GameManager.ApplyBuffData(buff, towersInRange[i]);
            gameManager.EnqueBuffToApply(buffData);

            if (fireRateBuff != 0)
            {
                buff = new GameManager.Buff(GameManager.BuffNames.SupportBonusAttackSpeed, fireRateBuff, -123);
                buffData = new GameManager.ApplyBuffData(buff, towersInRange[i]);
                gameManager.EnqueBuffToApply(buffData);
            }
            if (fireRateBuff != 0)
            {
                buff = new GameManager.Buff(GameManager.BuffNames.SupportBonusDamage, fireRateBuff, -123);
                buffData = new GameManager.ApplyBuffData(buff, towersInRange[i]);
                gameManager.EnqueBuffToApply(buffData);
            }
        }
    }

    public void RemoveBuffs()
    {
        //Removes Buffs
        int towersInRangeCount = towersInRange.Count;
        for (int i = 0; i < towersInRangeCount; i++)
        {
            GameManager.Buff buff = new GameManager.Buff(GameManager.BuffNames.SupportBonusRange, attackRangeBuff, -123);
            GameManager.ApplyBuffData buffData = new GameManager.ApplyBuffData(buff, towersInRange[i]);
            gameManager.EnqueBuffToRemove(buffData);

            if (fireRateBuff != 0)
            {
                buff = new GameManager.Buff(GameManager.BuffNames.SupportBonusAttackSpeed, fireRateBuff, -123);
                buffData = new GameManager.ApplyBuffData(buff, towersInRange[i]);
                gameManager.EnqueBuffToRemove(buffData);
            }
            if (fireRateBuff != 0)
            {
                buff = new GameManager.Buff(GameManager.BuffNames.SupportBonusDamage, fireRateBuff, -123);
                buffData = new GameManager.ApplyBuffData(buff, towersInRange[i]);
                gameManager.EnqueBuffToRemove(buffData);
            }
        }
    }

    public void damageTick(Enemy target)
    {

    }
}