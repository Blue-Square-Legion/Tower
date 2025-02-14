using AudioSystem;
using UnityEngine;

public class StandardDamage : TowerDamage
{
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

            target.lastDamagingTower = transform.GetComponent<TowerBehavior>();
            gameManager.EnqueueDamageData(new GameManager.EnemyDamageData(target, damage, GameManager.DamageTypes.Sharp, transform.GetComponent<TowerBehavior>()));

            delay = 1 / fireRate;
        }
    }
}