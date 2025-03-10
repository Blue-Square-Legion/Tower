using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Card : MonoBehaviour
{
    //Need an image/3d model variable
    public int cost;
    public int id;
    public string cardName;
    public string cardType;
    public string effect;

    public GameObject position;
    public GameObject cube;
    public GameObject Camera;
    public GameObject canvas;
    public Cards cardData;

    public GameObject cardManager;
    public GameObject player;

    public GameObject cardModel;

    public Card(Cards scriptObject, GameObject camera)
    {
        cost = scriptObject.cost;
        cardName = scriptObject.cardName;
        cardType = scriptObject.cardType;
        effect = scriptObject.effect;
    }
    public void setData(int costIn, string cardNameIn, string cardTypeIn, string effectIn)
    {
        cost = costIn;
        cardName = cardNameIn;
        cardType = cardTypeIn;
        effect = effectIn;
    }

    public void setCamera(GameObject camera)
    {
        
        Camera = camera;
        canvas.GetComponent<Canvas>().worldCamera = Camera.GetComponent<Camera>();
    }
    private void clicked()
    {

    }    


    public void execute()
    {
        //this is where we get to add the fun stuff later.
        //player.GetComponent<TowerPlacement>().SetTowerToPlace(cardManager.GetComponent<CardManager>().towerPrefabs[cardData.towerIndex]);
        cardData.playCard(player, cardManager);
        cardManager.GetComponent<CardManager>().cardBeingPlayed(id);
    }

    public void setModel()
    {
        cardModel.GetComponent<MeshRenderer>().materials[0].SetTexture("_BaseMap", cardData.cardMaterial);
        //GetComponent<Material>().SetTexture("Card Image", cardData.cardMaterial); "_BaseMap"

    }

}
