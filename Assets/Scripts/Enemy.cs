using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int maxHealth;
    public float currentHealth;
    public float speed;
    public float damageResistance;
    public int ID;
    public int nodeIndex;
    GameManager gameManager;
    public void Init()
    {
        gameManager = GameManager.Instance;
        currentHealth = maxHealth;
        transform.position = gameManager.nodePositions[0];
        damageResistance = 1;
        nodeIndex = 0;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0)
        {
            Destroy(gameObject);
        }
    }
}