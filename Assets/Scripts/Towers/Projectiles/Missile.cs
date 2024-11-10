using System;
using UnityEngine;

public class Missile : MonoBehaviour
{
    [SerializeField] public float speed;
    [SerializeField] private LayerMask collisionLayer;
    [SerializeField] private MissileDamage parentClass;
    [SerializeField] private ParticleSystem explosionSystem;
    public float explosionRadius;

    GameManager gameManager;
    EnemySpawner enemySpawner;

    private void Start()
    {
        gameManager = GameManager.Instance;
        enemySpawner = EnemySpawner.Instance;
    }
    void Update()
    {
        transform.position += transform.forward * speed;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            explosionSystem.transform.position = transform.position;
            explosionSystem.Play();

            Collider[] enemiesInRadius = Physics.OverlapSphere(transform.position, explosionRadius, parentClass.enemiesLayer);

            int enemiesInRadiusCount = enemiesInRadius.Length;
            for (int j = 0; j < enemiesInRadiusCount; j++)
            {
                Enemy enemyToDamage = enemySpawner.enemyTransformDictionary[enemiesInRadius[j].transform];
                GameManager.EnemyDamageData damageToApply = new GameManager.EnemyDamageData(enemyToDamage, parentClass.damage, enemyToDamage.damageResistance);
                gameManager.EnqueueDamageData(damageToApply);
            }

            Destroy(gameObject);
        }
    }
}
