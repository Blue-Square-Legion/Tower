using System;
using System.Collections;
using UnityEngine;

public class Missile : MonoBehaviour
{   
    private ParticleSystem explosionSystem;
    [NonSerialized] public float explosionRadius;

    GameManager gameManager;
    EnemySpawner enemySpawner;

    [NonSerialized] public GameObject parent;
    [NonSerialized] public Enemy target;
    private MissileDamage parentClass;
    private float speed;
    private float time;
    private Vector3 direction;
    private Transform targetTransform;
    public void Init()
    {
        if (parent != null && target != null)
        {
            gameManager = GameManager.Instance;
            enemySpawner = EnemySpawner.Instance;
            speed = 0.5f;
            time = 0;
            explosionSystem = parent.GetComponentInChildren<ParticleSystem>();
            explosionSystem.startSize = explosionRadius;
            parentClass = parent.GetComponent<MissileDamage>();
            targetTransform = target.gameObject.transform;

            StartCoroutine(Fire());
        }
    }

    IEnumerator Fire()
    {
        while (time < 2)
        {
            if (target.isAlive)
            {
                direction = targetTransform.position - transform.position;
                direction.Normalize();
                transform.rotation = Quaternion.LookRotation(direction);
            }
            
            transform.position += transform.forward * speed;
            time += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        Destroy(gameObject);
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
                GameManager.EnemyDamageData damageToApply = new GameManager.EnemyDamageData(enemyToDamage, parentClass.damage, 
                    enemyToDamage.damageResistance, parent.GetComponent<TowerBehavior>());
                enemyToDamage.lastDamagingTower = parent.GetComponent<TowerBehavior>();
                gameManager.EnqueueDamageData(damageToApply);
            }

            Destroy(gameObject);
        }
    }
}
