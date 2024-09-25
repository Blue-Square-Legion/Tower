using TMPro;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] TMP_Text healthText;
    [SerializeField] TMP_Text moneyText;
    [SerializeField] int maxHealth;
    [SerializeField] int startingMoney;
    private int currentHealth;
    private int money;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        money = startingMoney;
    }

    // Update is called once per frame
    void Update()
    {
        healthText.SetText("Health: " + currentHealth.ToString());
        moneyText.SetText("Money: " + money.ToString());
    }

    public void DoDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            //gameover
            UIManager.Instance.GameOver();
        }
    }

    public void GiveMoney(int amount)
    {
        money += amount;
    }

    public int GetMoney()
    {
        return money;
    }

    public void RemoveMoney(int amount)
    {
        money -= amount;
    }
}
