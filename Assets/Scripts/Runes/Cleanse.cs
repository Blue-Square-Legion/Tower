using UnityEngine;

public class Cleanse : MonoBehaviour
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

        DoCleanse();

        Destroy(gameObject);
    }

    void DoCleanse()
    {
        Collider[] towers = Physics.OverlapSphere(transform.position, impactRadius);

        foreach (Collider tower in towers)
        {
            if (tower.gameObject.layer == LayerMask.NameToLayer("Towers"))
            {
                print("cleansed");
                tower.GetComponent<TowerBehavior>().CleanseDebuffs();
            }
        }
    }

    void ImpactEffect()
    {
        Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
    }
}
