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
    private CardData[] hand = new CardData[5];
    public List<Vector2> moves;

    public bool pEnabled = false, sEnabled = false;
    private bool showSkipButton = false;
	
	public List<Vector2> possibleStakes = new List<Vector2>();


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
	
	public int getBoardWidth()
	{
		int bW = BOARD_WIDTH;
		
		return bW;
	}
	
	public int getBoardHeight()
	{
		int bH = BOARD_HEIGHT;
		
		return bH;
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

    private void OnGUI()
    {
        //Hand
        for (int i = 0; i < 5; i++)
        {
            Texture2D test = new Texture2D(0,0);
            GUI.Box(new Rect((Screen.width * (i * .06f) + (Screen.width * .1f)), (Screen.height * .82f), 75, 100), "Card " + (i + 1));
        }

        #region Set text
        string actionText = "", skipText = "";
        if (players.Count <= 1)
        {
            showSkipButton = false;
            actionText = "Please place player.";
        }
        else
        {
            switch (gameState.CurrentTurnState)
            {
                case GameStateManager.TurnState.TURN_ROLL:
                    actionText = "Roll";
                    skipText = "Prospect";
                    showSkipButton = true;
                    break;
                case GameStateManager.TurnState.TURN_MOVE:
                    actionText = "Prospect";
                    break;
                case GameStateManager.TurnState.TURN_STAKE:
                    actionText = "Stake";
                    if (gameState.CurrentGameState == GameStateManager.GameState.GAME_MINING_STATE)
                    {
                        showSkipButton = true;
                        skipText = "Mine";
                    }
                    break;
                case GameStateManager.TurnState.TURN_MINE:
                    actionText = "Mine";
                    skipText = "Roll";
                    break;
            }
        }
        #endregion

        #region Action logic
        if (GUI.Button(new Rect((Screen.width * .8f), (Screen.height * .82f), 150, 75), actionText))
        {
            if (players.Count <= 1)
                return;
            switch (gameState.CurrentTurnState)
            {
                case GameStateManager.TurnState.TURN_ROLL:

                    currentRoll = Roll();
                    Debug.Log("rolled: " + currentRoll);

                    calculateMoveLocations();
                    gameState.CurrentGameState = GameStateManager.GameState.GAME_PROSPECTING_STATE;
                    gameState.CurrentTurnState = GameStateManager.TurnState.TURN_MOVE;

                    pEnabled = sEnabled = false; //reset these variables for this turn


                    // At the beginning, this bool is true - player can stay where he/she is by choosing not to roll. 
                    // Once the player rolls, he must move, so set this bool to false.
                    showSkipButton = false;
                    break;
                case GameStateManager.TurnState.TURN_MOVE:
					GameStateManager.Instance.CurrentTurnState = GameStateManager.TurnState.TURN_STAKE;		
								
					players[CurrentPlayerIndex].Position = GetComponent<ClickHandler>().PositionToVector2(players[CurrentPlayerIndex].transform.position);
				
					players[CurrentPlayerIndex].CurrentCard = board[(int)players[CurrentPlayerIndex].Position.x,
				                                                (int)players[CurrentPlayerIndex].Position.y].GetComponent<Card>();
					
					for (int i = 0; i < BOARD_WIDTH; i++)
			        {
			            for (int j = 0; j < BOARD_HEIGHT; j++)
			            {
							 board[i, j].transform.renderer.material.color = new Color(1,1,1,1);
						}
					}
					
					Debug.Log("Calc Stakes");
				
					calculateStakeableCards();
                    break;
                case GameStateManager.TurnState.TURN_STAKE:
                    Debug.Log("turn state: TURN_STAKE");
                    clicker.myUpdate();
                    if (gameState.CurrentGameState == GameStateManager.GameState.GAME_MINING_STATE)
                        gameState.CurrentTurnState = GameStateManager.TurnState.TURN_MINE;
                    else 
                        endTurn();

                    //the player can choose not to pick up a card this turn if he/she wants
                    showSkipButton = true;
                    break;
                case GameStateManager.TurnState.TURN_MINE:
                    Debug.Log("turn state: TURN_MINE");
                    break;
                default: Debug.Log("whoops"); break;
            }
        }
        #endregion

        #region Skip-action logic
        if (showSkipButton) //only show skip button if player can choose not to do this action
        {
            if (GUI.Button(new Rect(new Rect((Screen.width * .85f), (Screen.height * .935f), 70, 20)), skipText))
            {
                switch (gameState.CurrentTurnState)
                {
                    case GameStateManager.TurnState.TURN_ROLL:
                        gameState.CurrentTurnState = GameStateManager.TurnState.TURN_STAKE;
                        pEnabled = sEnabled = false;
                        showSkipButton = false;
                        break;
                    case GameStateManager.TurnState.TURN_STAKE:
                        gameState.CurrentTurnState = GameStateManager.TurnState.TURN_MINE;
                        break;
                    case GameStateManager.TurnState.TURN_MINE:
                        endTurn();
                        break;
                }
            }
        }
        #endregion
    }

    public void endTurn()
    {
		
		for (int i = 0; i < BOARD_WIDTH; i++)
	    {
            for (int j = 0; j < BOARD_HEIGHT; j++)
            {
				 board[i, j].transform.renderer.material.color = new Color(1,1,1,1);
			}
		}
		
        currentPlayerIndex++;
        if (currentPlayerIndex >= players.Count)
            currentPlayerIndex = 0;
        gameState.CurrentTurnState = GameStateManager.TurnState.TURN_ROLL;
    }

    // Update is called once per frame
    void Update()
    {
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
                
                board[(int)currentPos.x, (int)currentPos.y].transform.renderer.material.color = new Color(2,2,0);
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
	
	public void calculateStakeableCards()
	{
		players[currentPlayerIndex].CurrentCard.transform.renderer.material.color = new Color(2,2,0);
		
		possibleStakes.Add(players[currentPlayerIndex].Position);
		
		int currentCardRow = (int)players[currentPlayerIndex].Position.x;
		int currentCardCol = (int)players[currentPlayerIndex].Position.y;
		
//		Debug.Log(players[currentPlayerIndex].CurrentCard.GetComponent<Card>().data.row + ", " 
//		          + players[currentPlayerIndex].CurrentCard.GetComponent<Card>().data.col);
		
		
		if(currentCardRow - 1 >= 0)
		{
			int row = currentCardRow - 1;
			int col = currentCardCol;
			
			Vector2 newV2 = new Vector2(row, col);
			
			board[row, col].transform.renderer.material.color = new Color(2,2,0);
			
			possibleStakes.Add(newV2);
		}
		
		if(currentCardRow + 1 <= 12)
		{
			int row = currentCardRow + 1;
			int col = currentCardCol;
			
			Vector2 newV2 = new Vector2(row, col);;
			
			board[row, col].transform.renderer.material.color = new Color(2,2,0);
			
			possibleStakes.Add(newV2);
		}
		
		if(currentCardCol - 1 >= 0)
		{
			int row = currentCardRow;
			int col = currentCardCol - 1;
			
			Vector2 newV2 = new Vector2(row, col);
			
			board[row, col].transform.renderer.material.color = new Color(2,2,0);
			
			possibleStakes.Add(newV2);
		}
		
		if(currentCardCol + 1 <= 3)
		{
			int row = currentCardRow;
			int col = currentCardCol + 1;
			
			Vector2 newV2 = new Vector2(row, col);
			
			board[row, col].transform.renderer.material.color = new Color(2,2,0);
			
			possibleStakes.Add(newV2);
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
