using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Card : MonoBehaviour
{
    //Need an image/3d model variable
    [SerializeField] public TMP_Text cost;
    [SerializeField] private TMP_Text cardName;
    [SerializeField] private TMP_Text cardType;
    [SerializeField] private TMP_Text effect;
    public GameObject position;
    public GameObject cube;
    public GameObject Camera;
    public GameObject canvas;

    public Card(int costIn, string cardNameIn, string cardTypeIn, string effectIn, GameObject camera)
    {
        cost.text = "" + costIn;
        cardName.text = cardNameIn;
        cardType.text = cardTypeIn;
        effect.text = effectIn;
        Camera = camera;
    }
    public void setData(int costIn, string cardNameIn, string cardTypeIn, string effectIn)
    {
        cost.text = "" + costIn;
        cardName.text = cardNameIn;
        cardType.text = cardTypeIn;
        effect.text = effectIn;
    }

    public void setCamera(GameObject camera)
    {
        
        Camera = camera;
        canvas.GetComponent<Canvas>().worldCamera = Camera.GetComponent<Camera>();
    }
    private void clicked()
    {

    }    


    private void execute()
    {
        //this is where we get to add the fun stuff later.
    }


}
