using TMPro;
using UnityEngine;

public class RunePlacement : MonoBehaviour
{
    public GameObject meteorPreviewPrefab;
    public GameObject lightingPreviewPrefab;
    public GameObject confusionPreviewPrefab;

    public GameObject meteorPrefab;
    public GameObject lightningPrefab;
    public GameObject confusionPrefab;
    public float castRange = 10f; 
    private GameObject currentPreview;
    private bool isCasting = false;
    private SkillType selectedSkill = SkillType.Meteor;

    public float meteorCost = 100;
    public float lightningCost = 50;
    public float confusionCost = 50;

    private float meteorCooldown = 5f;
    private float lightningCooldown = 7f;
    private float confusionCooldown = 6f;

    private float meteorCooldownTimer = 0f;
    private float lightningCooldownTimer = 0f;
    private float confusionCooldownTimer = 0f;

    // UI Text to display the cooldown in seconds
    public TextMeshProUGUI meteorCooldownText;
    public TextMeshProUGUI lightningCooldownText;
    public TextMeshProUGUI confusionCooldownText;

    public enum SkillType
    {
        Meteor,
        Lightning,
        Confusion
    }

    void Update()
    {

        //disable casting and cooldown regen if no wave is active
        if (!GameManager.Instance.waveActive)
        {
            isCasting = false;
            if (currentPreview != null)
            {
                Destroy(currentPreview);
            }
            UIManager.Instance.ToggleDeselect(false);

            return;
        }

        // Handle input for starting skill casting
        if (UIManager.Instance.runeSelection.activeInHierarchy)
        {
            if (Input.GetKeyDown(KeyCode.Q) && !isCasting && meteorCooldownTimer <= 0f)
            {
                SelectSkill(SkillType.Meteor);
            }
            if (Input.GetKeyDown(KeyCode.E) && !isCasting && lightningCooldownTimer <= 0f)
            {
                SelectSkill(SkillType.Lightning);
            }
            if (Input.GetKeyDown(KeyCode.R) && !isCasting && confusionCooldownTimer <= 0f)
            {
                SelectSkill(SkillType.Confusion);
            }
        }


        // Handle skill cooldowns
        if (meteorCooldownTimer > 0f)
        {
            meteorCooldownTimer -= Time.deltaTime;
            //UpdateCooldownText(meteorCooldownText, meteorCooldownTimer);
        }
        else
        {
            //meteorCooldownText.text = "Ready";
        }
        if (lightningCooldownTimer > 0f)
        {
            lightningCooldownTimer -= Time.deltaTime;
            //UpdateCooldownText(lightningCooldownText, lightningCooldownTimer);
        }
        else
        {
            //lightningCooldownText.text = "Ready";
        }
        if (confusionCooldownTimer > 0f)
        {
            confusionCooldownTimer -= Time.deltaTime;
            //UpdateCooldownText(confusionCooldownText, confusionCooldownTimer);
        }
        else
        {
            //confusionCooldownText.text = "Ready";
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
                UIManager.Instance.ToggleDeselect(false);
            }
        }
    }

    void StartSkillCasting(SkillType skill)
    {
        isCasting = true;

        if (currentPreview != null)
        {
            Destroy(currentPreview);
        }

        switch (skill) {
            case SkillType.Meteor:
                currentPreview = Instantiate(meteorPreviewPrefab);
                break;
            case SkillType.Lightning:
                currentPreview = Instantiate(lightingPreviewPrefab);
                break;
            case SkillType.Confusion:
                currentPreview = Instantiate(confusionPreviewPrefab);
                break;
        }


        
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
                    if (CastMeteor(castPosition)) 
                        meteorCooldownTimer = meteorCooldown;
                    break;
                case SkillType.Lightning:
                    if (CastLightning(castPosition))
                        lightningCooldownTimer = lightningCooldown;
                    break;
                case SkillType.Confusion:
                    if (CastConfusion(castPosition))
                        confusionCooldownTimer = confusionCooldown;
                    break;
            }

            // Destroy the preview after casting
            Destroy(currentPreview);

            isCasting = false;

            UIManager.Instance.ToggleDeselect(false);
        }
    }

    bool CastMeteor(Vector3 castPosition)
    {
        if (!Player.Instance.CheckAndUseMana(meteorCost)) return false;
        GameObject meteor = Instantiate(meteorPrefab, currentPreview.transform.position, Quaternion.identity);

        Meteor meteorScript = meteor.GetComponent<Meteor>();
        meteorScript.targetPosition = castPosition;
        return true;
    }

    bool CastLightning(Vector3 castPosition)
    {
        if (!Player.Instance.CheckAndUseMana(lightningCost)) return false;
        GameObject lightning = Instantiate(lightningPrefab, currentPreview.transform.position, Quaternion.identity);

        Lightning lightningScript = lightning.GetComponent<Lightning>();
        lightningScript.targetPosition = castPosition;
        return true;
    }

    bool CastConfusion(Vector3 castPosition)
    {
        if (!Player.Instance.CheckAndUseMana(confusionCost)) return false;

        GameObject confusion = Instantiate(confusionPrefab, currentPreview.transform.position, Quaternion.identity);

        Confusion confusionScript = confusion.GetComponent<Confusion>();
        confusionScript.targetPosition = castPosition;
        return true;
    }

    void SelectSkill(SkillType skillType)
    {
        UIManager.Instance.ToggleDeselect(true);
        UIManager.Instance.ToggleTowerSelection(false);
        selectedSkill = skillType;
        Debug.Log("Selected skill: " + selectedSkill);
        StartSkillCasting(skillType);
    }

    void UpdateCooldownText(TextMeshProUGUI cooldownText, float timer)
    {
        // Show the remaining cooldown time with two decimals
        cooldownText.text = timer.ToString("F2") + "s"; 
    }
}
