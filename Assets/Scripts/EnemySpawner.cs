using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject EnemyPrefab;
    [SerializeField] Transform SpawnPoint;

    private bool waveActive = false;

    private void Update()
    {
        if (SpawnPoint.childCount > 0)
        {
            waveActive = true;
        } else
        {
            waveActive = false;
        }
            
    }

    IEnumerator SpawnEnemies()
    {
        for (int i = 0; i < 10; i++)
        {
            yield return new WaitForSeconds(2);
            Instantiate(EnemyPrefab, SpawnPoint);
        }
        
    }

    public void SpawnWave()
    {
        if (!waveActive)
            StartCoroutine(SpawnEnemies());
    }
}
