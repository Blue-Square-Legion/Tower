using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeDamage : MonoBehaviour, IDamageMethod
{
    GameManager gameManager;
    private float damage;
    private float fireRate;
    private float delay;


    public void Init(float damage, float fireRate)
    {
        Debug.Log("init");
        gameManager = GameManager.Instance;
        this.damage = damage;
        this.fireRate = fireRate;
        delay = 1f / fireRate;
    }
    public void UpdateDamage(float damage)
    {
        Debug.Log("update");
        this.damage = damage;
    }

    public void UpdateFireRate(float fireRate)
    {
        Debug.Log("updatefr");
        this.fireRate = fireRate;
    }

    public void damageTick(Enemy target)
    {
        
        if (target)
        {

            if (delay > 0)
            {
                delay -= Time.deltaTim se;
                return;
            }

            target.TakeDamage(damage);

            gameManager.EnqueueDamageData(new GameManager.EnemyDamageData(target, damage, target.damageResistance));

            delay = 1 / fireRate;
        }
    }
}

