using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{

	#region Properties
	/* Game Stage Manager - Defines and controls various stages of the game, including actions within a given turn. */
	private GameStateManager gameState;

	/* ClickHandler - Handles mouse clicks */
	private ClickHandler clicker;

	/* ScoringSystem - Used to count up the score for mined cards */
	public ScoringSystem scoringSystem;

	/* The game "board" - represents 52 cards laid out in a 13x4 pattern */
	private const int BOARD_WIDTH = 13;
    private const int BOARD_HEIGHT = 4;
	public GameObject[,] board = new GameObject[BOARD_WIDTH, BOARD_HEIGHT];

	/* Represents a common deck of 52 cards */
	private CardData[] deck = new CardData[52];

	/* Card properties */
	private char[] kinds = { 'A', '2', '3', '4', '5', '6', '7', '8', '9', '0', 'J', 'Q', 'K' };
	private int[] values = { 11, 2, 3, 4, 5, 6, 7, 8, 9, 10, 10, 10, 10 };
	private char[] suits = { 'C', 'D', 'H', 'S' };
	public GameObject CardPrefab;									// Card asset
    public Texture2D CardTexture;									// Used in giving individual cards the correct images
    private const float CARD_TEX_X_OFFSET = 0.0687f;				// X offset changes the card value
    private const float CARD_TEX_Y_OFFSET = 0.2f;					// y value changes the suit

	/* Amount of players */
	public List<Player> players;
	public int maxPlayers = 4;										// How many players can be in the game
	private int currentPlayerIndex = 0;								// Index of the current player

	/* The dice roll the player rolled */
    private int currentRoll = -1;

	/* Lists for what can be done on the current turn */
    public List<Vector2> moves;										// Locations the current player can move to as a result of their dice roll
	public List<Vector2> possibleStakes = new List<Vector2>();		// Locations the player can stake a claim too (should be at most 5 positions)

	/* Whether or not the player can prospect - depends on whether they rolled the dice, and whether they've actually moved or not*/
    public bool pEnabled = false;

	/* Whether or not the player has staked a claim */
	public bool sEnabled = false;

	/* Whether or not the current action can be skipped */
    private bool showSkipButton = false;

    public bool[,] checkedList = new bool[BOARD_WIDTH, BOARD_HEIGHT];

	#endregion

	#region Accessors / Mutators
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

	#endregion

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

	#region Update() and OnGUI()
	private void OnGUI()
    {
        //Hand
        for (int i = 0; i < 5; i++)
        {
            Texture2D test = new Texture2D(0,0);
            GUI.Box(new Rect((Screen.width * (i * .06f) + (Screen.width * .1f)), (Screen.height * .82f), 75, 100), "Card " + (i + 1));
        }

		//set the text fields on the buttons
        #region Set text
        string actionText = "", skipText = "";
        if (players.Count <= 1)
        {
            showSkipButton = false;
            actionText = "Please place players.";		// If not enough player objects have been placed, let the players know.
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
                        showSkipButton = true;		// only once all a player's stakes have been placed and it goes into the mining phase,
                        skipText = "Mine";			// players can skip the option to move their stakes around
                    }
                    break;
                case GameStateManager.TurnState.TURN_MINE:
                    actionText = "Mine";
                    skipText = "Roll";
					showSkipButton = true;
                    break;
            }
        }
        #endregion

		// This section controls what clicking on the Action button (larger button will do)
        #region Action logic
		//actually create a rectangle object so the skip button can be based off the same location
		Rect actionBox = new Rect((Screen.width * .8f), (Screen.height * .82f), 150, 75);
        if (GUI.Button(actionBox, actionText))
        {
            if (players.Count <= 1) //if there aren't enough players, the button should not do anything
                return;

			//start the game if there are enough players and a button is hit
			if (gameState.CurrentGameState == GameStateManager.GameState.GAME_SETUP)
				gameState.CurrentGameState = GameStateManager.GameState.GAME_PROSPECTING_STATE;


            switch (gameState.CurrentTurnState)
            {
				case GameStateManager.TurnState.TURN_ROLL:		// The player is choosing to roll
                    
					currentRoll = Roll();		// roll the dice
                    calculateMoveLocations();	// calculate where the player can move as a result of the dice roll

                    pEnabled = sEnabled = false; //reset these variables for this turn

                    // At the beginning, this bool is true - player can stay where he/she is by choosing not to roll. 
                    // Once the player rolls, he must move, so set this bool to false.
                    showSkipButton = false;

					gameState.CurrentTurnState = GameStateManager.TurnState.TURN_MOVE; //move on to the next turn state
                    break;
                case GameStateManager.TurnState.TURN_MOVE:	// the player is moving after rolling the dice
					
					if (pEnabled) //make sure the player has actually moved - he/she cannot sit on the same spot after rolling the dice
					{			//pEnabled is set to true in clickHandler's moveClick()

						calculateStakes(); // based on where the player has moved to, find the adjacent positions he/she can stake a claim

						GameStateManager.Instance.CurrentTurnState = GameStateManager.TurnState.TURN_STAKE;  //move on to the next turn state
					}
					else
						Debug.Log("Player rolled, they need to actually move");

                    break;
                case GameStateManager.TurnState.TURN_STAKE: //player is staking a claim after placing a marker

					if (sEnabled) // make sure the player has actually placed a marker
					{ //sEnabled is set to true in clickHandler's stakeClick()

						// this can probably be deleted, since this handles clicks on the card, not on the buttons...
						//clicker.myUpdate();

						// Either move on or let the next player go
						if (gameState.CurrentGameState == GameStateManager.GameState.GAME_MINING_STATE)
						{
							gameState.CurrentTurnState = GameStateManager.TurnState.TURN_MINE;
							showSkipButton = true;		//the player can choose not to pick up a card this turn if he/she wants
						} else
							endTurn();
					}
					else
						Debug.Log("Pressed with no stake placed");
                    break;
                case GameStateManager.TurnState.TURN_MINE:
                    Debug.Log("turn state: TURN_MINE");
					endTurn();
                    break;
                default: Debug.Log("whoops"); break;
            }
        }
        #endregion

        #region Skip-action logic
		//start the game if there are enough players and a button is hit
		if (players.Count > 1 && gameState.CurrentGameState == GameStateManager.GameState.GAME_SETUP)
			gameState.CurrentGameState = GameStateManager.GameState.GAME_PROSPECTING_STATE;

		float width = 70;
        if (showSkipButton) //only show skip button if player can choose not to do this action
        {
            if (GUI.Button(new Rect(new Rect((actionBox.x + actionBox.width - width), (actionBox.y+actionBox.height), width, 20)), skipText))
            {
                switch (gameState.CurrentTurnState)
                {
                    case GameStateManager.TurnState.TURN_ROLL:		// the player is choosing to stay where they are and not roll the dice this turn

                        pEnabled = true; //player is skipping rolling, meaning they can prospect without moving
						sEnabled = false; //reset stake boolean, player still needs to do this

						calculateStakes(); // based on where the player has moved to, find the adjacent positions he/she can stake a claim

						gameState.CurrentTurnState = GameStateManager.TurnState.TURN_STAKE; // move on to the next turn state
                        break;
                    case GameStateManager.TurnState.TURN_STAKE: //player is not moving his stakes his turn (option is available in mining phase only)
                        gameState.CurrentTurnState = GameStateManager.TurnState.TURN_MINE;
                        break;
                    case GameStateManager.TurnState.TURN_MINE:
                        
						endTurn(); //after the player takes a card, the turn ends
                        break;
                }
            }
        }
        #endregion
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
            case GameStateManager.GameState.GAME_MINING_STATE: break;
            case GameStateManager.GameState.GAME_END: break;
            default: Debug.Log("whoops"); break;
        }
	}
	#endregion

	# region Calculations
	private void calculateMoveLocations()
    {
        Vector2 currentPlayerPos = players[currentPlayerIndex].Position;
        moves = findMoves(currentPlayerPos);

    }

	private void calculateStakes()
	{
		players[CurrentPlayerIndex].Position = GetComponent<ClickHandler>().PositionToVector2(players[CurrentPlayerIndex].transform.position);

		players[CurrentPlayerIndex].CurrentCard = board[(int)players[CurrentPlayerIndex].Position.x,
													(int)players[CurrentPlayerIndex].Position.y].GetComponent<Card>();


		for (int i = 0; i < BOARD_WIDTH; i++)
		{
			for (int j = 0; j < BOARD_HEIGHT; j++)
			{
				board[i, j].transform.renderer.material.color = new Color(1, 1, 1, 1);
			}
		}
		CreateMaterial(players[currentPlayerIndex].CurrentCard.data.TexCoordinate, board[(int)players[CurrentPlayerIndex].Position.x,
													(int)players[CurrentPlayerIndex].Position.y]);


		Debug.Log("Calc Stakes");

		calculateStakeableCards();
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
	#endregion

	#region Actions
	public void endTurn()
	{
		//return board to default color, remove hightlighting from highlighted cards
		for (int i = 0; i < BOARD_WIDTH; i++)
		{
			for (int j = 0; j < BOARD_HEIGHT; j++)
			{
				board[i, j].transform.renderer.material.color = new Color(1, 1, 1, 1);
			}
		}

		currentPlayerIndex++;	// move to next player
		if (currentPlayerIndex >= players.Count)	//wrap around if necessary
			currentPlayerIndex = 0;

		//move turn state back to the beginning
		gameState.CurrentTurnState = GameStateManager.TurnState.TURN_ROLL;
	}

	private int Roll(){
		int currentRoll = Random.Range(1, 6);
		Debug.Log("rolled: " + currentRoll);
		return currentRoll;
    }
	#endregion

	#region setup
	private void BuildDeck()
    {
        int counter = 0;
        for (int i = 0; i < kinds.Length; i++)
        {
            for (int j = 0; j < suits.Length; j++)
            {
                deck[counter] = new CardData(kinds[i], suits[j], values[i], new Vector2(i + 1, j + 1));
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

    public void CreateMaterial(Vector2 Coordinate, GameObject Card)
    {
        int aX = (int)Coordinate.x;
        int aY = (int)Coordinate.y;

        Debug.Log("material coord: " + Coordinate.x + ", " + Coordinate.y);
        Debug.Log(CARD_TEX_X_OFFSET * Coordinate.x);

        Material newMat = new Material(Shader.Find("Diffuse"));
        newMat.mainTexture = CardTexture;
        newMat.mainTextureScale = new Vector2(0.0668f, 0.2f);
        newMat.mainTextureOffset = new Vector2(CARD_TEX_X_OFFSET * -Coordinate.x, CARD_TEX_Y_OFFSET * Coordinate.y);
        Card.renderer.material = newMat;
	}
	#endregion


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
				//Debug.Log("turn state: TURN_STAKE");
				clicker.myUpdate();
				break;
			case GameStateManager.TurnState.TURN_MINE:
				//Debug.Log("turn state: TURN_MINE"); 
				break;
			default:
				Debug.Log("whoops");
				break;
		}
	}
}
