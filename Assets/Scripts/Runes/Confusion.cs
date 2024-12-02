using UnityEngine;

public class Confusion : MonoBehaviour
{
    public float fallSpeed = 30f;
    public float impactRadius = 3f;
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

        DoEffect();

        Destroy(gameObject);
    }

    void DoEffect()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, impactRadius);

        foreach (Collider enemy in enemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                enemy.GetComponent<Enemy>().lastDamagingTower = null;
                enemy.GetComponent<Enemy>().isConfused = true;
            }
        }
    }

    void ImpactEffect()
    {
        Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
    }
}
