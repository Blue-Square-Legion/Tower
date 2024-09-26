using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileDamage : MonoBehaviour, IDamageMethod
{
    public LayerMask enemiesLayer;
    [SerializeField] private ParticleSystem missileSystem;
    [SerializeField] private Transform towerPivot;

    GameManager gameManager;
    [NonSerialized] public float damage;
    private float fireRate;
    private float delay;
    private ParticleSystem.MainModule missileSystemMain;
    public void Init(float damage, float fireRate)
    {
        gameManager = GameManager.Instance;
        missileSystemMain = missileSystem.main;
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

            missileSystemMain.startRotationX = towerPivot.forward.x;
            missileSystemMain.startRotationY = towerPivot.forward.y;
            missileSystemMain.startRotationZ = towerPivot.forward.z;

            missileSystem.Play();
            delay = 1 / fireRate;
        }
    }
}