using System.Collections.Generic;
using UnityEngine;

public class EnemyBuffer : MonoBehaviour
{
    [SerializeField] float radius;
    [SerializeField] float resistanceModifier;
    [SerializeField] float speedModifier;
    [SerializeField] LayerMask enemiesLayer;

    GameManager gameManager;
    private float countDown;
    private List<Enemy> enemiesInRange;
    Enemy enemy;

    private float averageSpeed;
    void Start()
    {
        gameManager = GameManager.Instance;
        enemy = GetComponent<Enemy>();
        enemiesInRange = new();
        countDown = 0.2f;
        averageSpeed = 0;
    }

    private void Update()
    {
        countDown -= Time.deltaTime;

        if (countDown < 0)
        {
            averageSpeed = 0;
            //Find all tower objects within range
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, radius, Vector3.forward, 0, enemiesLayer);

            //Clears list
            enemiesInRange.Clear();
            int count = hits.Length;
            for (int i = 0; i < count; i++)
            {
                //Filters out everything that is not an Enemy and filters out itself
               // print(hits[i].collider.gameObject.name);
                if (hits[i].collider.gameObject.TryGetComponent(out Enemy enemy) && hits[i].collider != gameObject.GetComponent<Collider>())
                    enemiesInRange.Add(enemy); //Adds object to list
            }

            int enemiesInRangeCount = enemiesInRange.Count;
            for (int i = 0; i < enemiesInRangeCount; i++)
            {
                GameManager.EnemyBuff buff = new GameManager.EnemyBuff(GameManager.EnemyBuffNames.SpeedEnemyBuffer, 0, 0, speedModifier, 1, false, null);
                GameManager.ApplyEnemyBuffData buffData = new GameManager.ApplyEnemyBuffData(buff, enemiesInRange[i]);
                gameManager.EnqueueEnemyBuffToApply(buffData);

                buff = new GameManager.EnemyBuff(GameManager.EnemyBuffNames.ResistanceEnemyBuffer, 0, 0, resistanceModifier, 1, false, null);
                buffData = new GameManager.ApplyEnemyBuffData(buff, enemiesInRange[i]);
                gameManager.EnqueueEnemyBuffToApply(buffData);

                averageSpeed += enemiesInRange[i].currentSpeed;
            }

            //Sets speed to the average of all enemies within its buff range (to stay near them)
            if (enemiesInRangeCount > 0)
            {
                if(!enemy.isStunned)
                {
                    averageSpeed /= enemiesInRangeCount;
                    enemy.SetSpeed(averageSpeed);
                }
            }
            else if (!enemy.isStunned) //If no enemies are nearby, return to default speed
            {
                //Does not return to normal speed if the Buffer is slowed.
                //Returns to normal speed if its current speed is greater than its normal speed
                if (!enemy.isSlowed || enemy.currentSpeed > enemy.speed)
                    enemy.SetSpeed(enemy.speed);
            }

            countDown = 0.2f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}