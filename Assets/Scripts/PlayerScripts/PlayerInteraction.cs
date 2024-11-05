using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    public float hitDistance = 5f;
    public LayerMask hitLayers;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            HitObjectsInFront();
        }
    }

    void HitObjectsInFront()
    {
        RaycastHit hit;
        Vector3 rayOrigin = Camera.main.transform.position;
        Vector3 rayDirection = Camera.main.transform.forward;

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, hitDistance, hitLayers))
        {
            Debug.Log("Hit: " + hit.collider.gameObject.name);
        }
        else
        {
            Debug.Log("No object hit!");
        }

        Debug.DrawRay(rayOrigin, rayDirection * hitDistance, Color.red);
    }
}
