using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class CardManager : MonoBehaviour
{
    public GameObject cardPrefab;
    public Transform handPosition;

    public List<GameObject> deck = new List<GameObject>();
    public List<GameObject> hand = new List<GameObject>();
    public List<GameObject> discard = new List<GameObject>();

    public GameObject Camera;

    float cardSpacing = 1f;
    float vSpacing = 1f;


    private void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            GameObject tempCard = Instantiate(cardPrefab, handPosition.position,Quaternion.identity);
            tempCard.SetActive(false);
            tempCard.GetComponent<Card>().cost.text = "" + i;
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

            float hOffset = 1;

            float normalizedPos = 2;
            float vOffset = handPosition.position.y + vSpacing * (1 - normalizedPos * normalizedPos);

            //set Position
            hand[0].GetComponent<Card>().position.transform.localPosition = new Vector3(hOffset, vOffset, 0f);
        }
        else
        {
            for (int i = 0; i < cards; i++)
            {
                float rotationAngle = -5f * (i - (cards - 1) / 2f);
                hand[i].GetComponent<Card>().position.transform.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);

                float hOffset = handPosition.position.x + cardSpacing * (i - (cards - 1) / 2f);

                float normalizedPos = 2 * (2f * i / (cards - 1) - 1f); //adjust for the arc
                float vOffset = handPosition.position.y + vSpacing * (1 - normalizedPos * normalizedPos);

                //set Position
                hand[i].GetComponent<Card>().position.transform.position = new Vector3(hOffset, vOffset, 0f);
                //Camera.transform.position.x
                hand[i].GetComponent<Card>().position.transform.position += Camera.transform.position + Camera.transform.forward * 10f;
                var n = Camera.transform.position - hand[i].GetComponent<Card>().position.transform.position;
                //hand[i].GetComponent<Card>().position.transform.rotation = Quaternion.LookRotation(n);

            }
        }
    }
}
