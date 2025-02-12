using System.Collections.Generic;
using UnityEngine;

public class SpikeTrigger : MonoBehaviour
{
    [SerializeField] SpikeDamage parent;

    GameManager gameManager;
    private float damage;
    List<Enemy> enemiesInside;

    private void Start()
    {
        gameManager = GameManager.Instance;
        damage = parent.damage;
        enemiesInside = new();
    }

    public void UpdateDamage()
    {
        damage = parent.damage;
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.transform.CompareTag("Enemy"))
        {
            enemiesInside.Add(other.gameObject.GetComponent<Enemy>());
        }
    }

    private void OnTriggerExit(Collider other)
    {

        if (other.transform.CompareTag("Enemy"))
        {
            enemiesInside.Remove(other.gameObject.GetComponent<Enemy>());
        }
    }

    public void damageTick()
    {
        enemiesInside.RemoveAll(enemy => enemy == null || !enemy.gameObject.activeInHierarchy);
        int enemyCount = enemiesInside.Count;
        for (int i = 0; i < enemyCount; i++)
        {
            Debug.Log(enemiesInside[i]);
            enemiesInside[i].lastDamagingTower = transform.GetComponent<TowerBehavior>();
            gameManager.EnqueueDamageData(new GameManager.EnemyDamageData(enemiesInside[i], damage, 
                GameManager.DamageTypes.Sharp, transform.GetComponent<TowerBehavior>()));
        }
    }
}