using UnityEditor.PackageManager;
using UnityEngine;

public class RunePlacement : MonoBehaviour
{
    public GameObject skillPreviewPrefab;  // A prefab to represent the preview (e.g., a circle)
    public float castRange = 10f;          // Range where the skill can be cast
    private GameObject currentPreview;    // Instance of the skill preview
    private bool isCasting = false;        // Is the player currently casting the skill

    void Update()
    {
        // Handle input for starting skill casting
        if (Input.GetKeyDown(KeyCode.Q) && !isCasting)
        {
            StartSkillCasting();
        }

        // If we are casting, update the preview and listen for a click
        if (isCasting)
        {
            UpdateSkillPreview();

            // Check for a click to cast the skill
            if (Input.GetMouseButtonDown(0)) // Left mouse button
            {
                CastSkill();
            }
        }
    }

    // Starts the casting process and shows the preview
    void StartSkillCasting()
    {
        isCasting = true;

        // Instantiate the preview at the initial mouse position
        if (currentPreview != null)
        {
            Destroy(currentPreview);
        }

        currentPreview = Instantiate(skillPreviewPrefab);
        UpdateSkillPreview();
    }

    // Updates the preview's position to match the mouse location in the world
    void UpdateSkillPreview()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Raycast to find where the mouse is pointing in the world
        if (Physics.Raycast(ray, out hit, castRange))
        {
            currentPreview.transform.position = new Vector3(hit.point.x, 0.1f, hit.point.z);
        }
    }

    // Cast the skill at the preview location
    void CastSkill()
    {
        if (currentPreview != null)
        {
            // Implement your skill casting logic here
            // For example, spawn a projectile, apply effects, etc.

            // Destroy the preview after casting
            Destroy(currentPreview);

            // End the casting
            isCasting = false;
        }
    }
}
