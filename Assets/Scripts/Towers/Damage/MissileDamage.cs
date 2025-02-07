using System;
using UnityEngine;
using AudioSystem;

public class MissileDamage : TowerDamage
{
    public Animator cannonAnimator;
    public LayerMask enemiesLayer;
    [SerializeField] private Transform towerPivot;

    [SerializeField] private GameObject missile;
    public float explosionRadius;

    public override void DamageTick(Enemy target)
    {
        if (target)
        {
            if (delay > 0)
            {
                delay -= Time.deltaTime;
                return;
            }
            AudioManager.Instance.CreateAudio()
                .WithAudioData(audioData)
                .WithPosition(gameObject.transform.position)
                .Play();

            cannonAnimator.SetTrigger("CannonFire");
            GameObject tempMissile = Instantiate(missile);
            tempMissile.transform.position = transform.position + new Vector3(0, 0.5f, 0);
            tempMissile.transform.rotation = towerPivot.transform.rotation;
            tempMissile.transform.Rotate(0, 180, 0, Space.Self);

            Missile missileObject = tempMissile.GetComponent<Missile>();
            missileObject.explosionRadius = explosionRadius;
            missileObject.parent = gameObject;
            missileObject.target = target;

            missileObject.Init();
            delay = 1 / fireRate;
        }
    }
}