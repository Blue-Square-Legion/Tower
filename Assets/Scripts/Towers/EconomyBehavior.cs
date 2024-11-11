using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EconomyBehavior : MonoBehaviour
{
    public int bonus;
    void Start()
    {
        bonus = 50;
        GameManager.Instance.farmBonus += bonus;
    }
}
