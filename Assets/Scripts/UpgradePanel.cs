using System;
using TMPro;
using UnityEngine;
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

    [Header("Upgrade")]
    [SerializeField] GameObject upgradeButton1;
    [SerializeField] TextMeshProUGUI upgradeDescriptionText1;

    [SerializeField] GameObject upgradeButton2;
    [SerializeField] TextMeshProUGUI upgradeDescriptionText2;

    [SerializeField] GameObject upgradeButton3;
    [SerializeField] TextMeshProUGUI upgradeDescriptionText3;

    [Header("Targeting")]
    [SerializeField] TextMeshProUGUI upgradeTargetText;
    [SerializeField] GameObject leftArrowButton;
    [SerializeField] GameObject rightArrowButton;

    [NonSerialized] public TowerBehavior target;
    private int currentTargetIndex = 0;
    private int targetNum = Enum.GetValues(typeof(TargetType)).Length - 1;
    private int maxUpgradeLevel = 3;

    void Start()
    {
        upgradePanel.SetActive(false);
        target = null;
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
        sellButton.GetComponentInChildren<TextMeshProUGUI>().text = "Sell\n$" + price;
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
}