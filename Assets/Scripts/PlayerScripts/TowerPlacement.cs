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
    public NavMeshAgent[] agents;
    [SerializeField] private Transform towersFolder;
    public Transform destination;
    [SerializeField] private LayerMask placementCheckMask;
    [SerializeField] private LayerMask placementColliderMask;
    private GameObject currentTowerBeingPlaced;
    private bool canPlace;
    private Player player;
    private bool pathBlocked;
    Color redColor = Color.red;
    Color blueColor = Color.blue;
    GameManager gameManager;
    void Start()
    {
        gameManager = GameManager.Instance;
        canPlace = true;
        player = GetComponent<Player>();
    }

    private void Update()
    {
        if (currentTowerBeingPlaced != null)
        {
            UIManager.Instance.ToggleDeselect(true);
            //Ray casts from screen to mouse
            Ray camRay = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            UIManager.Instance.ToggleDeselect(true);
            //Gets data from raycast
            if (Physics.Raycast(camRay, out hitInfo, 100f, placementColliderMask))
            {
                currentTowerBeingPlaced.transform.position = new Vector3(hitInfo.point.x, 0.15f, hitInfo.point.z);
            }

            //Cancels placing tower
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UIManager.Instance.ToggleDeselect(false);
                CancelPlacingTower();
                return;
            }
            Transform rangeObject = currentTowerBeingPlaced.transform.Find("Range");
            Renderer renderer = rangeObject.GetComponent<Renderer>();
            if (hitInfo.collider.gameObject != null)
            {
                if (!hitInfo.collider.gameObject.CompareTag("NotBuildable"))
                {

                    BoxCollider towerCollider = currentTowerBeingPlaced.gameObject.GetComponent<BoxCollider>();
                    towerCollider.isTrigger = true;

                    Vector3 boxCenter = currentTowerBeingPlaced.gameObject.transform.position + towerCollider.center;
                    Vector3 halfExtends = towerCollider.size / 2;

                    //Checks if the tower is too close to a different tower or structure
                    if (!Physics.CheckBox(boxCenter, halfExtends, Quaternion.identity, placementCheckMask, QueryTriggerInteraction.Ignore))
                    {
                        renderer.material.SetColor("_BaseColor", blueColor);
                        if (Input.GetMouseButtonDown(0) && hitInfo.collider.gameObject != null)
                        {
                            dummySurface.BuildNavMesh();
                            CheckPath();
                            // Build if path wont be blocked
                            if (!pathBlocked)
                            {
                                if (canPlace)
                                {
                                    surface.BuildNavMesh();
                                    gameManager.builtTowers.Add(currentTowerBeingPlaced.GetComponent<TowerBehavior>());
                                    player.RemoveMoney(currentTowerBeingPlaced.GetComponent<TowerBehavior>().cost);
                                    towerCollider.isTrigger = false;
                                    towerCollider.providesContacts = true;

                                    if (currentTowerBeingPlaced.TryGetComponent<SupportBehavior>(out SupportBehavior supportBehavior))
                                        supportBehavior.Built();

                                    currentTowerBeingPlaced = null;
                                    UIManager.Instance.ToggleDeselect(false);
                                }
                                else
                                {
                                    Destroy(currentTowerBeingPlaced);
                                    UIManager.Instance.SendPopUp("Tower cannot be placed here");
                                    UIManager.Instance.ToggleDeselect(false);
                                }
                            }
                            else
                            {
                                towerCollider.isTrigger = true;
                                Destroy(currentTowerBeingPlaced);
                                UIManager.Instance.SendPopUp("Placing the tower here will block all enemy paths");
                                UIManager.Instance.ToggleDeselect(false);
                            }

                        }
                    }
                    else
                    {
                        renderer.material.SetColor("_BaseColor", redColor);
                    }
                }
                else
                {
                    renderer.material.SetColor("_BaseColor", redColor);
                }
            }
        }
    }

    public void SetTowerToPlace(GameObject tower)
    {
        //return if not enough money
        if (player.GetMoney() < tower.GetComponent<TowerBehavior>().cost) return;

        if (currentTowerBeingPlaced == null)
        {
            currentTowerBeingPlaced = Instantiate(tower, Vector3.zero, Quaternion.identity);
            currentTowerBeingPlaced.transform.parent = towersFolder;
            UIManager.Instance.ToggleRuneSelection(false);
        }
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

    public void SellTower(GameObject tower)
    {
        UpgradePanel.Instance.SetUpgradePanel(false);
        player.GiveMoney(tower.GetComponent<TowerBehavior>().sellCost);
        gameManager.EnqueueTowerToRemove(tower.GetComponent<TowerBehavior>());
        GameManager.Instance.SelectedTower = null;
        dummySurface.BuildNavMesh();
        surface.BuildNavMesh();
    }

    private void CheckPath()
    {
        NavMeshPath path = new NavMeshPath();
        dummySurface.BuildNavMesh();
        foreach (NavMeshAgent agent in agents)
        {
            agent.CalculatePath(destination.position, path);
            if (path.status != NavMeshPathStatus.PathComplete)
            {
                pathBlocked = true;
                return;
            }

        }
        pathBlocked = false; ;
    }
}