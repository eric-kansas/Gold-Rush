using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{

    #region Properties
    /* Game Stage Manager - Defines and controls various stages of the game, including actions within a given turn. */
    public GameStateManager gameState;

    /* ClickHandler - Handles mouse clicks */
    public ClickHandler clicker;

    /* GUI */
    private GuiHandler gui;

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

    /* The number of turns that have passed so far for this phase */
    private int phaseTurns = 0;

    /* Lists for what can be done on the current turn */
    public List<Vector2> moves;										// Locations the current player can move to as a result of their dice roll
    public List<Vector2> possibleStakes = new List<Vector2>();		// Locations the player can stake a claim too (should be at most 5 positions)

    /* Whether or not the player can prospect - depends on whether they rolled the dice, and whether they've actually moved or not*/
    public bool pEnabled = false;

    /* Whether or not the player has staked a claim */
    public bool sEnabled = false;

    public bool[,] checkedList = new bool[BOARD_WIDTH, BOARD_HEIGHT];

    /* Number of turns the player will have in the first round. Also used as the number of stakes the player will have. */
    public int numProspectingTurns = 1;
    public bool cancelStake = false;

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
    void Start()
    {
        gameState = new GameStateManager();
        scoringSystem = new ScoringSystem();
        scoringSystem.selectSystem(new Grouping());
        clicker = this.GetComponent<ClickHandler>();
        gui = this.GetComponent<GuiHandler>();

        if (!CardPrefab)
            Debug.LogError("No card prefab set.");
    }

    #region Update() and OnGUI()
    private void OnGUI()
    {
        if (gameState.CurrentGameState != GameStateManager.GameState.BEFORE_GAME)
        {
            gui.adjustLocation();
            gui.setText();
            gui.handleAction();
            gui.handleSkip();

            gui.playerDisplay();
        }
        else
        {
            gui.ShowMenu();
        }
    }

    // Update is called once per frame
    void Update()
    {
        switch (gameState.CurrentGameState)
        {
            case GameStateManager.GameState.BEFORE_GAME: break;
            case GameStateManager.GameState.GAME_SETUP:
                clicker.myUpdate();
                break;
            case GameStateManager.GameState.GAME_PROSPECTING_STATE:
                handleTurn();
                break;
            case GameStateManager.GameState.GAME_MINING_STATE:
                handleTurn();
                break;
            case GameStateManager.GameState.GAME_END: break;
            default: Debug.Log("whoops"); break;
        }
    }
    #endregion

    # region Calculations

    public void calculateMoveLocations()
    {
        Vector2 currentPlayerPos = players[currentPlayerIndex].Position;
        moves = findMoves(currentPlayerPos);
    }

    /// <summary>
    /// Finds the move locations from a specific board location with a certain roll.
    /// For example: Finding valid moves one space from a mined card to bump other players.
    /// </summary>
    /// <param name="position"></param>
    public void calculateMoveLocations(Vector2 position, int roll)
    {
        currentRoll = roll;
        moves = findMoves(position);
    }

    public void calculateStakes()
    {
        //get the location of the current player based on the grid
        players[CurrentPlayerIndex].Position = GetComponent<ClickHandler>().PositionToVector2(players[CurrentPlayerIndex].transform.position);

        // set the card that the player is currently on
        players[CurrentPlayerIndex].CurrentCard = board[(int)players[CurrentPlayerIndex].Position.x,
                                                    (int)players[CurrentPlayerIndex].Position.y].GetComponent<Card>();

        //return board to default colors
        clearHighlights();

        //prospect - turn the card over
        CreateMaterial(players[currentPlayerIndex].CurrentCard.data.TexCoordinate, board[(int)players[CurrentPlayerIndex].Position.x,
                                                    (int)players[CurrentPlayerIndex].Position.y]);

        calculateStakeableCards();
    }

    public void calculateMines()
    {
        for (int i = 0; i < players[currentPlayerIndex].stakes.Count; i++)
        {
            if (players[currentPlayerIndex].stakedCards[i].data.Minable) //check if card is minable
                players[currentPlayerIndex].stakedCards[i].transform.renderer.material.color = new Color(1, 1, 0);
        }
    }

    private List<Vector2> findMoves(Vector2 position)
    {
        checkedList = new bool[BOARD_WIDTH, BOARD_HEIGHT];
        List<Vector2> holder = new List<Vector2>();

        return findMovesAccumlative(position, 0, checkedList, holder);
    }

    /// <summary>
    /// Recursive function which would find all available moves
    /// </summary>
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

        if (board[(int)currentPos.x, (int)currentPos.y] == null && currentCount != 0) //currentcount != 0 allows us to look for moves FROM empty spaces, but will ignore moves TO empty spaces
        {
            looked[(int)currentPos.x, (int)currentPos.y] = true;
            return holder;
        }
        else
        {

            //set that this one has been checked
            looked[(int)currentPos.x, (int)currentPos.y] = true;

            if (currentCount == currentRoll)
            {
                if (!holder.Contains(new Vector2((int)currentPos.x, (int)currentPos.y)))
                {
                    holder.Add(new Vector2((int)currentPos.x, (int)currentPos.y));
                }

                board[(int)currentPos.x, (int)currentPos.y].transform.renderer.material.color = new Color(1, 1, 0);
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
        possibleStakes.Clear();

        // color the cards that can be staked as yellow
        if (!players[currentPlayerIndex].CurrentCard.data.staked)
            players[currentPlayerIndex].CurrentCard.transform.renderer.material.color = new Color(1, 1, 0);

        // add the card the player is currently on
        possibleStakes.Add(players[currentPlayerIndex].Position);

        // temp variables for current card's row and column
        int currentCardRow = (int)players[currentPlayerIndex].Position.x;
        int currentCardCol = (int)players[currentPlayerIndex].Position.y;

        //check if the player is on the edge
        //below the current card
        if (currentCardRow - 1 >= 0)
        {
            int row = currentCardRow - 1;
            int col = currentCardCol;

            if (board[row, col] != null && !board[row, col].GetComponent<Card>().data.staked)
            {
                Vector2 newV2 = new Vector2(row, col);

                board[row, col].transform.renderer.material.color = new Color(1, 1, 0); // color yellow

                possibleStakes.Add(newV2);
            }
        }

        //above the current card
        if (currentCardRow + 1 <= 12)
        {
            int row = currentCardRow + 1;
            int col = currentCardCol;

            if (board[row, col] != null && !board[row, col].GetComponent<Card>().data.staked)
            {
                Vector2 newV2 = new Vector2(row, col); ;

                board[row, col].transform.renderer.material.color = new Color(1, 1, 0);

                possibleStakes.Add(newV2);
            }
        }

        //left of the current card
        if (currentCardCol - 1 >= 0)
        {
            int row = currentCardRow;
            int col = currentCardCol - 1;

            if (board[row, col] != null && !board[row, col].GetComponent<Card>().data.staked)
            {
                Vector2 newV2 = new Vector2(row, col);

                board[row, col].transform.renderer.material.color = new Color(1, 1, 0);

                possibleStakes.Add(newV2);
            }
        }

        //right of the current card
        if (currentCardCol + 1 <= 3)
        {
            int row = currentCardRow;
            int col = currentCardCol + 1;

            if (board[row, col] != null && !board[row, col].GetComponent<Card>().data.staked)
            {
                Vector2 newV2 = new Vector2(row, col);

                board[row, col].transform.renderer.material.color = new Color(1, 1, 0);

                possibleStakes.Add(newV2);
            }
        }
    }

    public void calculateStakeableCards(Vector2 position)
    {
        possibleStakes.Clear();

        // color the cards that can be staked as yellow
        players[currentPlayerIndex].CurrentCard.transform.renderer.material.color = new Color(1, 1, 0);

        // add the card the player is currently on
        possibleStakes.Add(players[currentPlayerIndex].Position);

        // temp variables for current card's row and column
        int currentCardRow = (int)position.x;
        int currentCardCol = (int)position.y;

        //check if the player is on the edge
        //below the current card
        if (currentCardRow - 1 >= 0)
        {
            int row = currentCardRow - 1;
            int col = currentCardCol;

            if (board[row, col] != null && !board[row, col].GetComponent<Card>().data.staked) // don't highlight if card is unavailable because it is already staked
            {
                Vector2 newV2 = new Vector2(row, col);

                board[row, col].transform.renderer.material.color = new Color(1, 1, 0); // color yellow

                possibleStakes.Add(newV2);
            }
        }

        //above the current card
        if (currentCardRow + 1 <= 12)
        {
            int row = currentCardRow + 1;
            int col = currentCardCol;

            if (board[row, col] != null && !board[row, col].GetComponent<Card>().data.staked)
            {
                Vector2 newV2 = new Vector2(row, col); ;

                board[row, col].transform.renderer.material.color = new Color(1, 1, 0);

                possibleStakes.Add(newV2);
            }
        }

        //left of the current card
        if (currentCardCol - 1 >= 0)
        {
            int row = currentCardRow;
            int col = currentCardCol - 1;

            if (board[row, col] != null && !board[row, col].GetComponent<Card>().data.staked)
            {
                Vector2 newV2 = new Vector2(row, col);

                board[row, col].transform.renderer.material.color = new Color(1, 1, 0);

                possibleStakes.Add(newV2);
            }
        }

        //right of the current card
        if (currentCardCol + 1 <= 3)
        {
            int row = currentCardRow;
            int col = currentCardCol + 1;

            if (board[row, col] != null && !board[row, col].GetComponent<Card>().data.staked)
            {
                Vector2 newV2 = new Vector2(row, col);

                board[row, col].transform.renderer.material.color = new Color(1, 1, 0);

                possibleStakes.Add(newV2);
            }
        }
    }
    #endregion

    #region Actions
    public void setUpBoard()
    {
        //shuffle cards
        //build board
        BuildDeck();
        ShuffleDeck();
        BuildBoard();
        gameState.CurrentGameState = GameStateManager.GameState.GAME_SETUP;
    }

    public void endTurn()
    {
        //return board to default color, remove hightlighting from highlighted cards
        clearHighlights();

        //mark cards as minable again
        for (int i = 0; i < BOARD_WIDTH; i++)
        {
            for (int j = 0; j < BOARD_HEIGHT; j++)
            {
                if (board[i, j] != null)
                    board[i, j].GetComponent<Card>().data.Minable = true;
            }
        }

        //reset temporary stake
        clicker.TempStake = null;
        clicker.selectedCard = false;

        //reset for this turn - the player hasn't moved or placed a stake
        pEnabled = sEnabled = false;
        cancelStake = false;
        clicker.numToMove = 0;
        clicker.indexToMove = -1;
        clicker.stakeIndex = clicker.stakeOwnerIndex = -1;
        clicker.movedStake = false;

        currentPlayerIndex++;	// move to next player
        if (currentPlayerIndex >= players.Count)	//wrap around if necessary
            currentPlayerIndex = 0;

        //increment the turns and check if game state needs to be changed
        phaseTurns++;

        if (phaseTurns >= players.Count * numProspectingTurns)
        {
            gameState.CurrentGameState = GameStateManager.GameState.GAME_MINING_STATE;  //move on to the next game state
            GameStateManager.Instance.CurrentGameState = GameStateManager.GameState.GAME_MINING_STATE;  //move on to the next game state
            phaseTurns = 0;
        }

        //move turn state back to the beginning
        gameState.CurrentTurnState = GameStateManager.TurnState.TURN_ROLL;
    }

    //return board to default color, remove hightlighting from highlighted cards
    public void clearHighlights()
    {
        for (int i = 0; i < BOARD_WIDTH; i++)
        {
            for (int j = 0; j < BOARD_HEIGHT; j++)
            {
                if (board[i, j] != null)
                    board[i, j].transform.renderer.material.color = new Color(1, 1, 1, 1);
            }
        }
    }

    public int Roll()
    {
        int currentRoll = Random.Range(1, 6);
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
                Vector3 pos = new Vector3(.88f * i, .5f, 1.1f * j);
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
                deck[counter].row = i;
                deck[counter].col = j;
                board[i, j].GetComponent<Card>().data = deck[counter];
                counter++;
            }
        }
    }

    public void CreateMaterial(Vector2 Coordinate, GameObject Card)
    {
        Material newMat = new Material(Shader.Find("Diffuse"));
        newMat.mainTexture = CardTexture;
        newMat.mainTextureScale = new Vector2(0.0668f, 0.2f);
        newMat.mainTextureOffset = new Vector2(CARD_TEX_X_OFFSET * -Coordinate.x, CARD_TEX_Y_OFFSET * Coordinate.y);
        Card.renderer.material = newMat;
    }
    #endregion

    private void handleTurn()
    {
        switch (gameState.CurrentTurnState)
        {
            case GameStateManager.TurnState.TURN_ROLL:
                break;
            case GameStateManager.TurnState.TURN_MOVE:
                clicker.myUpdate();
                break;
            case GameStateManager.TurnState.TURN_STAKE:
                clicker.myUpdate();
                break;
            case GameStateManager.TurnState.TURN_MINE:
                clicker.myUpdate();
                break;
            default:
                Debug.Log("whoops");
                break;
        }
    }

}
