using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType(typeof(GameManager)) as GameManager;
            return instance;
        }
        set 
        {
            instance = value;
        }
    }
    #endregion


    public List<TowerBehavior> towers;
    public Vector3[] nodePositions;
    public float[] nodeDistances;
    void Start()
    {
        towers = new List<TowerBehavior>();
    }

    void Update()
    {
        
    }
}