using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UI;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class CardManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform handPosition;
    public GameObject cardParent;
    public GameObject Camera;
    public GameObject Player;
    public GameObject self;
    public GameObject[] towerPrefabs;

    public List<GameObject> deck = new List<GameObject>();
    public List<GameObject> hand = new List<GameObject>();
    public List<GameObject> discard = new List<GameObject>();



    public Cards[] data;


    float cardSpacing = 1f;
    float vSpacing = 1f;
    public float distance = 10;

    int maxHandsize = 10;
    int lastID;

    int cardBeingPlayedID;

    private void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject tempCard = Instantiate(cardPrefab, handPosition.position,Quaternion.identity);
            tempCard.SetActive(false);
            deck.Add(tempCard);
            tempCard.transform.parent = cardParent.transform;
            tempCard.GetComponent<Card>().setCamera(Camera);
            tempCard.GetComponentInChildren<CardButton>().manager = self;
            tempCard.GetComponent<Card>().cardData = data[i%8];
            tempCard.GetComponent<Card>().cardManager = self;
            tempCard.GetComponent<Card>().player = Player;
            tempCard.GetComponentInChildren<CardButton>().parent = tempCard;
            tempCard.GetComponent<Card>().setModel();
            tempCard.GetComponent<Card>().id = i;
            lastID = i;
        }
        shuffle();
        for(int i = 0;i< 3; i++)
        {
            drawCard();
        }

    }

    public void drawCard()
    {
        if (hand.Count < maxHandsize)
        {
            if (deck.Count > 0)
            {
                hand.Add(deck[0]);
                deck.RemoveAt(0);
                hand[hand.Count - 1].SetActive(true);
            }
            else
            {
                Debug.Log("No cards in deck");
                addDiscardIntoDeck();

                if (deck.Count > 0)
                {
                    hand.Add(deck[0]);
                    deck.RemoveAt(0);
                }
            }
            updateHandVisuals();
        }
    }

    public void addDiscardIntoDeck()
    {
        while (discard.Count > 0)
        {
            deck.Add(discard[0]);
            discard.RemoveAt(0);
        }
        shuffle();
    }

    public void shuffle()
    {
        for (int i = 0; i < 3; i++)
        {
            int n = deck.Count;
            while (n > 1)
            {
                n--;
                int k = Random.Range(0, n + 1);
                GameObject temp = deck[k];
                deck[k] = deck[n];
                deck[n] = temp;
            }
        }
    }

    public void cardBeingPlayed(int cardID)
    {
        cardBeingPlayedID = cardID;
    }
    public void cardPlayed()
    {
        if (cardBeingPlayedID != -1)
        {
            int pos = getCardHandPos(cardBeingPlayedID);
            hand[pos].SetActive(false);
            removeFromHand(pos);
            cardBeingPlayedID = -1; //reset for next card
        }

    }

    public void cancelCard()
    {
        cardBeingPlayedID = -1;
    }

    public void removeFromHand(int a)
    {
        if (hand.Count > a && a != -1)
        {
            discard.Add(hand[a]);
            hand.RemoveAt(a);
        }
    }

    public int getCardHandPos(int cardID)
    {
        int cardPos = -1;
        for(int i = 0;i<hand.Count;i++)
        {
            if (hand[i].GetComponent<Card>().id == cardID)
                cardPos = i;
        }
        return cardPos;
    }

    public void addToDiscard(GameObject card)
    {
        discard.Add(card);
    }

    private void Update()
    {
        updateHandVisuals();
    }

    public void updateHandVisuals()
    {
        int cards = hand.Count;

        if (cards == 1)
        {
            float rotationAngle = 0;
            hand[0].transform.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);

            float hOffset = handPosition.position.x;

            float normalizedPos = 2;
            float vOffset = handPosition.position.y + vSpacing * (1 - normalizedPos * normalizedPos);

            //set Position
            hand[0].GetComponent<Card>().transform.position = new Vector3(hOffset, vOffset - 5, 1f);
        }
        else
        {
            for (int i = 0; i < cards; i++)
            {
                //Tilt the card to simulate the way you would with real cards
                float rotationAngle = -5f * (i - (cards - 1) / 2f);
                hand[i].GetComponent<Card>().transform.localRotation = Quaternion.Euler(0f, 1f, rotationAngle);

                //set relative Position to other cards
                float hOffset = handPosition.position.x + cardSpacing * (i - (cards - 1) / 2f)*2;
                float normalizedPos = 2 * (2f * i / (cards - 1) - 1f); //adjust for the arc
                float vOffset = handPosition.position.y + vSpacing * (1 - normalizedPos * normalizedPos)/10*(cards/2);

                hand[i].GetComponent<Card>().transform.position = new Vector3(hOffset, vOffset - 7.5f, 2f - (i / 10f));
                hand[i].GetComponentInChildren<CardButton>().normalZ = 1f + (i / 10f);

            }
        }
    }


}
