using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTriggerCollisionDetector : MonoBehaviour
{
    [SerializeField] private FlameThrowerDamage parent;

    GameManager gameManager;
    private EnemySpawner enemySpawner;
    private void Start()
    {
        gameManager = GameManager.Instance;
        enemySpawner = EnemySpawner.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            GameManager.DamageOverTime onFire = new GameManager.DamageOverTime("Fire", parent.fireRate, parent.damage, 5f);
            GameManager.ApplyDamageOverTimeData dotData = new GameManager.ApplyDamageOverTimeData(onFire, enemySpawner.enemyTransformDictionary[other.transform.parent]);
            gameManager.EnqueueDamageOverTime(dotData);
        }
    }
}