using UnityEngine;

public class Meteor : MonoBehaviour
{
    public float fallSpeed = 10f;  // Speed at which the meteor falls
    public float impactRadius = 5f; // Radius of impact for AoE damage
    public int damage = 100;    // Damage dealt by the meteor
    public float fallHeight = 10f; // Height from which the meteor falls (above ground)

    public Vector3 targetPosition;
    private bool hasLanded = false;
    private Vector3 fallStartPosition;

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
                enemy.GetComponent<Enemy>().TakeDamage(damage);
            }
        }
    }

    void ImpactEffect()
    {
        // Implement a particle effect
    }
}
