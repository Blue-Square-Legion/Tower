using UnityEngine;

public class TowerBehavior : MonoBehaviour
{
    public LayerMask enemiesLayer;
    public Enemy target;
    public Transform towerPivot;

    public float damage;
    public float fireRate;
    public float range;
    public int cost;

    private float delay;

    private IDamageMethod currentDamageMethodClass;

    private void Start()
    {
        currentDamageMethodClass = GetComponent<IDamageMethod>();

        if (currentDamageMethodClass == null )
        {
            Debug.LogError("ERROR: FAILED TO FIND A DAMAGE CLASS ON CURRENT TOWER!");
        }
        else
        {
            currentDamageMethodClass.Init(damage, fireRate);
        }

        delay = 1 / fireRate;
        cost = 100;
    }

    //Desyncs the towers from regular game loop to prevent errors
    public void Tick()
    {

        currentDamageMethodClass.damageTick(target);

        if (target != null)
        {
            towerPivot.transform.rotation = Quaternion.LookRotation(target.transform.position - transform.position);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(towerPivot.position, range);
        if (target != null)
            Gizmos.DrawWireCube(target.transform.position, new Vector3(.5f, .5f, .5f));
    }
}