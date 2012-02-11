using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {
   
    private const int BOARD_WIDTH = 13;
    private const int BOARD_HEIGHT = 4;
	
	private int currentPlayerIndex = 0;
    private int currentRoll = -1;

	public int maxPlayers = 2;
    private GameStateManager gameState;
    private ClickHandler clicker;
	
    public GameObject CardPrefab;
	public ScoringSystem scoringSystem;

    private CardData[] deck = new CardData[52];
    public List<Vector2> moves;


    // 0 = 10
    private char[] kinds = {'A','2','3','4','5','6','7','8','9','0','J', 'Q', 'K'};
    private int[] values = {11, 2, 3, 4, 5, 6, 7, 8, 9, 10, 10, 10, 10 };
    private char[] suits= { 'C', 'D', 'H', 'S'};
	
	public List<Player> players;

    public GameObject[,] board = new GameObject[BOARD_WIDTH, BOARD_HEIGHT];
    public bool[,] checkedList = new bool[BOARD_WIDTH, BOARD_HEIGHT];

    public int CurrentPlayerIndex
	{
        get { return currentPlayerIndex; }
        set { currentPlayerIndex = value; }
	}

    public int CurrentRoll
    {
        get { return currentRoll; }
        set { currentRoll = value; }
    }
	
	// Use this for initialization
	void Start () {
        gameState = new GameStateManager();
		scoringSystem = new ScoringSystem();
		scoringSystem.selectSystem(new Grouping());
        clicker = this.GetComponent<ClickHandler>();
		
        if (!CardPrefab)
            Debug.LogError("No card prefab set.");

        //shuffle cards
        //build board
        BuildDeck();
        ShuffleDeck();
        BuildBoard();
	}
	
	// Update is called once per frame
	void Update () {
        switch (gameState.CurrentGameState)
        {
            case GameStateManager.GameState.GAME_SETUP: 
                clicker.myUpdate();    
                break;
            case GameStateManager.GameState.GAME_PROSPECTING_STATE:
                prospectingTurn();
                break;
            case GameStateManager.GameState.GAME_MINING_STATE: Debug.Log("game state: MINING"); break;
            case GameStateManager.GameState.GAME_END: Debug.Log("game state: END"); break;
            default: Debug.Log("whoops"); break;
        }
	}

    private void prospectingTurn()
    {
        switch (gameState.CurrentTurnState)
        {
            case GameStateManager.TurnState.TURN_ROLL:
                currentRoll = Roll();
                Debug.Log("rolled: " + currentRoll);

                calculateMoveLocations();
                gameState.CurrentTurnState = GameStateManager.TurnState.TURN_MOVE;
                break;
            case GameStateManager.TurnState.TURN_MOVE:
                clicker.myUpdate();
                break;
            case GameStateManager.TurnState.TURN_STAKE:
                Debug.Log("turn state: TURN_STAKE");
                clicker.myUpdate();
                break;
            case GameStateManager.TurnState.TURN_MINE:
                Debug.Log("turn state: TURN_MINE"); 
                break;
            default: Debug.Log("whoops"); break;
        }
    }

    private void calculateMoveLocations()
    {
        Vector2 currentPlayerPos = players[currentPlayerIndex].Position;
        moves = findMoves(currentPlayerPos);

    }

    private List<Vector2> findMoves(Vector2 currentPlayerPos)
    {
        checkedList = new bool[BOARD_WIDTH, BOARD_HEIGHT];
        List<Vector2> holder = new List<Vector2>();

        return findMovesAccumlative(currentPlayerPos, 0, checkedList, holder);
    }

    private List<Vector2> findMovesAccumlative(Vector2 currentPos, int currentCount, bool[,] looked, List<Vector2> holder)
    {
        //out of bounds
        if (currentPos.x < 0 || currentPos.x > 12 ||
            currentPos.y < 0 || currentPos.y > 3)
        {
            return holder;
        }
        //if we have been here
        if (looked[(int)currentPos.x, (int)currentPos.y])
        {
            return holder;
        }
        else
        {
            
            //set that this one has been checked
            looked[(int)currentPos.x, (int)currentPos.y] = true;

            if (currentCount == currentRoll && board[(int)currentPos.x, (int)currentPos.y] != null)
            {
                if(!holder.Contains(new Vector2((int)currentPos.x, (int)currentPos.y)))
                {
                    holder.Add(new Vector2((int)currentPos.x, (int)currentPos.y));
                }
                
                board[(int)currentPos.x, (int)currentPos.y].transform.renderer.material.color = new Color(255,0,0);
                looked[(int)currentPos.x, (int)currentPos.y] = false;
                return holder;
            }
            else
            {
                currentCount++;
                //look up
                holder = findMovesAccumlative(new Vector2(currentPos.x, currentPos.y + 1), currentCount, looked, holder);
                //look right
                holder = findMovesAccumlative(new Vector2(currentPos.x + 1, currentPos.y), currentCount, looked, holder);
                //look left
                holder = findMovesAccumlative(new Vector2(currentPos.x - 1, currentPos.y), currentCount, looked, holder);
                //look down
                holder = findMovesAccumlative(new Vector2(currentPos.x, currentPos.y - 1), currentCount, looked, holder);
                looked[(int)currentPos.x, (int)currentPos.y] = false;
                return holder;
            }
        }
    }


    private int Roll(){
        return Random.Range(1, 6);
    }

    private void BuildDeck()
    {
        int counter = 0;
        for (int i = 0; i < kinds.Length; i++)
        {
            for (int j = 0; j < suits.Length; j++)
            {
                deck[counter] = new CardData(kinds[i], suits[j], values[i]);
                counter++;
            }
        }
    }

    private void ShuffleDeck()
    {
        CardData temp;
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
                        board[i, j] = (GameObject)Instantiate(CardPrefab, pos, Quaternion.identity);
                        break;
                    case 'D':
                        board[i, j] = (GameObject)Instantiate(CardPrefab, pos, Quaternion.identity);
                        break;
                    case 'C':
                        board[i, j] = (GameObject)Instantiate(CardPrefab, pos, Quaternion.identity);
                    break;
                    case 'S':
                        board[i, j] = (GameObject)Instantiate(CardPrefab, pos, Quaternion.identity);
                        break;
                }
                board[i, j].GetComponent<Card>().data = deck[counter];
                counter++;
            }
        }
    }
}
