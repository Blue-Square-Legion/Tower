using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] int maxHealth;
    [SerializeField] int value;
    [SerializeField] PlayerHealth player;
    private int currentHealth;

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<PlayerHealth>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0)
        {
            player.GiveMoney(value);
            Destroy(gameObject);
        }
    }
}
