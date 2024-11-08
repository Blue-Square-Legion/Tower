using UnityEngine;

public class Confusion : MonoBehaviour
{
    public float fallSpeed = 30f;  // Speed at which the meteor falls
    public float impactRadius = 3f; // Radius of impact for AoE damage
    public float fallHeight = 10f; // Height from which the meteor falls (above ground)

    public Vector3 targetPosition;
    private bool hasLanded = false;
    private Vector3 fallStartPosition;

    void Start()
    {
        // Set Fall position
        targetPosition = transform.position;
        fallStartPosition = new Vector3(targetPosition.x, targetPosition.y + fallHeight, targetPosition.z);

        transform.position = fallStartPosition;
    }

    void Update()
    {
        // If the meteor hasn't landed yet, fall towards the target position
        if (!hasLanded)
        {
            // Move the meteor towards the target position (falling effect)
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, fallSpeed * Time.deltaTime);

            // If the meteor reaches the ground, trigger the impact
            if (transform.position.y <= targetPosition.y + 0.1f)
            {
                Land();
            }
        }
    }

    void Land()
    {
        // Set the meteor as landed
        hasLanded = true;

        // Create an impact effect (optional: add a particle effect or sound)
        ImpactEffect();

        // Deal damage to any enemies within the impact radius
        DealDamage();

        // Destroy the meteor after impact
        Destroy(gameObject);
    }

    void DealDamage()
    {
        // Find all enemies in the area (assuming enemies have a tag "Enemy")
        Collider[] enemies = Physics.OverlapSphere(transform.position, impactRadius);

        foreach (Collider enemy in enemies)
        {
            if (enemy.CompareTag("Enemy")) // Ensure the object is an enemy
            {
                // Assuming enemies have a script with a method to take damage
                enemy.GetComponent<Enemy>().isConfused = true;
            }
        }
    }

    // Optional: Add a particle effect at impact
    void ImpactEffect()
    {
        // Implement a particle effect, sound, or any other impact visuals here
    }
}
