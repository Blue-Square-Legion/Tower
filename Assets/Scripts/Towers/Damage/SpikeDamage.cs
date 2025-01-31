using System;
using UnityEngine;

public class SpikeDamage : TowerDamage
{
    [SerializeField] private SpikeTrigger child;

    public override void DamageTick(Enemy target)
    {
        if (delay > 0)
        {
            delay -= Time.deltaTime;
            return;
        }

        child.damageTick();

        delay = 1 / fireRate;
    }
}