using System;
using UnityEngine;
using AudioSystem;

public class FireDamage : MonoBehaviour, IDamageMethod
{
    [SerializeField] public Collider fireTrigger;
    [SerializeField] private ParticleSystem fireEffect;
    [SerializeField] AudioData audioData;
    GameManager gameManager;
    [NonSerialized] public float damage;
    [NonSerialized] public float fireRate;

    AudioEmitter audioEmitter;
    public void Init(float damage, float fireRate)
    {
        gameManager = GameManager.Instance;
        this.damage = damage;
        this.fireRate = fireRate;

        AudioManager.Instance.CreateAudio().WithAudioData(audioData).WithPosition(transform.position);
        audioEmitter = AudioManager.Instance.Get();
        audioEmitter.Init(audioData);
    }

    public void UpdateDamage(float damage)
    {
        this.damage = damage;
    }

    public void damageTick(Enemy target)
    {

         fireTrigger.enabled = target != null;

         if(target)
         {
            if (!fireEffect.isPlaying)
            {
                fireEffect.Play();
                if (audioData.frequentSound)
                    AudioManager.Instance.frequentAudioEmitters.Enqueue(audioEmitter);
                audioEmitter.Play();
            }
             return;
         }
         if (audioEmitter.IsPlaying())
            audioEmitter.Stop();
        fireEffect.Stop();
        

    }
}