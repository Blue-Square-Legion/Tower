using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunTowersChild : MonoBehaviour
{
    private StunTowers parent;

    private void Start()
    {
        parent = GetComponentInParent<StunTowers>();
    }
    public void DoStunAttack()
    {
        parent.StunTowersInRange();
    }

    public void ExitAnimation()
    {
        parent.ExitAnimation();
    }
}
