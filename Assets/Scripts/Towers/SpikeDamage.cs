using System;
using UnityEngine;

public class SpikeDamage : MonoBehaviour, IDamageMethod
{
    GameManager gameManager;
    [SerializeField] private SpikeTrigger child;
    [NonSerialized] public float damage;
    [NonSerialized] public float fireRate;


    public void Init(float damage, float fireRate)
    {
        gameManager = GameManager.Instance;
        this.damage = damage;
        this.fireRate = fireRate;
    }
    public void UpdateDamage(float damage)
    {
        this.damage = damage;
        child.UpdateStats();
    }

    public void UpdateFireRate(float fireRate)
    {
        this.fireRate = fireRate;
        child.UpdateStats();
    }

    public void damageTick(Enemy target)
    {

    }
}