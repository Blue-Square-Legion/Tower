using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public List<Card> deck = new List<Card>();
    public List<Card> hand = new List<Card>();
    public List<Card> graveyard = new List<Card>();



    public void drawCard()
    {
        if (deck.Count > 0)
        {
            hand.Add(deck[0]);
            deck.RemoveAt(0);
        }
        else
        {
            addGraveyardIntoDeck();

            if (deck.Count > 0)
            {
                hand.Add(deck[0]);
                deck.RemoveAt(0);
            }
        }
    }

    public void addGraveyardIntoDeck()
    {
        while (graveyard.Count > 0)
        {
            deck.Add(graveyard[0]);
            graveyard.RemoveAt(0);
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
            Card temp = deck[k];
            deck[k] = deck[n];
            deck[n] = temp;
        }
    }

    public void removeFromHand(int a)
    {
        if (hand.Count > a)
        {
            addToGraveyard(getCardInHandID(a));
            hand.RemoveAt(a);
        }
    }

    public Card getCardInHandID(int pos)
    {
        Card cardID = hand[pos];
        return cardID;
    }

    public void addToGraveyard(Card card)
    {
        graveyard.Add(card);
    }

}
