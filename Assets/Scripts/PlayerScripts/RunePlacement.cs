using TMPro;
using UnityEngine;

public class RunePlacement : MonoBehaviour
{
    public GameObject skillPreviewPrefab;   // A prefab to represent the preview (e.g., a circle)
    public GameObject meteorPrefab;         // Meteor prefab to instantiate for meteor skill
    public GameObject lightningPrefab;      // Lightning prefab for lightning skill
    public GameObject windCurrentPrefab;    // Wind current prefab for wind skill
    public float castRange = 10f;           // Range where the skill can be cast
    private GameObject currentPreview;     // Instance of the skill preview
    private bool isCasting = false;         // Is the player currently casting the skill
    private SkillType selectedSkill = SkillType.Meteor;  // Default skill is Meteor

    public enum SkillType
    {
        Meteor,
        Lightning,
        WindCurrent
    }

    void Update()
    {
        // Handle input for starting skill casting
        if (Input.GetKeyDown(KeyCode.Q) && !isCasting)
        {
            SelectSkill(SkillType.Meteor);
        }
        if (Input.GetKeyDown(KeyCode.E) && !isCasting)
        {
            SelectSkill(SkillType.Lightning);
        }
        if (Input.GetKeyDown(KeyCode.R) && !isCasting)
        {
            SelectSkill(SkillType.WindCurrent);
        }

        if (isCasting)
        {
            UpdateSkillPreview();

            if (Input.GetMouseButtonDown(0))
            {
                CastSkill();
            }

            // Cancel casting
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                isCasting = false;
                if (currentPreview != null)
                {
                    Destroy(currentPreview);
                }
            }
        }
    }

    void StartSkillCasting()
    {
        isCasting = true;

        if (currentPreview != null)
        {
            Destroy(currentPreview);
        }

        currentPreview = Instantiate(skillPreviewPrefab);
        UpdateSkillPreview();
    }

    void UpdateSkillPreview()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, castRange))
        {
            currentPreview.transform.position = new Vector3(hit.point.x, 0.1f, hit.point.z);
        }
    }

    void CastSkill()
    {
        if (currentPreview != null)
        {
            Vector3 castPosition = currentPreview.transform.position;

            switch (selectedSkill)
            {
                case SkillType.Meteor:
                    CastMeteor(castPosition);
                    break;
                case SkillType.Lightning:
                    CastLightning(castPosition);
                    break;
                case SkillType.WindCurrent:
                    CastWindCurrent(castPosition);
                    break;
            }

            // Destroy the preview after casting
            Destroy(currentPreview);

            // End the casting
            isCasting = false;
        }
    }

    void CastMeteor(Vector3 castPosition)
    {
        // Instantiate the meteor at the selected position
        GameObject meteor = Instantiate(meteorPrefab, currentPreview.transform.position, Quaternion.identity);

        // Get the Meteor script and set the target position for falling
        Meteor meteorScript = meteor.GetComponent<Meteor>();
        meteorScript.targetPosition = castPosition; // Set the target position (on the ground)
    }

    void CastLightning(Vector3 castPosition)
    {
        // Instantiate the meteor at the selected position
        GameObject lightning = Instantiate(lightningPrefab, currentPreview.transform.position, Quaternion.identity);

        // Get the Meteor script and set the target position for falling
        Lightning lightningScript = lightning.GetComponent<Lightning>();
        lightningScript.targetPosition = castPosition; // Set the target position (on the ground)
    }

    void CastWindCurrent(Vector3 position)
    {
        // Instantiate wind current prefab at the selected position
        Instantiate(windCurrentPrefab, position, Quaternion.identity);
        Debug.Log("Wind current created at " + position);
    }

    void SelectSkill(SkillType skillType)
    {
        selectedSkill = skillType;
        Debug.Log("Selected skill: " + selectedSkill);
        StartSkillCasting();
    }
}
