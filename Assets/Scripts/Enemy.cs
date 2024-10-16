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
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0)
        {
            Destroy(gameObject);
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
    }

    public void ReachedEnd()
    {

    }
}