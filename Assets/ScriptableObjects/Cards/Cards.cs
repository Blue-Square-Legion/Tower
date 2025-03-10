using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "Cards")]
public class Cards : ScriptableObject
{

    //Need an image/3d model variable
    public int cost;
    public string cardName;
    public string cardType;
    public string[] subtype;
    public string effect;
    public int towerIndex;

    public GameObject cardModel;

    public Texture cardMaterial;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void playCard(GameObject player, GameObject cardManager)
    {
        if (cardType != "Rune")
            player.GetComponent<TowerPlacement>().SetTowerToPlace(cardManager.GetComponent<CardManager>().towerPrefabs[towerIndex]);
        else
            player.GetComponent<RunePlacement>().StartCasting(subtype[0]);
    }
}
