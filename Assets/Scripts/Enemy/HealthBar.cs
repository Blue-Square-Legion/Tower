using UnityEngine;

public class HealthBar : MonoBehaviour
{
    MeshRenderer mRenderer;
    int maxHealth, currentHealth;
    float maxSize, currentSize;

    //RBG values
    float redValue;
    const int BLUE_VALUE = 0;
    float greenValue;

    void Start()
    {
        mRenderer = GetComponent<MeshRenderer>();
        maxHealth = GetComponentInParent<Enemy>().maxHealth;
        currentHealth = maxHealth;
        maxSize = transform.localScale.x;
        currentSize = maxSize;

        redValue = 0;
        greenValue = 255;
    }

    public void UpdateHealth(int newHealth)
    {
        currentHealth = newHealth;
        //Caps HP bar to max size
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        //Sets size of the health bar;
        float sizeRatio = (float) currentHealth / maxHealth * maxSize;
        transform.localScale = new Vector3(sizeRatio, transform.localScale.y, transform.localScale.z);
    }
}
