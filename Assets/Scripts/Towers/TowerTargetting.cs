using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class TowerTargetting
{
    public enum TargetType
    {
        First,
        Last,
        Close,
        Furthest
        //TODO - Add Strongest and Weakest
    }

    public static Enemy GetTarget(TowerBehavior currentTower, TargetType targetType)
    {
        GameManager gameManager = GameManager.Instance;
        EnemySpawner enemySpawner = EnemySpawner.Instance;

        Collider[] enemiesInRange = Physics.OverlapSphere(currentTower.transform.position, currentTower.range, currentTower.enemiesLayer);
        if (enemiesInRange.Length == 0)
        {
            return null;
        }

        NativeArray<EnemyDataValues> enemiesToCalculate = new(enemiesInRange.Length, Allocator.TempJob);
        NativeArray<Vector3> nodePositions = new(gameManager.nodePositions, Allocator.TempJob);
        NativeArray<float> nodeDistances = new(gameManager.nodeDistances, Allocator.TempJob);
        NativeArray<int> enemyToIndex = new(1, Allocator.TempJob);
        int enemyIndexToReturn = -1;

        int validEnemyCount = 0;  // Add this line to track valid enemies
        for (int i = 0; i < enemiesInRange.Length; i++)
        {
            Enemy currentEnemy = enemiesInRange[i].GetComponent<Enemy>();
            //if taunted and target in range, hit taunt target
            if (currentTower.isTaunted && currentEnemy == currentTower.tauntTarget)
                return currentTower.tauntTarget;

            // Check if the enemy is invisible
            if (currentEnemy.isInvisible)
                continue;

            int enemyIndexInList = enemySpawner.spawnedEnemies.FindIndex(x => x == currentEnemy);

            enemiesToCalculate[validEnemyCount] = new EnemyDataValues(
                currentEnemy.transform.position,
                currentEnemy.nodeIndex,
                currentEnemy.currentHealth,
                enemyIndexInList,
                currentEnemy.GetComponent<NavMeshMovement>().remainingDist,
                currentEnemy.isInvisible 
            );
            validEnemyCount++;  
        }

        if (validEnemyCount == 0)
        {
            enemiesToCalculate.Dispose();
            nodePositions.Dispose();
            nodeDistances.Dispose();
            enemyToIndex.Dispose();
            return null;
        }

        SearchForEnemy enemySearchJob = new SearchForEnemy
        {
            _enemiesToCalculate = enemiesToCalculate,
            _nodePositions = nodePositions,
            _nodeDistances = nodeDistances,
            _enemyToIndex = enemyToIndex,
            targetingType = targetType,
            _towerPositon = currentTower.transform.position
        };

        switch (targetType)
        {
            case TargetType.First:
                enemySearchJob.compareValue = Mathf.Infinity;
                break;
            case TargetType.Last:
                enemySearchJob.compareValue = Mathf.NegativeInfinity;
                break;
            case TargetType.Close:
                enemySearchJob.compareValue = Mathf.Infinity;
                break;
            case TargetType.Furthest:
                enemySearchJob.compareValue = Mathf.NegativeInfinity;
                break;
        }

        JobHandle dependency = new JobHandle();
        JobHandle searchJobHandle = enemySearchJob.Schedule(validEnemyCount, dependency);  
        searchJobHandle.Complete();

        enemyIndexToReturn = enemiesToCalculate[enemyToIndex[0]].enemyIndex;

        enemiesToCalculate.Dispose();
        nodePositions.Dispose();
        nodeDistances.Dispose();
        enemyToIndex.Dispose();

        if (enemyIndexToReturn == -1)
        {
            return null;
        }
        return enemySpawner.spawnedEnemies[enemyIndexToReturn];
    }

    struct EnemyDataValues
    {
        public EnemyDataValues(Vector3 enemyPosition, int nodeIndex, float health, int enemyIndex, float remainingDist, bool isInvisible)
        {
            this.enemyPosition = enemyPosition;
            this.nodeIndex = nodeIndex;
            this.health = health;
            this.enemyIndex = enemyIndex;
            this.remainingDist = remainingDist;
            this.isInvisible = isInvisible;
        }

        public Vector3 enemyPosition;
        public int nodeIndex;
        public float health;
        public int enemyIndex;
        public float remainingDist;
        public bool isInvisible; 
    }

    struct SearchForEnemy : IJobFor
    {
        [ReadOnly] public NativeArray<EnemyDataValues> _enemiesToCalculate;
        [ReadOnly] public NativeArray<Vector3> _nodePositions;
        [ReadOnly] public NativeArray<float> _nodeDistances;
        [NativeDisableParallelForRestriction] public NativeArray<int> _enemyToIndex;
        public Vector3 _towerPositon;

        public float compareValue;
        public TargetType targetingType;
        public void Execute(int index)
        {
            if (_enemiesToCalculate[index].isInvisible) { return; } 

            float distance;

            switch (targetingType)
            {
                case TargetType.First:
                    distance = _enemiesToCalculate[index].remainingDist;
                    if (distance < compareValue)
                    {
                        _enemyToIndex[0] = index;
                        compareValue = distance;
                    }
                    break;
                case TargetType.Last:
                    distance = _enemiesToCalculate[index].remainingDist;
                    if (distance > compareValue)
                    {
                        _enemyToIndex[0] = index;
                        compareValue = distance;
                    }
                    break;
                case TargetType.Close:
                    distance = Vector3.Distance(_towerPositon, _enemiesToCalculate[index].enemyPosition);
                    if (distance < compareValue)
                    {
                        _enemyToIndex[0] = index;
                        compareValue = distance;
                    }
                    break;
                case TargetType.Furthest:
                    distance = Vector3.Distance(_towerPositon, _enemiesToCalculate[index].enemyPosition);
                    if (distance > compareValue)
                    {
                        _enemyToIndex[0] = index;
                        compareValue = distance;
                    }
                    break;
            }
        }
    }
}
