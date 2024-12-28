using UnityEngine;

public class FireTriggerCollisionDetector : MonoBehaviour
{
    [SerializeField] private FireDamage parent;

    GameManager gameManager;
    private EnemySpawner enemySpawner;
    public float duration;
    
    private void Start()
    {
        gameManager = GameManager.Instance;
        enemySpawner = EnemySpawner.Instance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            GameManager.EnemyBuff onFire = new GameManager.EnemyBuff(GameManager.EnemyBuffNames.Burn,
                parent.damage, parent.fireRate, 0, duration, true, GetComponent<TowerBehavior>());
            GameManager.ApplyEnemyBuffData effectData = new GameManager.ApplyEnemyBuffData(onFire, enemySpawner.enemyTransformDictionary[other.transform]);
            gameManager.EnqueueEnemyBuffToApply(effectData);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            GameManager.EnemyBuff onFire = new GameManager.EnemyBuff(GameManager.EnemyBuffNames.Burn,
                parent.damage, parent.fireRate, 0, duration, true, GetComponent<TowerBehavior>());
            GameManager.ApplyEnemyBuffData effectData = new GameManager.ApplyEnemyBuffData(onFire, enemySpawner.enemyTransformDictionary[other.transform]);
            gameManager.EnqueueEnemyBuffToApply(effectData);
        }
    }
}