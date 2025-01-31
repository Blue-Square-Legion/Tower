using System.Collections.Generic;
using UnityEngine;

public class MissileCollisionDetector : MonoBehaviour
{
    [SerializeField] private MissileDamage parentClass;
    [SerializeField] private ParticleSystem explosionSystem;
    [SerializeField] private ParticleSystem missileSystem;
    public float explosionRadius;
    private List<ParticleCollisionEvent> missileCollisions;

    GameManager gameManager;
    EnemySpawner enemySpawner;
    private void Start()
    {
        gameManager = GameManager.Instance;
        enemySpawner = EnemySpawner.Instance;
        missileCollisions = new List<ParticleCollisionEvent>();
    }

    private void OnParticleCollision(GameObject other)
    {

        missileSystem.GetCollisionEvents(other, missileCollisions);
        int collisionCount = missileCollisions.Count;

        for (int i = 0; i < collisionCount; i++)
        {
            explosionSystem.transform.position = missileCollisions[i].intersection;
            explosionSystem.Play();

            Collider[] enemiesInRadius = Physics.OverlapSphere(missileCollisions[i].intersection, explosionRadius, parentClass.enemiesLayer);

            int enemiesInRadiusCount = enemiesInRadius.Length;
            for (int j = 0; j < enemiesInRadiusCount; j++)
            {
                Enemy enemyToDamage = enemySpawner.enemyTransformDictionary[enemiesInRadius[j].transform];
                GameManager.EnemyDamageData damageToApply = new GameManager.EnemyDamageData(enemyToDamage, 
                    parentClass.damage, enemyToDamage.damageResistance, parentClass.GetComponent<TowerBehavior>()); //TODO - Switch to Explosion Resistance
                gameManager.EnqueueDamageData(damageToApply);
            }
        }
    }
}
