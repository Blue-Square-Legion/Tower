using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static TowerTargetting;

public class UpgradePanel : MonoBehaviour
{
    #region Singleton
    private static UpgradePanel instance;
    public static UpgradePanel Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<UpgradePanel>();
            return instance;
        }
        set
        {
            instance = value;
        }
    }
    #endregion

    [SerializeField] GameObject upgradePanel;

    [SerializeField] GameObject sellButton;
    [SerializeField] TextMeshProUGUI towerName;
    [Header("Upgrade")]
    [SerializeField] GameObject upgradeButton1;
    [SerializeField] TextMeshProUGUI upgradeDescriptionText1;
    [SerializeField] private Image restrictPath1;

    [SerializeField] GameObject upgradeButton2;
    [SerializeField] TextMeshProUGUI upgradeDescriptionText2;
    [SerializeField] private Image restrictPath2;

    [SerializeField] GameObject upgradeButton3;
    [SerializeField] TextMeshProUGUI upgradeDescriptionText3;
    [SerializeField] private Image restrictPath3;

    [Header("Targeting")]
    [SerializeField] TextMeshProUGUI upgradeTargetText;
    [SerializeField] GameObject leftArrowButton;
    [SerializeField] GameObject rightArrowButton;

    [NonSerialized] public TowerBehavior target;

    [Header("Progress Bar")]
    [SerializeField] Image progressBar1;
    [SerializeField] Image progressBar2, progressBar3;
    [SerializeField] Sprite zeroUpgrades, oneUpgrades, twoUpgrades, threeUpgrades, fourUpgrades;

    [Header("Tooltips")]
    [SerializeField] private GameObject tooltip1;
    [SerializeField] private GameObject tooltip2, tooltip3;

    private int currentTargetIndex = 0;
    private int targetNum = Enum.GetValues(typeof(TargetType)).Length - 1;
    private int maxUpgradeLevel;

    void Start()
    {
        upgradePanel.SetActive(false);
        target = null;

        if (restrictPath1 != null)
            restrictPath1.enabled = false;
        if (restrictPath2 != null)
            restrictPath2.enabled = false;
        if (restrictPath3 != null)
            restrictPath3.enabled = false;

        if (progressBar1 != null)
            progressBar1.sprite = zeroUpgrades;
        if (progressBar2 != null)
            progressBar2.sprite = zeroUpgrades;
        if (progressBar3 != null)
            progressBar3.sprite = zeroUpgrades;

        if (tooltip1 != null)
            tooltip1.SetActive(false);
        if (tooltip2 != null)
            tooltip2.SetActive(false);
        if (tooltip3 != null)
            tooltip3.SetActive(false);

    }

    public void SetUpgradePanel(bool isActive)
    {
        upgradePanel.SetActive(isActive);
    }

    public void SetTarget(TowerBehavior target, int type)
    {
        this.target = target;
        currentTargetIndex = type;
        maxUpgradeLevel = target.GetMaxUpgradeLevel(1) + 1;
        UpdateTargetInfo();
        towerName.text = target.towerType.ToString();
    }

    public void ToggleUpgradeButton(bool isActive, int path)
    {
        switch(path)
        {
            case 1:
                upgradeButton1.SetActive(isActive);
                break;
            case 2:
                upgradeButton2.SetActive(isActive);
                break;
            case 3:
                upgradeButton3.SetActive(isActive);
                break;
        }
    }

    public void ToggleSellButton(bool isActive)
    {
        sellButton.SetActive(isActive);
    }

    public void SetSellButton(int price)
    {
        sellButton.GetComponentInChildren<TextMeshProUGUI>().text = price.ToString();
    }

    public void SetText(string upgradeText, int path)
    {
        switch(path)
        {
            case 1:
                upgradeDescriptionText1.text = upgradeText;
                break;
            case 2:
                upgradeDescriptionText2.text = upgradeText;
                break;
            case 3:
                upgradeDescriptionText3.text = upgradeText;
                break;
        }
    }

    public void SetTargetIndex(int t)
    {
        currentTargetIndex = t;
        SetTargetText();
    }

    public void SetTargetText()
    {
        switch (currentTargetIndex)
        {
            case 0:
                upgradeTargetText.GetComponentInChildren<TextMeshProUGUI>().text = "First";
                break;
            case 1:
                upgradeTargetText.GetComponentInChildren<TextMeshProUGUI>().text = "Last";
                break;
            case 2:
                upgradeTargetText.GetComponentInChildren<TextMeshProUGUI>().text = "Close";
                break;
            case 3:
                upgradeTargetText.GetComponentInChildren<TextMeshProUGUI>().text = "Furthest";
                break;
            default:
                Debug.Log("Error No Target Type");
                break;
        }
    }

    public void UpgradePressed(int path)
    {
        switch(path)
        {
            case 1:
                target.Upgrade1();
                break;
            case 2:
                target.Upgrade2();
                break;
            case 3:
                target.Upgrade3();
                break;
        }        
    }

    public void SellPressed()
    {
        if (target.TryGetComponent(out EconomyBehavior economy))
        {
            GameManager.Instance.farmBonus -= target.GetComponent<EconomyBehavior>().bonus;
            economy.RemoveBuffs();

        }
        if (target.TryGetComponent(out SupportBehavior support))
            support.RemoveBuffs();
        TowerPlacement.Instance.SellTower(target.gameObject);
    }

    public void OnLeftArrowClicked()
    {
        
       if (currentTargetIndex > 0)
        {
            currentTargetIndex--;
            UpdateTargetInfo();
        } 
        else
        {
            currentTargetIndex = targetNum;
            UpdateTargetInfo();
        }

    }

    public void OnRightArrowClicked()
    {
        
        if (currentTargetIndex < targetNum)
        {
            currentTargetIndex++;
            UpdateTargetInfo();
        } else
        {
            currentTargetIndex = 0;
            UpdateTargetInfo();
        }
    }

    private void UpdateTargetInfo()
    {
        if (target == null) return;

        SetTargetText();
        target.SetTargetType(currentTargetIndex);
        ToggleUpgradeButton(target.upgradeLevel1 < maxUpgradeLevel, 1);
        ToggleUpgradeButton(target.upgradeLevel2 < maxUpgradeLevel, 2);
        ToggleUpgradeButton(target.upgradeLevel3 < maxUpgradeLevel, 3);
    }

    public void RestrictPaths(bool isPath1Restricted, bool isPath2Restricted, bool isPath3Restricted)
    {
        if ( restrictPath1 != null)
        {
            if (isPath1Restricted)
            {
                restrictPath1.enabled = true;
            }
            else
            {
                restrictPath1.enabled = false;
            }
        }

        if (restrictPath2 != null)
        {
            if (isPath2Restricted)
            {
                restrictPath2.enabled = true;
            }
            else
            {
                restrictPath2.enabled = false;
            }
        }

        if (restrictPath3 != null)
        {
            if (isPath3Restricted)
            {
                restrictPath3.enabled = true;
            }
            else
            {
                restrictPath3.enabled = false;
            }
        }
    }

    public void UpdateProgressBar(int path, int level)
    {
        switch(path)
        {
            case 1:
                switch(level)
                {
                    case 0:
                        progressBar1.sprite = zeroUpgrades;
                        break;
                    case 1:
                        progressBar1.sprite = oneUpgrades;
                        break;
                    case 2:
                        progressBar1.sprite = twoUpgrades;
                        break;
                    case 3:
                        progressBar1.sprite = threeUpgrades;
                        break;
                    case 4:
                        progressBar1.sprite = fourUpgrades;
                        break;
                    default:
                        print("ERROR: INVALID LEVEL");
                        break;
                }
                break;
            case 2:
                switch (level)
                {
                    case 0:
                        progressBar2.sprite = zeroUpgrades;
                        break;
                    case 1:
                        progressBar2.sprite = oneUpgrades;
                        break;
                    case 2:
                        progressBar2.sprite = twoUpgrades;
                        break;
                    case 3:
                        progressBar2.sprite = threeUpgrades;
                        break;
                    case 4:
                        progressBar2.sprite = fourUpgrades;
                        break;
                    default:
                        print("ERROR: INVALID LEVEL");
                        break;
                }
                break;
            case 3:
                switch (level)
                {
                    case 0:
                        progressBar3.sprite = zeroUpgrades;
                        break;
                    case 1:
                        progressBar3.sprite = oneUpgrades;
                        break;
                    case 2:
                        progressBar3.sprite = twoUpgrades;
                        break;
                    case 3:
                        progressBar3.sprite = threeUpgrades;
                        break;
                    case 4:
                        progressBar3.sprite = fourUpgrades;
                        break;
                    default:
                        print("ERROR: INVALID LEVEL");
                        break;
                }
                break;
            default:
                print("ERROR: FAILED TO GET PATH");
                break;
        }
    }

    public void OnMouseEnterUpgrade(int path)
    {
        switch(path)
        {
            case 1:
                tooltip1.SetActive(true);
                break;
            case 2:
                tooltip2.SetActive(true);
                break;
            case 3:
                tooltip3.SetActive(true);
                break;
        }
    }

    public void OnMouseExitUpgrades(int path)
    {
        switch (path)
        {
            case 1:
                tooltip1.SetActive(false);
                break;
            case 2:
                tooltip2.SetActive(false);
                break;
            case 3:
                tooltip3.SetActive(false);
                break;
        }
    }
}