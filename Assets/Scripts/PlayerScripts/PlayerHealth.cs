using TMPro;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    [SerializeField] int maxHealth;
    private int currentHealth;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        text.SetText("Health: " + currentHealth.ToString());
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
}
