using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class floatingText : MonoBehaviour
{
    Transform mainCam;
    Transform units;
    Transform worldSpace;

    public Vector3 offset;
    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main.transform;
        units = transform.parent;
        worldSpace = GameObject.FindObjectOfType<Canvas>().transform;

        transform.SetParent(worldSpace);
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(transform.position - mainCam.position);
        transform.position = units.position - offset;
    }
}
