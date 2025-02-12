using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    #region Singleton
    private static Player instance;
    public static Player Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType(typeof(Player)) as Player;
            return instance;
        }
        set
        {
            instance = value;
        }
    }
    #endregion


    [SerializeField] TMP_Text healthText;
    [SerializeField] TMP_Text moneyText;
    [SerializeField] TMP_Text manaText;
    [SerializeField] int maxHealth;
    [SerializeField] float maxMana;
    [SerializeField] int startingMoney;
    private float currentMana;
    private int currentHealth;
    private float money;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;
        money = startingMoney;
    }

    // Update is called once per frame
    void Update()
    {
        healthText.SetText(currentHealth.ToString());
        moneyText.SetText(money.ToString());

        // regen mana over time
        //if (GameManager.Instance.waveActive && currentMana < maxMana)
        //    currentMana += 4 * Time.deltaTime;

        manaText.text = currentMana.ToString("0");
    }

    public void RegenMana(float mana)
    {
        currentMana += mana;
        if (currentMana > maxMana)
            currentMana = maxMana;
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

    public void GiveMoney(float amount)
    {
        money += amount;
    }

    public int GetMoney()
    {
        return (int) money;
    }

    public void RemoveMoney(int amount)
    {
        money -= amount;
    }

    //deduct mana if enough, else return false
    public bool CheckAndUseMana(float cost)
    {
        if (cost > currentMana)
        {
            print("not enough mana");
            return false;
        }
        else
        {
            currentMana -= cost;
            return true;
        }
    }
}