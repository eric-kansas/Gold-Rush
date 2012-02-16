using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {

    private Vector2 position = new Vector2();
    public Card[] hand;
    public List<GameObject> stakes;
    public List<Card> stakedCards;
	private int numStakes = 5;
	private Card currentCard;

    public Vector2 Position
    {
        get { return position; }
        set { position = value; }
    }

	// Use this for initialization
	void Start () {
        hand = new Card[5];
	}
	
	public int NumStakes
    {
        get { return numStakes; }
        set { numStakes = value; }
    }
	
	public Card CurrentCard
	{
		get { return currentCard; }
        set { currentCard = value; }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
