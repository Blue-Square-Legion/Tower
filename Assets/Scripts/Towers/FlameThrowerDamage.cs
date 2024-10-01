using System;
using UnityEngine;

public class FlameThrowerDamage : MonoBehaviour, IDamageMethod
{
    [SerializeField] private Collider fireTrigger;
    [SerializeField] private ParticleSystem fireEffect;
    GameManager gameManager;
    [NonSerialized] public float damage;
    [NonSerialized] public float fireRate;
    public void Init(float damage, float fireRate)
    {
        gameManager = GameManager.Instance;
        this.damage = damage;
        this.fireRate = fireRate;
    }

    public void damageTick(Enemy target)
    {
        fireTrigger.enabled = target != null;

        if(target)
        {
            if (!fireEffect.isPlaying) fireEffect.Play();
            return;
        }

        fireEffect.Stop();
    }
}