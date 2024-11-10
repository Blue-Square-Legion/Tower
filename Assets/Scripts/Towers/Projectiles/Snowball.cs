using System;
using System.Collections;
using UnityEngine;

public class Snowball : MonoBehaviour
{
    [SerializeField] private GameObject snowTerrain;
    [SerializeField] private AnimationCurve animationCurve;

    private Vector3 startPostion;
    private Vector3 endPosition;
    [NonSerialized] public float snowSize;
    [NonSerialized] public Enemy target;
    [NonSerialized] public IceDamage parent;

    /// <summary>
    /// Call Init after assigning the snowball a target
    /// </summary>
    public void Init()
    {
        startPostion = transform.position;
        if (target != null && parent != null)
        {
            Transform targetTransform = target.gameObject.transform;

            //Gets target position, then subtracts the target's height, then leads the snowball a bit
            endPosition = targetTransform.position - new Vector3(0, targetTransform.localScale.y, 0) + targetTransform.forward * 2;
            StartCoroutine(Fire());
        }
        else
            Debug.LogWarning("Failed to Launch. Requires a target and a parent");
    }

    IEnumerator Fire()
    {
        //Sets up snowball
        float duration = 0.4f;
        float time = 0f;

        //Arc
        while (time < duration)
        {
            time += Time.deltaTime;

            float linearTime = time / duration; //Gets time
            float heightTime = animationCurve.Evaluate(linearTime); //Gets data from animation curve

            float height = Mathf.Lerp(0, 2, heightTime); //Start, apex, interval

            transform.position = Vector3.Lerp(startPostion, endPosition, linearTime) + new Vector3(0,height, 0); //Displaces snowball by height

            yield return null;
        }

        //Spawn Snow
        GameObject snow = Instantiate(snowTerrain, new Vector3(endPosition.x, 0, endPosition.z), Quaternion.identity);
        Snow tempSnow = snow.GetComponent<Snow>();
        tempSnow.duration = parent.GetSnowDuration();
        tempSnow.slowModifier = parent.GetSnowSpeed();
        tempSnow.transform.localScale = new Vector3(snowSize, tempSnow.transform.localScale.y, snowSize);
        tempSnow.Init();
        Destroy(gameObject);
    }
}
