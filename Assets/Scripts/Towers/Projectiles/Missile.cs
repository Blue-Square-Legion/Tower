using System;
using System.Collections;
using UnityEngine;

public class Missile : MonoBehaviour
{   
    private ParticleSystem explosionSystem;
    public float explosionRadius;

    GameManager gameManager;
    EnemySpawner enemySpawner;

    [NonSerialized] public GameObject parent;
    private MissileDamage parentClass;
    private float speed;
    float time;

    public void Init()
    {
        if (parent != null)
        {
            gameManager = GameManager.Instance;
            enemySpawner = EnemySpawner.Instance;
            speed = 0.5f;
            time = 0;
            explosionSystem = parent.GetComponentInChildren<ParticleSystem>();
            parentClass = parent.GetComponent<MissileDamage>();

            StartCoroutine(Fire());
        }
    }

    IEnumerator Fire()
    {
        while (time < 5)
        {
            transform.position += transform.forward * speed;
            yield return new WaitForEndOfFrame();
        }
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
