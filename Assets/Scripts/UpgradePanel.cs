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
    [SerializeField] GameObject upgradeButton;
    [SerializeField] GameObject sellButton;
    [SerializeField] TextMeshProUGUI upgradeDescriptionText;
    [SerializeField] TextMeshProUGUI upgradeTargetText;
    [SerializeField] GameObject leftArrowButton;
    [SerializeField] GameObject rightArrowButton;

    [NonSerialized] public TowerBehavior target;
    private int currentTargetIndex = 0;
    private int targetNum = Enum.GetValues(typeof(TargetType)).Length - 1;
    private int maxUpgradeLevel = 4;

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
        maxUpgradeLevel = target.GetMaxUpgradeLevel();
        UpdateTargetInfo();
    }

    public void SetUpgradeButton(int price)
    {
        upgradeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Upgrade\n$" + price;
    }

    public void ToggleUpgradeButton(bool isActive)
    {
        upgradeButton.SetActive(isActive);
    }

    public void ToggleSellButton(bool isActive)
    {
        sellButton.SetActive(isActive);
    }

    public void SetSellButton(int price)
    {
        sellButton.GetComponentInChildren<TextMeshProUGUI>().text = "Sell\n$" + price;
    }

    public void SetText(string upgradeText)
    {
        upgradeDescriptionText.text = upgradeText;
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

    public void UpgradePressed()
    {

           target.Upgrade();
        
    }

    public void SellPressed()
    {
        if (target.GetComponent<EconomyBehavior>() != null)
            GameManager.Instance.farmBonus -= target.GetComponent<EconomyBehavior>().bonus;
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
        ToggleUpgradeButton(target.upgradeLevel != maxUpgradeLevel);

    }
}
