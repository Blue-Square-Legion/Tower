using System;
using UnityEngine;

public class MissileDamage : MonoBehaviour, IDamageMethod
{
    public Animator cannonAnimator;
    public LayerMask enemiesLayer;
    [SerializeField] private Transform towerPivot;

    GameManager gameManager;
    [SerializeField] private GameObject missile;
    [NonSerialized] public float damage;
    private float fireRate;
    private float delay;
    public void Init(float damage, float fireRate)
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

    public void damageTick(Enemy target)
    {
        if (target)
        {
            if (delay > 0)
            {
                delay -= Time.deltaTime;
                return;
            }

            cannonAnimator.SetTrigger("CannonFire");
            GameObject tempMissile = Instantiate(missile);
            tempMissile.transform.position = transform.position + new Vector3(0, 0.5f, 0);
            tempMissile.transform.rotation = transform.Find("Head").transform.rotation;
            tempMissile.GetComponent<Missile>().parent = gameObject;
            tempMissile.GetComponent<Missile>().Init();
            delay = 1 / fireRate;
        }
    }
}