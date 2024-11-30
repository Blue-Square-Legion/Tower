using System;
using UnityEngine;
using AudioSystem;

public class FireDamage : MonoBehaviour, IDamageMethod
{
    [SerializeField] public Collider fireTrigger;
    [SerializeField] private ParticleSystem fireEffect;
    [SerializeField] AudioData audioData;
    [NonSerialized] public float damage;
    [NonSerialized] public float fireRate;
    

    AudioBuilder audioBuilder;
    AudioEmitter audioEmitter;
    public void Init(float damage, float fireRate)
    {
        this.damage = damage;
        this.fireRate = fireRate;

        audioBuilder = AudioManager.Instance.CreateAudio().WithAudioData(audioData).WithPosition(transform.position);
        audioEmitter = audioBuilder.Play();
        audioEmitter.Stop();
        
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