using System;
using UnityEngine;

public class SpikeDamage : MonoBehaviour, IDamageMethod
{
    GameManager gameManager;
    [SerializeField] private SpikeTrigger child;
    [NonSerialized] public float damage;
    [NonSerialized] public float fireRate;
    private float delay;


    public void Init(float damage, float fireRate)
    {
        gameManager = GameManager.Instance;
        this.damage = damage;
        this.fireRate = fireRate;
        delay = 1 / fireRate;
    }
    public void UpdateDamage(float damage)
    {
        this.damage = damage;
        child.UpdateDamage();
    }

    public void UpdateFireRate(float fireRate)
    {
        this.fireRate = fireRate;
    }

    public void damageTick(Enemy target)
    {
        if (delay > 0)
        {
            delay -= Time.deltaTime;
            return;
        }

        child.damageTick();

        delay = 1 / fireRate;
    }
}