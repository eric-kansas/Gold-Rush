using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

    private const int BOARD_WIDTH = 13;
    private const int BOARD_HEIGHT = 4;

    public GameObject Heart;
    public GameObject Diamond;
    public GameObject Club;
    public GameObject Spade;

    private Card[] deck = new Card[52];

    // 0 = 10
    private char[] kinds = {'2','3','4','5','6','7','8','9','0','J', 'Q', 'K', 'A'};
    private int[] values = { 2, 3, 4, 5, 6, 7, 8, 9, 10, 10, 10, 10, 10 };
    private char[] suits= { 'H', 'D', 'C', 'S'};

    private GameObject[,] board = new GameObject[BOARD_WIDTH, BOARD_HEIGHT];

	// Use this for initialization
	void Start () {
        if (!Heart || !Diamond || !Club || !Spade)
            Debug.LogError("No card prefab set.");

        //shuffle cards
        //build board
        BuildDeck();
        ShuffleDeck();
        BuildBoard();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    private void BuildDeck()
    {
        int counter = 0;
        for (int i = 0; i < kinds.Length; i++)
        {
            for (int j = 0; j < suits.Length; j++)
            {
                deck[counter] = new Card(kinds[i], suits[j]);
                counter++;
            }
        }
    }

    private void ShuffleDeck()
    {
        Card temp;
        for (int i = 0; i < deck.Length; i++)
        {
            int randomCard = Random.Range(0, 51);
            temp = deck[i];
            deck[i] = deck[randomCard];
            deck[randomCard] = temp;
        }
    }


    private void BuildBoard()
    {

        int counter = 0;
        for (int i = 0; i < BOARD_WIDTH; i++)
        {
            for (int j = 0; j < BOARD_HEIGHT; j++)
            {
                Vector3 pos = new Vector3(.88f* i , .5f, 1.1f * j);
                switch (deck[counter].Suit)
                {
                    case 'H': 
                        board[i, j] = (GameObject)Instantiate(Heart, pos, Quaternion.identity); 
                        break;
                    case 'D':
                        board[i, j] = (GameObject)Instantiate(Diamond, pos, Quaternion.identity);
                        break;
                    case 'C': 
                        board[i, j] = (GameObject)Instantiate(Club, pos, Quaternion.identity); 
                        break;
                    case 'S': 
                        board[i, j] = (GameObject)Instantiate(Spade, pos, Quaternion.identity); 
                        break;
                }
                counter++;
            }
        }
    }
}
