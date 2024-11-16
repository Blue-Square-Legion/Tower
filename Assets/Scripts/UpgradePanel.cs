using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] GameObject leftArrowButton;
    [SerializeField] GameObject rightArrowButton;

    [NonSerialized] public TowerBehavior target;
    private int currentUpgradeIndex = 0;
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

    public void SetTarget(TowerBehavior target)
    {
        this.target = target;
        currentUpgradeIndex = target.upgradeLevel;
        maxUpgradeLevel = target.GetMaxUpgradeLevel();
        UpdateUpgradeInfo();
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

    public void UpgradePressed()
    {
        if (currentUpgradeIndex == target.upgradeLevel)
        {
            target.Upgrade();
            SetTarget(target);
        }
    }

    public void SellPressed()
    {
        if (target.GetComponent<EconomyBehavior>() != null)
            GameManager.Instance.farmBonus -= target.GetComponent<EconomyBehavior>().bonus;
        TowerPlacement.Instance.SellTower(target.gameObject);
    }

    public void OnLeftArrowClicked()
    {
        if (currentUpgradeIndex > 0)
        {
            currentUpgradeIndex--;
            UpdateUpgradeInfo();
        }
    }

    public void OnRightArrowClicked()
    {
        if (currentUpgradeIndex < maxUpgradeLevel)
        {
            currentUpgradeIndex++;
            UpdateUpgradeInfo();
        }
    }

    private void UpdateUpgradeInfo()
    {
        if (target == null) return;

        string upgradeDescription = target.GetUpgradeDescription(currentUpgradeIndex);
        SetText(upgradeDescription);

        int upgradeCost = target.GetUpgradeCost(currentUpgradeIndex);
        SetUpgradeButton(upgradeCost);


        ToggleUpgradeButton(currentUpgradeIndex != maxUpgradeLevel);

        ToggleSellButton(currentUpgradeIndex == target.upgradeLevel);

    }
}
