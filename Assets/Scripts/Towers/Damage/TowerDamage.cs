using UnityEngine;
using AudioSystem;

public abstract class TowerDamage : MonoBehaviour
{
    protected GameManager gameManager;
    [SerializeField] protected AudioData audioData;
    public float damage;
    public float fireRate;
    protected float delay;
    public virtual void Init(float damage, float fireRate)
    {
        gameManager = GameManager.Instance;
        this.damage = damage;
        this.fireRate = fireRate;
        delay = 1f / fireRate;
    }

    public void UpdateDamage(float damage)
    {
        this.damage = damage;
    }

    public void UpdateFireRate(float fireRate)
    {
        this.fireRate = fireRate;
    }

    public abstract void DamageTick(Enemy target);
}