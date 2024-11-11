using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTriggerCollisionDetector : MonoBehaviour
{
    [SerializeField] private FlameThrowerDamage parent;

    GameManager gameManager;
    private EnemySpawner enemySpawner;
    public float duration;
    public float speedModifier;
    public float removalDelay = 1f;

    private List<Collider> enemiesInRange = new List<Collider> ();
    
    private void Start()
    {
        gameManager = GameManager.Instance;
        enemySpawner = EnemySpawner.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            enemiesInRange.Add(other);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            StartCoroutine(RemoveEnemyAfterDelay(other, removalDelay));
        }
    }

    private IEnumerator RemoveEnemyAfterDelay(Collider enemy, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (enemiesInRange.Contains(enemy))
        {
            enemiesInRange.Remove(enemy);
        }
    }

    private void Update()
    {
        if (enemiesInRange.Count > 0)
        {
            applyEffects();
        }
    }

    private void applyEffects()
    {
        foreach (var collider in enemiesInRange)
        {
            if (collider != null && collider.gameObject.activeInHierarchy)
            {
                GameManager.Effect onFire = new GameManager.Effect(GameManager.EffectNames.Fire, parent.fireRate, parent.damage, duration, speedModifier);
                GameManager.ApplyEffectData effectData = new GameManager.ApplyEffectData(onFire, enemySpawner.enemyTransformDictionary[collider.transform]);
                gameManager.EnqueueAffectToApply(effectData);
            }
        }
    }
}
