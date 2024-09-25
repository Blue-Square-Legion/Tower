using UnityEngine;

public class TowerBehavior : MonoBehaviour
{
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] GameObject head;
    [SerializeField] float range;
    [SerializeField] float fireRate;
    [SerializeField] float projectileSpeed;
    [SerializeField] LayerMask enemyLayer;

    private float fireTimer = Mathf.Infinity;
    private bool enemyInRange = false;

    void Update()
    {
        fireTimer += Time.deltaTime;
        Vector3 target = CheckForEnemies();

        if (fireTimer > fireRate && enemyInRange)
        {
            GameObject projectile = Fire();
            Vector3 direction = new Vector3(target.x - transform.position.x, 0, target.z - transform.position.z);
            projectile.GetComponent<Rigidbody>().AddForce(direction * projectileSpeed);
        }
    }

    private GameObject Fire()
    {
        fireTimer = 0;
        return Instantiate(projectilePrefab,head.transform);
    }

    private Vector3 CheckForEnemies()
    {
        Vector3 lastEnemyPosition = new();
        float distanceToEnd = Mathf.Infinity;
        Collider[] EnemiesInRange = Physics.OverlapSphere(transform.position, range, enemyLayer);

        if (EnemiesInRange.Length == 0)
        {
            enemyInRange = false;
        }
            
        else
        {
            enemyInRange = true;
        }
            

        foreach (Collider enemy in EnemiesInRange)
        {
            float currentDistance = enemy.GetComponent<MoveAlongPath>().DistanceUntilEnd();
            if (currentDistance < distanceToEnd)
            {
                lastEnemyPosition = enemy.transform.position;
                distanceToEnd = currentDistance;
            }
        }
        return lastEnemyPosition;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, range);
    }
}