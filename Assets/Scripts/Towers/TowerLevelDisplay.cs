using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TowerLevelDisplay : MonoBehaviour
{
    public Transform target; 
    public Canvas canvas; 
    public Vector3 offset = new Vector3(0, 2, 0); 
    private Text textComponent;
    private Camera cam;

    void Start()
    {
       /* textComponent = GetComponent<Text>();
        cam = Camera.main;

        if (textComponent == null)
        {
            Debug.LogError("Text component not found on the FloatingTextOverlay GameObject");
        }
        if (target == null)
        {
            Debug.LogError("Target is not assigned to the FloatingTextOverlay component");
        }
        if (canvas == null)
        {
            Debug.LogError("Canvas is not assigned to the FloatingTextOverlay component");
        }*/
    }

   /* void Update()
    {
        if (target != null && cam != null)
        {
            // Convert the world position of the target to screen space
            Vector3 screenPosition = cam.WorldToScreenPoint(target.position + offset);
            textComponent.transform.position = screenPosition;

            // Update the text to show the tower's level
            TowerBehavior towerBehavior = target.GetComponent<TowerBehavior>();
            if (towerBehavior != null)
            {
                textComponent.text = "Level: " + towerBehavior.upgradeLevel;
            }
        }
   
    }*/
}
