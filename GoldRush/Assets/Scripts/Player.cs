using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{

	/* Player's board position */
	private Vector2 position = new Vector2();
	public Vector2 Position
	{
		get { return position; }
		set { position = value; }
	}

	/* List of stake prefabs */
	public List<GameObject> stakes;

	/* List of staked cards */
	public List<Card> stakedCards;

	/* List of mined cards */
	public List<Card> hand;

	/* The card the player is currently on */
	private Card currentCard;
	public Card CurrentCard
	{
		get { return currentCard; }
		set { currentCard = value; }
	}

	//database id
	private int id;
	public int ID
	{
		get { return id; }
		set { id = value; }
	}



	//Index in players array
	public int in_game_id; //(for sorting)

	// Use this for initialization
	void Start()
	{
		hand = new List<Card>();
		stakedCards = new List<Card>();
		stakes = new List<GameObject>();
	}

	// Update is called once per frame
	void Update()
	{

	}

	public void loadPlayerFromJson()
	{

	}
}
