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

        for (int i = 0; i < enemiesInRange.Length; i++)
        {
            Enemy currentEnemy = enemiesInRange[i].GetComponent<Enemy>();
            int enemyIndexInList = enemySpawner.spawnedEnemies.FindIndex(x => x == currentEnemy);

            enemiesToCalculate[i] = new EnemyDataValues(currentEnemy.transform.position, currentEnemy.nodeIndex, currentEnemy.currentHealth, enemyIndexInList);
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
                enemySearchJob.compareValue = Mathf.NegativeInfinity;
                break;
            case TargetType.Last:
                enemySearchJob.compareValue = Mathf.Infinity;
                break;
            case TargetType.Close:
                enemySearchJob.compareValue = Mathf.Infinity;
                break;
            case TargetType.Furthest:
                enemySearchJob.compareValue = Mathf.NegativeInfinity;
                break;
            /**
            case 0: //First
            case 2: //Close
                enemySearchJob.compareValue = Mathf.Infinity;
                break;
            case 1: //Last
            case 3: //Furthest
                enemySearchJob.compareValue = Mathf.NegativeInfinity;
                break;
            default:
                Debug.Log("ERROR: FAILED TO RECOGNIZE TARGETTING TYPE");
                break;
            */
        }

        JobHandle dependency = new JobHandle();
        JobHandle searchJobHandle = enemySearchJob.Schedule(enemiesToCalculate.Length, dependency);
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
        public EnemyDataValues(Vector3 enemyPosition, int nodeIndex, float health, int enemyIndex)
        {
            this.enemyPosition = enemyPosition;
            this.nodeIndex = nodeIndex;
            this.health = health;
            this.enemyIndex = enemyIndex;
        }

        public Vector3 enemyPosition;
        public int nodeIndex;
        public float health;
        public int enemyIndex;
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
            float distance;
            switch (targetingType)
            {
                case TargetType.First:
                    distance = GetDistanceToEnd(_enemiesToCalculate[index]);
                    if (distance > compareValue)
                    {
                        _enemyToIndex[0] = index;
                        compareValue = distance;
                    }
                    break;
                case TargetType.Last:
                    distance = GetDistanceToEnd(_enemiesToCalculate[index]);
                    if (distance < compareValue)
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

        private float GetDistanceToEnd(EnemyDataValues enemyToEvaluate)
        {
            float finalDistance = Vector3.Distance(enemyToEvaluate.enemyPosition, _nodePositions[enemyToEvaluate.nodeIndex]);

            for (int i = enemyToEvaluate.nodeIndex; i < _nodeDistances.Length; i++)
            {
                finalDistance += _nodeDistances[i];
            }
            return finalDistance;
        }
    }
}
