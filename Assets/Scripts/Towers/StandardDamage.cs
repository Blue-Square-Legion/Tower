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
        AudioManager.Instance.Add("Crossbow Fire", gameObject);
        AudioManager.Instance.Add("Crossbow Rotation", gameObject);
    }
    public void UpdateDamage(float damage)
    {
        this.damage = damage;
    }

    public void UpdateFireRate(float fireRate)
    {
        this.fireRate = fireRate;
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
            //AudioManager.Instance.PlayAudioOnObject("Crossbow Fire", gameObject);
            AudioManager.Instance.Play("Flame Fire", gameObject);
            gameManager.EnqueueDamageData(new GameManager.EnemyDamageData(target, damage, target.damageResistance));

            delay = 1 / fireRate;
        }
    }
}