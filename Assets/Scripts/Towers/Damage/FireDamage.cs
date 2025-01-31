using System;
using UnityEngine;
using AudioSystem;

public class FireDamage : TowerDamage
{
    [SerializeField] public Collider fireTrigger;
    [SerializeField] private ParticleSystem fireEffect;
    
    AudioBuilder audioBuilder;
    AudioEmitter audioEmitter;

    public override void Init(float damage, float fireRate)
    {
        this.damage = damage;
        this.fireRate = fireRate;

        audioBuilder = AudioManager.Instance.CreateAudio().WithAudioData(audioData).WithPosition(transform.position);
        audioEmitter = audioBuilder.Play();
        audioEmitter.Stop();
    }

    public override void DamageTick(Enemy target)
    {

         fireTrigger.enabled = target != null;

         if(target)
         {
            if (!fireEffect.isPlaying)
            {
                fireEffect.Play();
                audioEmitter = audioBuilder.Play();
            }
             return;
         }
         if (audioEmitter != null && audioEmitter.IsPlaying())
            audioEmitter.Stop();
        fireEffect.Stop();
    }
}