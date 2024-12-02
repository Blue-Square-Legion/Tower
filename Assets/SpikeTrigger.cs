using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeTrigger : MonoBehaviour
{
    [SerializeField] SpikeDamage parent;

    GameManager gameManager;
    private float damage;
    private float fireRate;
    private float delay;

    private void Start()
    {
        gameManager = GameManager.Instance;
        damage = parent.damage;
        fireRate = parent.fireRate;
        delay = 1f / fireRate;
    }

    public void UpdateStats()
    {
        damage = parent.damage;
        fireRate = parent.fireRate;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.CompareTag("Enemy"))
        {
            if (delay > 0)
            {
                delay -= Time.deltaTime;
                return;
            }

            Enemy target = other.gameObject.GetComponent<Enemy>();

            target.lastDamagingTower = transform.GetComponent<TowerBehavior>();
            gameManager.EnqueueDamageData(new GameManager.EnemyDamageData(target, damage, target.damageResistance, transform.GetComponent<TowerBehavior>()));

            delay = 1 / fireRate;
        }
    }
}