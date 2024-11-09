using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth;
    public int moneyToPlayer;
    public float currentHealth;
    public float speed;
    public float damageResistance;
    public int ID;
    public int nodeIndex;
    public List<GameManager.Effect> activeEffects;
    GameManager gameManager;
    public NavMeshMovement navMeshMovement;
    public bool isStunned;
    private float stunTimer;
    public bool isConfused;
    private float confusedTimer;

    private float normalSpeed;
    public void Init()
    {
        gameManager = GameManager.Instance;
        currentHealth = maxHealth;
        normalSpeed = speed;
        activeEffects = new();
        transform.position = gameManager.SpawnPoint.position;
        damageResistance = 1;
        nodeIndex = 0;
        moneyToPlayer = 10;
        navMeshMovement = GetComponent<NavMeshMovement>();
        navMeshMovement.SetSpeed(speed);
        isStunned = false;
        stunTimer = 2;
        isConfused = false;
        confusedTimer = 2;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        gameObject.GetComponentInChildren<HealthBar>().UpdateHealth((int) currentHealth);
        if (currentHealth <= 0)
        {
            GameManager.Instance.EnqueEnemyToRemove(this);
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
                    gameManager.EnqueueDamageData(new GameManager.EnemyDamageData(this, activeEffects[i].damage, 1f));
                    activeEffects[i].damageDelay = 1f / activeEffects[i].damageRate;
                }
                activeEffects[i].duration -= Time.deltaTime;
            }
            if (activeEffects[i].effectName == GameManager.EffectNames.Fire)
            {
                if (activeEffects[i].duration > 0)
                    navMeshMovement.SetSpeed(speed * activeEffects[i].modifier);
                else
                    navMeshMovement.SetSpeed(normalSpeed);

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
            navMeshMovement.SetSpeed(normalSpeed);
        }


    }

    public void ReachedEnd()
    {

    }
}