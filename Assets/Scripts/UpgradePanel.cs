using System;
using TMPro;
using UnityEngine;

public class UpgradePanel : MonoBehaviour
{
    #region
    private static UpgradePanel instance;
    public static UpgradePanel Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType(typeof(UpgradePanel)) as UpgradePanel;
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

    [NonSerialized] public TowerBehavior target;
    void Start()
    {
        upgradePanel.gameObject.SetActive(false);
        target = null;
    }

    public void SetUpgradePanel(bool isActive)
    {
        upgradePanel.gameObject.SetActive(isActive);
    }

    public void SetTarget(TowerBehavior target)
    {
        this.target = target;
    }

    public void SetUpgradeButton(int price)
    {
        upgradeButton.GetComponentInChildren<TextMeshProUGUI>().text = "Upgrade\n$" + price;
    }

    public void ToggleUpgradeButton(bool isActive)
    {
        upgradeButton.SetActive(isActive);
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
        target.Upgrade();
    }

    public void SellPressed()
    {
        if (target.GetComponent<EconomyBehavior>() != null)
            GameManager.Instance.farmBonus -= target.GetComponent<EconomyBehavior>().bonus;
        TowerPlacement.Instance.SellTower(target.gameObject);
    }
}