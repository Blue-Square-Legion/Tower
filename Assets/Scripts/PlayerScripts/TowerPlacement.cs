using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class TowerPlacement : MonoBehaviour
{
    #region Singleton

    private static TowerPlacement instance;
    public static TowerPlacement Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType(typeof(TowerPlacement)) as TowerPlacement;
            return instance;
        }
        set { instance = value; }
    }

    #endregion


    public Camera cam;
    public NavMeshSurface surface;
    public NavMeshSurface dummySurface;
    public NavMeshAgent agent;
    public Transform destination;
    [SerializeField] private LayerMask placementCheckMask;
    [SerializeField] private LayerMask placementColliderMask;
    private GameObject currentTowerBeingPlaced;
    private bool canPlace;
    private Player player;

    GameManager gameManager;
    void Start()
    {
        gameManager = GameManager.Instance;
        canPlace = true;
        player = GetComponent<Player>();
    }

    private void Update()
    {
        if (canPlace && currentTowerBeingPlaced != null)
        {
            //Ray casts from screen to mouse
            Ray camRay = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            //Gets data from raycast
            if (Physics.Raycast(camRay, out hitInfo, 100f, placementColliderMask))
            {
                currentTowerBeingPlaced.transform.position = hitInfo.point;
            }

            //Cancels placing tower
            if (Input.GetKeyDown(KeyCode.Q))
            {
                CancelPlacingTower();
                return;
            }

            //Checks if the tower can be placed

            //If left mouse button is down and mouse is pointing to a valid object
            if (Input.GetMouseButtonDown(0) && hitInfo.collider.gameObject != null)
            {
                //If the surface is buildable
                if (!hitInfo.collider.gameObject.CompareTag("NotBuildable"))
                {
                    BoxCollider towerCollider = currentTowerBeingPlaced.gameObject.GetComponent<BoxCollider>();
                    towerCollider.isTrigger = true;

                    Vector3 boxCenter = currentTowerBeingPlaced.gameObject.transform.position + towerCollider.center;
                    Vector3 halfExtends = towerCollider.size / 2;

                    //Checks if the tower is too close to a different tower or structure
                    if (!Physics.CheckBox(boxCenter, halfExtends, Quaternion.identity, placementCheckMask, QueryTriggerInteraction.Ignore))
                    {
                        ////Places tower
                        //surface.BuildNavMesh();
                        //gameManager.builtTowers.Add(currentTowerBeingPlaced.GetComponent<TowerBehavior>());
                        //player.RemoveMoney(currentTowerBeingPlaced.GetComponent<TowerBehavior>().cost);
                        //towerCollider.isTrigger = false;
                        //currentTowerBeingPlaced = null;

                        NavMeshPath path = new NavMeshPath();
                        
                        towerCollider.isTrigger = false;
                        dummySurface.BuildNavMesh();
                        //surface.BuildNavMesh();
                        agent.CalculatePath(destination.position, path);

                        // Build if path wont be blocked
                        if (path.status == NavMeshPathStatus.PathComplete)
                        {
                            surface.BuildNavMesh();
                            gameManager.builtTowers.Add(currentTowerBeingPlaced.GetComponent<TowerBehavior>());
                            player.RemoveMoney(currentTowerBeingPlaced.GetComponent<TowerBehavior>().cost);
                            towerCollider.isTrigger = false;
                            currentTowerBeingPlaced = null;
                        } else
                        {
                            towerCollider.isTrigger = true;
                            Destroy(currentTowerBeingPlaced);
                            //surface.BuildNavMesh();
                            print("Placing the tower here will block all enemy paths");
                        }
                    }
                }
            }
        }
    }

    public void SetTowerToPlace(GameObject tower)
    {
        //return if not enough money
        if (player.GetMoney() < tower.GetComponent<TowerBehavior>().cost) return;

        if (currentTowerBeingPlaced == null)
            currentTowerBeingPlaced = Instantiate(tower, Vector3.zero, Quaternion.identity);
    }

    public void CancelPlacingTower()
    {
        if (currentTowerBeingPlaced != null)
        {
            Destroy(currentTowerBeingPlaced);
            currentTowerBeingPlaced = null;
        }
    }

    public void OnMouseEnterAnyButton()
    {
        canPlace = false;
    }

    public void OnMouseExitAnyButton()
    {
        canPlace = true;
    }
}