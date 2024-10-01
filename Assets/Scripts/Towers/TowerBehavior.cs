using System;
using UnityEngine;

public class TowerBehavior : MonoBehaviour
{
    [NonSerialized] public Enemy target;

    [SerializeField] Transform towerPivot;
    [SerializeField] LayerMask towerInfoMask;
    public LayerMask enemiesLayer;
    public float damage;
    public float fireRate;
    public float range;
    public int cost;

    private float delay;
    private TowerPlacement towerPlacement;
    Camera cam;
    private bool isSelected;

    private IDamageMethod currentDamageMethodClass;

    private void Start()
    {
        towerPlacement = TowerPlacement.Instance;
        cam = towerPlacement.cam;
        isSelected = true;

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
    }

    //Desyncs the towers from regular game loop to prevent errors
    public void Tick()
    {
        currentDamageMethodClass.damageTick(target);

        if (target != null)
        {
            towerPivot.transform.rotation = Quaternion.LookRotation(target.transform.position - transform.position);
        }

        if (Input.GetMouseButtonDown(0))
            isSelected = false;

        //Ray casts from screen to mouse
        Ray camRay = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        //Gets data from raycast
        if (Physics.Raycast(camRay, out hitInfo, 100f, towerInfoMask))
        {
            //If tower was clicked
            if (Input.GetMouseButtonDown(0))
                isSelected = true;
        }
            
        gameObject.transform.Find("Base").transform.Find("Range").gameObject.SetActive(isSelected);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(towerPivot.position, range);
        if (target != null)
            Gizmos.DrawWireCube(target.transform.position, new Vector3(.5f, .5f, .5f));
    }
}