using UnityEngine;

public class IceDamage : MonoBehaviour, IDamageMethod
{
    [SerializeField] private GameObject snowBall;
    [SerializeField] private float snowDuration;
    [SerializeField] private float snowSpeed;
    [SerializeField] private float snowSize;
    GameManager gameManager;
    private float damage;
    private float fireRate;
    private float delay;
    public void Init(float damage, float fireRate)
    {
        gameManager = GameManager.Instance;
        this.damage = damage;
        this.fireRate = fireRate;
        delay = 1f / fireRate;
    }

    #region Getters and Setters
    public void UpdateDamage(float damage)
    {
        this.damage = damage;
    }

    public void UpdateFireRate(float fireRate)
    {
        this.fireRate = fireRate;
    }

    public void UpdateSnowDuration(float time)
    {
        this.snowDuration = time;
    }

    public void UpdateSnowSpeed(float speed)
    {
        snowSpeed = speed;
    }

    public void UpdateSnowSize(float size)
    {
        snowSize = size;
    }

    public float GetSnowDuration()
    {
        return snowDuration;
    }

    public float GetSnowSpeed()
    {
        return snowSpeed;
    }

    public float GetSnowSize()
    {
        return snowSize;
    }
    #endregion

    public void damageTick(Enemy target)
    {
        if (target)
        {
            if (delay > 0)
            {
                delay -= Time.deltaTime;
                return;
            }

            GameObject snowBallObject = Instantiate(snowBall, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);
            Snowball snowballScript = snowBallObject.GetComponent<Snowball>();
            snowballScript.target = target;
            snowballScript.parent = this;
            snowballScript.snowSize = snowSize;
            snowballScript.Init();

            delay = 1 / fireRate;
        }
    }
}