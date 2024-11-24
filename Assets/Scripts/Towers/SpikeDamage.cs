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
        gameManager = GameManager.Instance;
        this.damage = damage;
        this.fireRate = fireRate;
        delay = 1f / fireRate;
    }
    public void UpdateDamage(float damage)
    {
        this.damage = damage;
    }

    public void UpdateFireRate(float fireRate)
    {
        this.fireRate = fireRate;
    }

    public void damageTick(Enemy target)
    {
        
        if (target)
        {
            if (delay > 0)
            {
                delay -= Time.deltaTime;
                return;
            }

            DealDamage();

            delay = 1 / fireRate;
        }

        void DealDamage()
        {
            Collider[] enemies = Physics.OverlapSphere(transform.position, 2f);

            foreach (Collider enemy in enemies)
            {
                if (enemy.CompareTag("Enemy"))
                {
                    enemy.GetComponent<Enemy>().TakeDamage(damage);
                }
            }
        }
    }
}

