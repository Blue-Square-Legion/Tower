using UnityEngine;

public class Lightning : MonoBehaviour
{
    public float fallSpeed = 30f;  
    public float impactRadius = 3f;
    public int damage = 5; 
    public float fallHeight = 10f;

    public Vector3 targetPosition;
    private bool hasLanded = false;
    private Vector3 fallStartPosition;
    public GameObject impactEffectPrefab;

    void Start()
    {
        targetPosition = transform.position;
        fallStartPosition = new Vector3(targetPosition.x, targetPosition.y + fallHeight, targetPosition.z);

        transform.position = fallStartPosition;
    }

    void Update()
    {
        if (!hasLanded)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, fallSpeed * Time.deltaTime);

            if (transform.position.y <= targetPosition.y + 0.1f)
            {
                Land();
            }
        }
    }

    void Land()
    {
        hasLanded = true;

        ImpactEffect();

        DealDamage();

        Destroy(gameObject);
    }

    void DealDamage()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, impactRadius);

        foreach (Collider enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                enemy.GetComponent<Enemy>().lastDamagingTower = null;
                enemy.GetComponent<Enemy>().TakeDamage(damage);

                //Stun debuff
                GameManager.EnemyBuff buff = new GameManager.EnemyBuff(GameManager.EnemyBuffNames.Slow, 0, 0, 0.0001f, 2, true, null);
                GameManager.ApplyEnemyBuffData buffData = new GameManager.ApplyEnemyBuffData(buff, enemy.GetComponent<Enemy>());
                GameManager.Instance.EnqueueEnemyBuffToApply(buffData);
            }
        }
    }

    void ImpactEffect()
    {
        Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
    }
}
