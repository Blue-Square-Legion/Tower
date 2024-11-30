using System.Collections.Generic;
using UnityEngine;
using AudioSystem;
using System;

public class Enemy : MonoBehaviour
{
    public EnemyType type;
    public int maxHealth;
    public int moneyToPlayer;
    public float currentHealth;
    public float speed;
    public float currentSpeed {  get; private set; }
    public float damageResistance;
    [NonSerialized] public int ID;
    [NonSerialized] public int nodeIndex;
    [NonSerialized] public List<GameManager.Effect> activeEffects;
    GameManager gameManager;
    public NavMeshMovement navMeshMovement;
    [NonSerialized] public bool isStunned;
    private float stunTimer;
    [NonSerialized] public bool isConfused;
    [SerializeField] AudioData audioMovement;
    [SerializeField] AudioData audioDead;
    private float confusedTimer;
    [NonSerialized] public TowerBehavior lastDamagingTower;

    private AudioEmitter audioEmitterMove;
    public void Init()
    {
        gameManager = GameManager.Instance;
        currentHealth = maxHealth;
        currentSpeed = speed;
        activeEffects = new();
        damageResistance = 1;
        nodeIndex = 0;
        moneyToPlayer = 10;
        navMeshMovement = GetComponent<NavMeshMovement>();
        navMeshMovement.SetSpeed(currentSpeed);
        isStunned = false;
        stunTimer = 2;
        isConfused = false;
        confusedTimer = 2;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        gameObject.GetComponentInChildren<HealthBar>().UpdateHealth((int) currentHealth);
        if (currentHealth <= 0)
        {
            AudioManager.Instance.CreateAudio()
                .WithAudioData(audioDead)
                .WithPosition(gameObject.transform.position)
                .Play();
            GameManager.Instance.EnqueueEnemyToRemove(this);
            Player.Instance.GiveMoney(moneyToPlayer);
        }  
    }

    public void Tick()
    {
        int activeEffectsCount = activeEffects.Count;
        for (int i = 0; i < activeEffectsCount; i++)
        {
            if (activeEffects[i].duration > 0f)
            {
                if (activeEffects[i].damageDelay > 0f)
                {
                    activeEffects[i].damageDelay -= Time.deltaTime;
                }
                else
                {
                    lastDamagingTower = activeEffects[i].source;
                    gameManager.EnqueueDamageData(new GameManager.EnemyDamageData(this, activeEffects[i].damage, 1f, activeEffects[i].source));
                    activeEffects[i].damageDelay = 1f / activeEffects[i].damageRate;
                }
                activeEffects[i].duration -= Time.deltaTime;
            }
        }
        activeEffects.RemoveAll(x => x.duration <= 0);

        if (isConfused)
        {
            navMeshMovement.FlipDirection(0);
            confusedTimer -= Time.deltaTime;
        }

        if (confusedTimer < 0f)
        {
            confusedTimer = 2f;
            navMeshMovement.FlipDirection(1);
            isConfused = false;
        }

        if (isStunned)
        {
            navMeshMovement.SetSpeed(0);
            stunTimer -= Time.deltaTime;
        }

        if (stunTimer < 0f)
        {
            stunTimer = 2f;
            isStunned = false;
            navMeshMovement.SetSpeed(speed);
        }

        //Plays audio clip again once the clip ends
        if (audioEmitterMove == null || !audioEmitterMove.IsPlaying())
        {
            audioEmitterMove = AudioManager.Instance.CreateAudio().WithAudioData(audioMovement).WithPosition(gameObject.transform.position).Play();
        }
    }

    public void SetSpeed(float speed)
    {
        currentSpeed = speed;
        navMeshMovement.SetSpeed(currentSpeed);
    }

    public void ReachedEnd()
    {

    }

    public enum EnemyType
    {
        Basic,
        Fast,
        Slow,
        Ghost,
        Boss1,
        Boss2
    }
}