using System.Collections.Generic;
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
    private bool resetPath;
    void Start()
    {
        gameManager = GameManager.Instance;
        canPlace = true;
        player = GetComponent<Player>();
        resetPath = false;
    }

    private void Update()
    {
        if (resetPath)
        {
            ResetPath();
        }

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

            if (hitInfo.collider != null &&
                hitInfo.collider.gameObject != null)
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
                            CheckPath(towerCollider);
                            // Build if path wont be blocked
                            if (!pathBlocked)
                            {

                                if (canPlace)
                                {
                                    surface.BuildNavMesh();
                                    gameManager.builtTowers.Add(currentTowerBeingPlaced.GetComponent<TowerBehavior>());
                                    player.RemoveMoney(currentTowerBeingPlaced.GetComponent<TowerBehavior>().cost);
                                    

                                    if (currentTowerBeingPlaced.TryGetComponent(out SupportBehavior supportBehavior)) //When placement is locked in, do method
                                        supportBehavior.Built();
                                    UpdateBuffTowers();

                                    currentTowerBeingPlaced = null;
                                    UIManager.Instance.ToggleDeselect(false);
                                }
                                else
                                {
                                    Destroy(currentTowerBeingPlaced);
                                    UIManager.Instance.SendPopUp("Cannot Place Here!");
                                    UIManager.Instance.ToggleDeselect(false);
                                    resetPath = true;
                                }
                            }
                            else
                            {
                                towerCollider.isTrigger = true;
                                Destroy(currentTowerBeingPlaced);
                                UIManager.Instance.SendPopUp("Cannot Place Here! All paths blocked");
                                UIManager.Instance.ToggleDeselect(false);
                                resetPath = true;
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
    
    /// <summary>
    /// When a tower is placed, update all buffing towers to check if the placed tower was placed in range of a tower giving a buff
    /// If it was, it gives that tower the buff
    /// </summary>
    private void UpdateBuffTowers()
    {
        SupportBehavior[] supportTowers = FindObjectsOfType<SupportBehavior>(); //Finds all Support towers
        EconomyBehavior[] economyTowers = FindObjectsOfType<EconomyBehavior>(); //Finds all Economy Towers

        int supportTowerCount = supportTowers.Length;
        int economyTowerCount = economyTowers.Length;

        //Updates all towers within a support tower's range
        for (int i = 0; i < supportTowerCount; i++)
        {
            supportTowers[i].UpdateTowersInRange();
        }

        //Updates all towers within a economy tower's range
        for (int i = 0; i < economyTowerCount; i++)
        {
            economyTowers[i].UpdateTowersInRange();
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

    private void CheckPath(BoxCollider collider)
    {
        collider.isTrigger = false;
        collider.providesContacts = true;
        dummySurface.BuildNavMesh();

        NavMeshPath path = new NavMeshPath();
        foreach (NavMeshAgent agent in agents)
        {
            agent.CalculatePath(destination.position, path);
            if (path.status != NavMeshPathStatus.PathComplete)
            {
                pathBlocked = true;
                collider.isTrigger = true;
                collider.providesContacts = false;
                return;
            }

        }
        
        pathBlocked = false; ;
    }

    private void ResetPath()
    {
        NavMeshPath path = new NavMeshPath();
        dummySurface.BuildNavMesh();
        foreach (NavMeshAgent agent in agents)
        {
            agent.GetComponent<NavMeshMovement>().ResetDestination();
        }
        resetPath = false;
    }
}