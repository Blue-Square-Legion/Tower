using UnityEngine;

public class FireTriggerCollisionDetector : MonoBehaviour
{
    [SerializeField] private FlameThrowerDamage parent;

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
            GameManager.Effect onFire = new GameManager.Effect(GameManager.EffectNames.Fire, parent.damage, duration, parent.fireRate, 1);
            GameManager.ApplyEffectData effectData = new GameManager.ApplyEffectData(onFire, enemySpawner.enemyTransformDictionary[other.transform]);
            gameManager.EnqueueAffectToApply(effectData);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            GameManager.Effect onFire = new GameManager.Effect(GameManager.EffectNames.Fire, parent.damage, duration, parent.fireRate, 1);
            GameManager.ApplyEffectData effectData = new GameManager.ApplyEffectData(onFire, enemySpawner.enemyTransformDictionary[other.transform]);
            gameManager.EnqueueAffectToApply(effectData);
        }
    }
}