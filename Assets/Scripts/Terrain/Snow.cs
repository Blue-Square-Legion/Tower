using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snow : MonoBehaviour
{
    [NonSerialized] public float slowModifier;
    public float duration = 10;
    private List<Enemy> enemyList = new List<Enemy>();

    /// <summary>
    /// Set duration then call Init() to start countdown
    /// </summary>
    public void Init()
    {
        StartCoroutine(Duration());
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (!enemyList.Contains(enemy))
            {
                GameManager.EnemyBuff buff = new GameManager.EnemyBuff(GameManager.EnemyBuffNames.Slow, 0, 0, slowModifier, -123, true, null);
                GameManager.ApplyEnemyBuffData buffData = new GameManager.ApplyEnemyBuffData(buff, enemy);
                GameManager.Instance.EnqueueEnemyBuffToApply(buffData);

                enemyList.Add(enemy);
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();

            GameManager.EnemyBuff buff = new GameManager.EnemyBuff(GameManager.EnemyBuffNames.Slow, 0, 0, slowModifier, -123, true, null);
            GameManager.ApplyEnemyBuffData buffData = new GameManager.ApplyEnemyBuffData(buff, enemy);
            GameManager.Instance.EnqueueEnemyBuffToRemove(buffData);

            enemyList.Remove(other.GetComponent<Enemy>());
        }
    }

    IEnumerator Duration()
    {
        yield return new WaitForSeconds(duration);

        foreach (Enemy enemy in enemyList)
        {
            GameManager.EnemyBuff buff = new GameManager.EnemyBuff(GameManager.EnemyBuffNames.Slow, 0, 0, slowModifier, -123, true, null);
            GameManager.ApplyEnemyBuffData buffData = new GameManager.ApplyEnemyBuffData(buff, enemy);
            GameManager.Instance.EnqueueEnemyBuffToRemove(buffData);
        }

        Destroy(gameObject);
    }
}
