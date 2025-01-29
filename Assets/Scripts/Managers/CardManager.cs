using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

public class CardManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform handPosition;

    public List<GameObject> deck = new List<GameObject>();
    public List<GameObject> hand = new List<GameObject>();
    public List<GameObject> discard = new List<GameObject>();


    float cardSpacing = 50f;
    float vSpacing = 10f;


    private void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject tempCard = Instantiate(cardPrefab, handPosition.position,Quaternion.identity);
            tempCard.SetActive(false);
            deck.Add(tempCard);
        }
    }

    public void drawCard()
    {
        if (deck.Count > 0)
        {
            hand.Add(deck[0]);
            deck.RemoveAt(0);
            hand[hand.Count-1].SetActive(true);
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
        int n = deck.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0,n + 1);
            GameObject temp = deck[k];
            deck[k] = deck[n];
            deck[n] = temp;
        }
    }

    public void removeFromHand(int a)
    {
        if (hand.Count > a)
        {
            addToDiscard(getCardInHandID(a));
            hand.RemoveAt(a);
        }
    }

    public GameObject getCardInHandID(int pos)
    {
        GameObject cardID = hand[pos];
        return cardID;
    }

    public void addToDiscard(GameObject card)
    {
        discard.Add(card);
    }


    public void updateHandVisuals()
    {
        int cards = hand.Count;

        if (cards == 0)
        {
            //float hOffset = handPosition.position.x + cardSpacing * (i - (cards - 1) / 2f);
           // hand[0].transform.localPosition = new Vector3(hOffset, 0f, 0f);
        }
        else
        {
            for (int i = 0; i < cards; i++)
            {
                float rotationAngle = -5f * (i - (cards - 1) / 2f);
                hand[i].transform.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);

                float hOffset = handPosition.position.x + cardSpacing * (i - (cards - 1) / 2f);

                float normalizedPos = 2 * (2f * i / (cards - 1) - 1f); //adjust for the arc
                float vOffset = handPosition.position.y + vSpacing * (1 - normalizedPos * normalizedPos);

                //set Position
                hand[i].transform.localPosition = new Vector3(hOffset, vOffset, 0f);


                //float handWidth = (hand.Count - 1 - i * 2) * cardWidth * gap;
                //float position = handCenterPos - (handWidth / 2);
                //hand[i].transform
            }
        }
    }
}
