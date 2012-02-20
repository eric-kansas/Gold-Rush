using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {

    /* Player's board position */
    private Vector2 position = new Vector2();


    /* List of stake prefabs */
    public List<GameObject> stakes;

    /* List of staked cards */
    public List<Card> stakedCards;

    public List<Card> hand;
	private Card currentCard;

    public Vector2 Position
    {
        get { return position; }
        set { position = value; }
    }

    public Card CurrentCard
    {
        get { return currentCard; }
        set { currentCard = value; }
    }

	// Use this for initialization
	void Start () {
        hand = new List<Card>();
        stakedCards = new List<Card>();
        stakes = new List<GameObject>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
