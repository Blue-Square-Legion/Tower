using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class CardButton : MonoBehaviour
{
    public GameObject manager;
    public float normalZ;
    public GameObject parent;


    public void OnMouseUpAsButton()
    {
        parent.GetComponent<Card>().execute();
    }

    void OnMouseOver()
    {
        transform.localPosition = new Vector3(0, 0, 0f);
    }

    void OnMouseExit()
    {
        transform.localPosition = new Vector3(0, 0, 1f);
    }
    
}
