using UnityEngine;

public class TowerPlacement : MonoBehaviour
{
    private GameObject currentTowerBeingPlaced;
    void Start()
    {
        
    }

    private void Update()
    {
        
    }

    public void SetTowerToPlace()
    {
        currentTowerBeingPlaced = Instantiate(currentTowerBeingPlaced, Vector3.zero, Quaternion.identity);
    }
}