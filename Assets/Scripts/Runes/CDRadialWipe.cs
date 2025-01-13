using UnityEngine.UI;
using UnityEngine;

public class CDRadialWipe : MonoBehaviour
{
    public Image maskImage;  // Reference to the Image component (mask)
    public float wipeDuration = 5f;  // Time in seconds for full wipe
    public float timer = 0f;
    public bool isWiping = false;

    [SerializeField] RunePlacement.SkillType rune;

    private void Start()
    {
        maskImage = GetComponent<Image>();
        switch(rune)
        {
            case RunePlacement.SkillType.Meteor:
                wipeDuration = RunePlacement.Instance.meteorCooldown;
                break;
            case RunePlacement.SkillType.Lightning:
                wipeDuration = RunePlacement.Instance.lightningCooldown;
                break;
            case RunePlacement.SkillType.Confusion:
                wipeDuration = RunePlacement.Instance.confusionCooldown;
                break;
            case RunePlacement.SkillType.Cleanse:
                wipeDuration = RunePlacement.Instance.cleanseCooldown;
                break;
        }
    }

    void Update()
    {
        if (isWiping)
        {
            timer += Time.deltaTime;
            // Calculate the fill amount based on timer
            float fillAmount = timer / wipeDuration;

            // Set the fill amount to the mask image
            maskImage.fillAmount = Mathf.Clamp01(fillAmount);

            //// If the wipe is complete, stop the process
            if (timer >= wipeDuration)
            {
                isWiping = false;
                timer = 0f;
            }
        }
    }

    public void StartWipe()
    {
        isWiping = true;
    }
}
