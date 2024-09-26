using UnityEngine;

public interface IDamageMethod
{
    public void Init(float damage, float fireRate);
    public void damageTick(Enemy target);
}

public class StandardDamage : MonoBehaviour, IDamageMethod
{
    GameManager gameManager;
    private float damage;
    private float fireRate;
    private float delay;
    public void Init(float damage, float fireRate)
    {
        gameManager = GameManager.Instance;
        this.damage = damage;
        this.fireRate = fireRate;
        delay = 1f / fireRate;
    }

    public void damageTick(Enemy target)
    {
        if (target)
        {
            if (delay > 0)
            {
                delay -= Time.deltaTime;
                return;
            }

            gameManager.EnqueueDamageData(new GameManager.EnemyDamageData(target, damage, target.damageResistance));

            delay = 1 / fireRate;
        }
    }
}