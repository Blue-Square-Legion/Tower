using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snow : MonoBehaviour
{
    [NonSerialized] public float slowModifier;
    public float duration = 10;
    private List<Enemy> enemyList = new List<Enemy>();
    private float speedChange;

    /// <summary>
    /// Set duration then call Init() to start countdown
    /// </summary>
    public void Init()
    {
        StartCoroutine(Duration());
    }

    public void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (!enemyList.Contains(enemy))
            {
                speedChange = enemy.currentSpeed - (enemy.currentSpeed * slowModifier);
                enemy.SetSpeed(enemy.currentSpeed * slowModifier);
                enemyList.Add(enemy);
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            enemy.SetSpeed(enemy.currentSpeed / slowModifier);

            enemyList.Remove(other.GetComponent<Enemy>());
        }
    }

    IEnumerator Duration()
    {
        yield return new WaitForSeconds(duration);

        foreach (Enemy enemy in enemyList)
        {
            enemy.SetSpeed(enemy.currentSpeed / slowModifier);
        }

        Destroy(gameObject);
    }
}
