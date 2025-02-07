using UnityEngine;
using AudioSystem;

public class IceDamage : TowerDamage
{
    public Animator snowballAnimator;
    [SerializeField] private GameObject snowball;
    [SerializeField] private Transform snowballSpawn;
    [SerializeField] private float snowDuration;
    [SerializeField] private float snowSpeed;
    [SerializeField] private float snowSpeedReduction;
    [SerializeField] private float snowSize;

    #region Getters and Setters
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

    public float GetSnowSpeedReduction()
    {
        return snowSpeedReduction;
    }

    public void UpdateSnowSpeedReduction(float snowSpeedReduction)
    {
        this.snowSpeedReduction = snowSpeedReduction;
    }

    public float GetSnowSize()
    {
        return snowSize;
    }
    #endregion

    public override void DamageTick(Enemy target)
    {
        if (target)
        {
            if (delay > 0)
            {
                delay -= Time.deltaTime;
                return;
            }
            
            snowballAnimator.SetTrigger("CatapultFire");
            
            AudioManager.Instance.CreateAudio()
                .WithAudioData(audioData)
                .WithPosition(gameObject.transform.position)
                .Play();

            GameObject snowBallObject = Instantiate(snowball, new Vector3(snowballSpawn.position.x, snowballSpawn.position.y, snowballSpawn.position.z), Quaternion.identity);
            Snowball snowballScript = snowBallObject.GetComponent<Snowball>();
            snowballScript.target = target;
            snowballScript.parent = this;
            snowballScript.snowSize = snowSize;
            snowballScript.duration = snowSpeed;
            snowballScript.Init();

            delay = 1 / fireRate;
        }
    }
}