using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Create EnemyData")]
public class EnemyData : ScriptableObject
{
    public GameObject enemyPrefab;
    public int enemyID;
}
