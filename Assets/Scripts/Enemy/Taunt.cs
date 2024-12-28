using System.Collections.Generic;
using UnityEngine;

public class Taunt : MonoBehaviour
{
    [SerializeField] float radius;
    [SerializeField] LayerMask towerLayer;
    [SerializeField] private float countDown;
    [SerializeField] float duration;

    private List<TowerBehavior> towersInRange;
    GameManager gameManager;
    Enemy enemy;

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameManager.Instance;
        enemy = GetComponent<Enemy>();
        countDown = 8f;
        towersInRange = new();
    }

    private void Update()
    {
        countDown -= Time.deltaTime;

        if (countDown < 0)
        {
            TauntTowersInRange();

            countDown = 8f;
        }
    }

    public void TauntTowersInRange()
    {
        //Find all tower objects within range
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, radius, Vector3.forward, 0, LayerMask.NameToLayer("Tower"));

        //Clears list
        towersInRange.Clear();
        int count = hits.Length;
        for (int i = 0; i < count; i++)
        {
            //Filters out everything that is not a Tower
            if (hits[i].collider.gameObject.TryGetComponent(out TowerBehavior towerBehavior) && hits[i].collider != gameObject.GetComponent<Collider>())
                towersInRange.Add(towerBehavior); //Adds object to list
        }

        int towersInRangeCount = towersInRange.Count;
        for (int i = 0; i < towersInRangeCount; i++)
        {
            GameManager.Buff buff = new GameManager.Buff(GameManager.BuffNames.Taunt, 1, duration, true);
            GameManager.ApplyBuffData buffData = new GameManager.ApplyBuffData(buff, towersInRange[i]);
            gameManager.EnqueueBuffToApply(buffData);
            towersInRange[i].tauntTarget = enemy;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
