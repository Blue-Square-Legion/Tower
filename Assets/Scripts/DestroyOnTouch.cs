using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnTouch : MonoBehaviour
{
    [SerializeField] PlayerHealth playerHealth;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            playerHealth.DoDamage(1);
            Destroy(other.gameObject);
        }
    }
}
