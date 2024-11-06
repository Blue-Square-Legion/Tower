using System.Collections.Generic;
using UnityEngine;

public class FireTriggerCollisionDetector : MonoBehaviour
{
    [SerializeField] private FlameThrowerDamage parent;

    GameManager gameManager;
    private EnemySpawner enemySpawner;
    public float duration;
    public float speedModifier;

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
            applyEffects();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            enemiesInRange.Remove(other); // Remove enemy from list
        }
    }

    private void Update() {
        applyEffects();
       
    }

    private void applyEffects()
    {
        foreach (var collider in enemiesInRange)
        {
            Debug.Log($"Number of enemies in range: {enemiesInRange.Count}");
            GameManager.Effect onFire = new GameManager.Effect(GameManager.EffectNames.Fire, parent.fireRate, parent.damage, duration, speedModifier);
            GameManager.ApplyEffectData effectData = new GameManager.ApplyEffectData(onFire, enemySpawner.enemyTransformDictionary[collider.transform]);
            gameManager.EnqueueAffectToApply(effectData);
            Debug.Log($"Applying effect to: {collider.name}");
        }
    }
}