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
    private FeedbackGUI f_gui;

    /* Reference to JSON objects */
    public JsonFxScript jsonFx;

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
    public bool[,] islandList = new bool[BOARD_WIDTH, BOARD_HEIGHT];

    /* Number of turns the player will have in the first round. Also used as the number of stakes the player will have. */
    public int numProspectingTurns = 1;
    public bool cancelStake = false;

    private bool lastTurn = false;

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
        f_gui = gameObject.GetComponent<FeedbackGUI>();
        clicker = this.GetComponent<ClickHandler>();
        gui = this.GetComponent<GuiHandler>();
        jsonFx = this.GetComponent<JsonFxScript>();

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
            case GameStateManager.GameState.GAME_END:
                break;
            default: Debug.Log("whoops"); break;
        }
    }

    //If a player does not have a full hand by the end of the game, add their staked cards to their hand
    
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

        islandList = new bool[BOARD_WIDTH, BOARD_HEIGHT];
        checkedList = new bool[BOARD_WIDTH, BOARD_HEIGHT];

        checkIsland(position, checkedList);

        List<Vector2> holder = new List<Vector2>();

        holder = addIslandMoves(position, holder);
        checkedList = new bool[BOARD_WIDTH, BOARD_HEIGHT];
        return findMovesAccumlative(position, 0, checkedList, holder);
    }

    private List<Vector2> addIslandMoves(Vector2 currentPos, List<Vector2> holder)
    {
        for (int i = 0; i < 4; i++)
        {
            switch (i)
            {
                //up
                case 0:
                    holder = addIslandMove(new Vector2(currentPos.x, currentPos.y + 1), holder, i);
                    break;
                case 1://right
                    holder = addIslandMove(new Vector2(currentPos.x + 1, currentPos.y), holder, i);
                    break;
                case 2://left
                    holder = addIslandMove(new Vector2(currentPos.x - 1, currentPos.y), holder, i);
                    break;
                case 3://down
                    holder = addIslandMove(new Vector2(currentPos.x, currentPos.y - 1), holder, i);
                    break;
            }
        }

            Debug.Log(holder.Count);
        
        
        return holder;
    }

    private List<Vector2> addIslandMove(Vector2 currentPos, List<Vector2> holder, int direction)
    {
        //out of bounds
        if (currentPos.x < 0 || currentPos.x > 12 ||
            currentPos.y < 0 || currentPos.y > 3)
        {
            return holder;
        }
        //which direction are we looking
        switch(direction)
        {
            //up
            case 0:
                if (board[(int)currentPos.x, (int)currentPos.y] == null)
                {//if we are still in the ocean
                    addIslandMove(new Vector2(currentPos.x, currentPos.y + 1), holder, direction);
                }
                else
                {//hit land
                    if (!islandList[(int)currentPos.x, (int)currentPos.y])
                    {//if this is not the same island as we are on add it
                        Debug.Log("HJHERHERHEHREH: added a island spot");
                        holder.Add(new Vector2((int)currentPos.x, (int)currentPos.y));
                        board[(int)currentPos.x, (int)currentPos.y].transform.renderer.material.color = new Color(1, 1, 0);
                    }
                }
                break;
            case 1://right
                if (board[(int)currentPos.x, (int)currentPos.y] == null)
                {//if we are still in the ocean
                    addIslandMove(new Vector2(currentPos.x + 1, currentPos.y), holder, direction);
                }
                else
                {//hit land
                    if (!islandList[(int)currentPos.x, (int)currentPos.y])
                    {//if this is not the same island as we are on add it
                        Debug.Log("HJHERHERHEHREH: added a island spot");
                        holder.Add(new Vector2((int)currentPos.x, (int)currentPos.y));
                        board[(int)currentPos.x, (int)currentPos.y].transform.renderer.material.color = new Color(1, 1, 0);
                    }
                }
                break;
            case 2://left
                if (board[(int)currentPos.x, (int)currentPos.y] == null)
                {//if we are still in the ocean
                    addIslandMove(new Vector2(currentPos.x - 1, currentPos.y), holder, direction);
                }
                else
                {//hit land
                    if (!islandList[(int)currentPos.x, (int)currentPos.y])
                    {//if this is not the same island as we are on add it
                        Debug.Log("HJHERHERHEHREH: added a island spot");
                        holder.Add(new Vector2((int)currentPos.x, (int)currentPos.y));
                        board[(int)currentPos.x, (int)currentPos.y].transform.renderer.material.color = new Color(1, 1, 0);
                    }
                }
                break;
            case 3://down
                if (board[(int)currentPos.x, (int)currentPos.y] == null)
                {//if we are still in the ocean
                    addIslandMove(new Vector2(currentPos.x, currentPos.y - 1), holder, direction);
                }
                else
                {//hit land
                    if (!islandList[(int)currentPos.x, (int)currentPos.y])
                    {//if this is not the same island as we are on add it
                        Debug.Log("HJHERHERHEHREH: added a island spot");
                        holder.Add(new Vector2((int)currentPos.x, (int)currentPos.y));
                        board[(int)currentPos.x, (int)currentPos.y].transform.renderer.material.color = new Color(1, 1, 0);
                    }
                }
                break;
        }
        return holder;
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

    private void checkIsland(Vector2 currentPos, bool[,] looked)
    {
        //out of bounds
        if (currentPos.x < 0 || currentPos.x > 12 ||
            currentPos.y < 0 || currentPos.y > 3)
        {
            return;
        }
        //if we have been here
        if (looked[(int)currentPos.x, (int)currentPos.y])
        {
            return;
        }

        if (board[(int)currentPos.x, (int)currentPos.y] == null) //currentcount != 0 allows us to look for moves FROM empty spaces, but will ignore moves TO empty spaces
        {
            looked[(int)currentPos.x, (int)currentPos.y] = true;
            Debug.Log("x: " + currentPos.x + " y: " + currentPos.y + ", " +islandList[(int)currentPos.x, (int)currentPos.y]);
            return;
        }
        else
        {
            islandList[(int)currentPos.x, (int)currentPos.y] = true;
            looked[(int)currentPos.x, (int)currentPos.y] = true;
        }

        //look up
        checkIsland(new Vector2(currentPos.x, currentPos.y + 1), looked);
        //look right
        checkIsland(new Vector2(currentPos.x+1, currentPos.y), looked);
        //look left
        checkIsland(new Vector2(currentPos.x - 1, currentPos.y), looked);
        //look down
        checkIsland(new Vector2(currentPos.x, currentPos.y - 1), looked);
               
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

    public Vector2 getTexCoordinates(char kind, char suit)
    {
        Vector2 result = Vector2.zero;
        switch (suit)
        {
            case 'C':
                result.y = 1;
                break;
            case 'D':
                result.y = 2;
                break;
            case 'H':
                result.y = 3;
                break;
            case 'S':
                result.y = 4;
                break;
            default: Debug.Log("what?" + suit); break;
        }

        switch (kind)
        {
            case 'K':
                result.x = 1;
                break;
            case 'Q':
                result.x = 2;
                break;
            case 'J':
                result.x = 3;
                break;
            case '0':
                result.x = 4;
                break;
            case '9':
                result.x = 5;
                break;
            case '8':
                result.x = 6;
                break;
            case '7':
                result.x = 7;
                break;
            case '6':
                result.x = 8;
                break;
            case '5':
                result.x = 9;
                break;
            case '4':
                result.x = 10;
                break;
            case '3':
                result.x = 11;
                break;
            case '2':
                result.x = 12;
                break;
            case '1':
            case 'A':
                result.x = 13;
                break;
            default: Debug.Log("what? " + kind); break; 
        }
        return result;
    }

    //Calculate scores and find the winning player
    private List<Player> findWinners()
    {
        Debug.Log("Calculating scores");

        //Get score for player 1
        int highestScore = scoringSystem.score(players[0].hand);

        FeedbackGUI.setText("Player 1 score: " + highestScore);

        List<Player> winners = new List<Player>();

        winners.Add(players[0]);

        //Compare scores to get the highest
        for (int i = 1; i < players.Count; i++)
        {
            int tempScore = scoringSystem.score(players[i].hand);

            FeedbackGUI.setText("Player " + (i + 1) + " score: " + tempScore);

            if (tempScore > highestScore)
            {
                highestScore = tempScore;
                winners.Clear();
                winners.Add(players[i]);
            }
            else if (tempScore == highestScore)
            {
                winners.Add(players[i]);
            }

        }

        return winners;
    }

    #endregion

    #region Actions
    private void finishHands()
    {
        FeedbackGUI.setText("All player's staked positions will now be mined.");
        //Loop through players
        for(int i = 0; i < players.Count; i++)
        {
            if (players[i].hand.Count < numProspectingTurns)
            {
                //Add each staked card
                for(int j = 0; j < players[i].stakedCards.Count; j++) 
                {
                    players[i].hand.Add(players[i].stakedCards[j]);

                    //move the card to the side of the board
                    players[i].stakedCards[j].transform.position = clicker.findHandPosition(i);

                    //rotate if needed
                    if (players.Count > 2 && i == 1)
                        players[i].hand[players[i].hand.Count - 1].transform.Rotate(new Vector3(0, 1, 0), 90.0f);
                    else if (i == 3)
                        players[i].hand[players[i].hand.Count - 1].transform.Rotate(new Vector3(0, 1, 0), -90.0f);

                    GameObject stake = players[i].stakes[j];

                    //Remove card from staked cards list
                    players[i].stakedCards.Remove(players[i].stakedCards[j]);

                    //remove the stake from the list of stakes
                    players[i].stakes.Remove(stake);

                    //get rid of the stake GameObject
                    Destroy(stake);
                }
            }
        }
    }

    private void revealFaceDown()
    {
        FeedbackGUI.setText("Revealing cards!");
        foreach (Player p in players)
        {
            foreach (Card c in p.hand)
            {
                CreateMaterial(c.data.TexCoordinate, c.gameObject);
            }
        }

        for (int i = 0; i < BOARD_WIDTH; i++)
        {
            for (int j = 0; j < BOARD_HEIGHT; j++)
            {
                if (board[i, j] != null)
                    CreateMaterial(board[i, j].GetComponent<Card>().data.TexCoordinate, board[i, j]);
            }
        }
    }


    public void setUpBoard()
    {
        //shuffle cards
        //build board
        BuildDeck();
        ShuffleDeck();
        BuildBoard();
        gameState.CurrentGameState = GameStateManager.GameState.GAME_SETUP;
    }

    public void loadGameFromJson()
    {
        FeedbackGUI.setText("Loading game. . . ");
        //Load the players
        foreach (var entity in jsonFx.gameJSON.entities)
        {
            int playerID = entity.in_game_id;
            if (entity.is_avatar) //player
            {
                //positions
                Vector2 boardPosition = new Vector2(entity.row, entity.col);    //grid position
                Vector3 pos = clicker.Vector2ToPosition(boardPosition, 0.75f);          //Vector3 position, real space

                //create player
                Player p = (Player)Instantiate(clicker.tempPlayer, pos, Quaternion.identity);

                //set variables
                p.in_game_id = entity.in_game_id;
                p.Position = boardPosition;
                p.transform.renderer.material.color = clicker.bodyColor[p.in_game_id];

                //add to list of players
                players.Add(p);
            }
        }

        //sort the players based on in_game_id - they are read in alphabetically
        players.Sort(new PlayerSorter());
        for (int i = 0; i < players.Count; i++)
        {
            Debug.Log("Player " + i + ", DB in_game_id: " + players[i].in_game_id);
        }

        loadBoardFromJson(); //Load the board
        foreach (Player p in players)
            p.CurrentCard = board[(int)p.Position.x, (int)p.Position.y].GetComponent<Card>();

        //load the stakes
        foreach (var entity in jsonFx.gameJSON.entities)
        {
            if (!entity.is_avatar) //stake
            {
                Debug.Log("Loading a stake");
                //place the stake
                Vector3 pos = clicker.Vector2ToPosition(new Vector2(entity.row, entity.col), 0.51f);
                GameObject tempStake = (GameObject)Instantiate(clicker.stakePrefab, pos, Quaternion.identity);
                board[entity.row, entity.col].GetComponent<Card>().data.staked = true;

                //set to current player's color
                tempStake.transform.renderer.material.color = players[entity.in_game_id].transform.renderer.material.color;

                //add to player's stakes
                players[entity.in_game_id].stakes.Add(tempStake);
                players[entity.in_game_id].stakedCards.Add(board[entity.row, entity.col].GetComponent<Card>());
            }
        }

        //load game state and turn order
        currentPlayerIndex = jsonFx.gameJSON.current_player;
        currentRoll = jsonFx.gameJSON.current_roll;
        gameState.CurrentGameState = (GameStateManager.GameState)jsonFx.gameJSON.game_state;
        gameState.CurrentTurnState = (GameStateManager.TurnState)jsonFx.gameJSON.game_turn;
        switch (gameState.CurrentTurnState)
        {
            case GameStateManager.TurnState.TURN_ROLL:
                FeedbackGUI.setText("Please roll!");
                break;
            case GameStateManager.TurnState.TURN_MOVE:
                FeedbackGUI.setText("Please click on a space to move to. Double click to confirm, or click on the Action button.");
                calculateMoveLocations();
                break;
            case GameStateManager.TurnState.TURN_STAKE:
                FeedbackGUI.setText("Please select a location to stake a claim!");
                calculateStakeableCards();
                break;
            case GameStateManager.TurnState.TURN_MINE:
                FeedbackGUI.setText("You can mine one of your staked locations if you wish.");
                calculateMines();
                break;
            default: Debug.Log("See what's wrong here."); break;
        }

    }

    private void setupMiscElements()
    {
        MeshRenderer tempRenderer = GameObject.Find("Table").GetComponent<MeshRenderer>();
        tempRenderer.enabled = true;

    }

    private void loadBoardFromJson()
    {
        int counter = 0;
        foreach (var card in jsonFx.gameJSON.cards)
        {
            //number and suit
            char suit = card.suit[0];
            char kind = (card.kind + 1).ToString()[0];

            //value 
            int value = int.Parse(kind.ToString());
            if (card.kind == 0) //ace
            {
                value = 11;
                kind = 'A';
            }
            else if (card.kind == 9)
            {
                value = 10;
                kind = '0';
            }
            else if (card.kind == 10)
            {
                kind = 'J';
                value = 10;
            }
            else if (card.kind == 11)
            {
                value = 10;
                kind = 'Q';
            }
            else if (card.kind == 12)
            {
                value = 10;
                kind = 'K';
            }

            //texture coordinates
            Vector2 texCoordinates = getTexCoordinates(kind, suit);

            deck[counter] = new CardData(kind, suit, value, texCoordinates);//add it to the deck

            //create card
            if (card.in_game_id == -1) //on the board still
            {
                int row = card.col; int col = card.row; //this is bullshit
                Vector3 pos = new Vector3(.88f * row, .025f, 1.1f * col);
                board[row, col] = (GameObject)Instantiate(CardPrefab, pos, Quaternion.identity);


                //set variables
                deck[counter].Minable = card.minable;
                board[row, col].GetComponent<Card>().data = deck[counter];

                if (card.is_up) //turn face up if need be
                    CreateMaterial(texCoordinates, board[row, col]);
            }
            else //in a player's hand
            {
                Vector3 pos = clicker. findHandPosition(card.in_game_id); //find the hand's position
                GameObject cardObject = (GameObject)Instantiate(CardPrefab, pos, Quaternion.identity); //create the card
                int handSize =  players[card.in_game_id].hand.Count;
                players[card.in_game_id].hand.Add(cardObject.GetComponent<Card>()); //add it to the hand

                //rotate it if need be
                if (players.Count > 2 && card.in_game_id == 1)
                    players[card.in_game_id].hand[handSize].transform.Rotate(new Vector3(0, 1, 0), 90.0f);
                else if (CurrentPlayerIndex == 3)
                    players[card.in_game_id].hand[handSize].transform.Rotate(new Vector3(0, 1, 0), -90.0f);

                players[card.in_game_id].hand[handSize].data = deck[counter]; //set data

                if (card.is_up)  //turn face up if need be
                    CreateMaterial(texCoordinates, players[card.in_game_id].hand[handSize].gameObject);
            }

            counter++;
        }
    }

	public void saveWholeGameToJson()
	{
		jsonFx.gameJSON.current_player = currentPlayerIndex;
		jsonFx.gameJSON.current_roll = currentRoll;

		for (int i = 0; i < players.Count; i++)
		{
			int jsonPlayerIndex = jsonFx.findPlayerJsonIndex(i);
			jsonFx.gameJSON.entities[jsonPlayerIndex].row = (int)players[i].Position.x;
			jsonFx.gameJSON.entities[jsonPlayerIndex].col = (int)players[i].Position.y;

			for (int n = 0; n < players[i].stakedCards.Count; n++)
			{
				//how do we know the index within entity?
			}

			for (int n = 0; n < players[i].hand.Count; n++)
			{
				//how do we know the index within cards?
			}
		}
	}

	public void endTurn()
    {
        FeedbackGUI.setText("Ending turn of Player " + currentPlayerIndex + ".");

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

        //Check if the last turn has been triggered
        if (lastTurn)
        {
            //If the current player is the last player, game ends
            if (currentPlayerIndex == players.Count - 1)
            {
                endGame();
                return;
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

        //turn card back over?
        if (clicker.movedTo != null)
        {
            Material newMat = new Material(Shader.Find("Diffuse"));
            newMat.mainTexture = CardTexture;
            newMat.mainTextureScale = new Vector2(0.0668f, 0.2f);
            newMat.mainTextureOffset = new Vector2(0.138f, 0.0f);
            clicker.movedTo.renderer.material = newMat;
            clicker.movedTo = null;
        }


        //increment the turns and check if game state needs to be changed
        phaseTurns++;

        if (phaseTurns >= players.Count * numProspectingTurns)
        {
            FeedbackGUI.setText("Moving into the Mining Phase.");
            gameState.CurrentGameState = GameStateManager.GameState.GAME_MINING_STATE;  //move on to the next game state
            phaseTurns = 0;
        }

        //Check for last turn of the game
        //Only check if the current player is the last player
        if (!lastTurn)
        {

            int numFullHands = 0;

            //If any player has 5 cards in their hand, set lastTurn and increment numFullHands
            foreach (Player p in players)
            {
                if (p.hand.Count >= numProspectingTurns)
                {
                    lastTurn = true;

                    numFullHands++;
                }
            }

            //If EVERYONE has a full hand, game ends immediately
            if (numFullHands == players.Count)
                endGame();

            //If we're on the last player, game ends immediately
            if (currentPlayerIndex == players.Count - 1 && numFullHands > 0)
                endGame();
        }

        currentPlayerIndex++;	// move to next player
        if (currentPlayerIndex >= players.Count)	//wrap around if necessary
            currentPlayerIndex = 0;

        FeedbackGUI.setText("Player " + currentPlayerIndex + "'s turn.");

        //move turn state back to the beginning
        gameState.CurrentTurnState = GameStateManager.TurnState.TURN_ROLL;
        FeedbackGUI.setText("Please roll the dice or click the Skip button to remain at your current location.");
    }

    private void endGame()
    {
        FeedbackGUI.setText("Game over.");
        gameState.CurrentGameState = GameStateManager.GameState.GAME_END;
        finishHands();
        revealFaceDown();
        List<Player> winners = findWinners();

        if (winners.Count == 1)
        {
            FeedbackGUI.setText("Player " + (players.IndexOf(winners[0]) + 1) + " wins!");
        }
        else
        {
            string winningPlayers = "Tie! Winning players: ";
            foreach (Player p in winners)
            {
                winningPlayers += (players.IndexOf(p) + 1) + ", ";
            }
            FeedbackGUI.setText(winningPlayers);
        }

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
        FeedbackGUI.setText("A " + currentRoll + " was rolled.");
        return currentRoll;
    }
    #endregion

    #region setup
    private void BuildDeck()
    {
        FeedbackGUI.setText("Setting up game. . .");
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
                Vector3 pos = new Vector3(.88f * i, .025f, 1.1f * j);
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

                

                foreach (Transform child in board[i, j].transform)
                {
                Debug.Log("------------------------------Scale" + child.name);
                child.localPosition = new Vector3(0f, (2 * board[i, j].GetComponent<Card>().data.Value), 0f);
                child.localScale = new Vector3(0f, (4 * board[i, j].GetComponent<Card>().data.Value), 0f);
                }

                counter++;
            }
        }
    }

    #endregion

    public void CreateMaterial(Vector2 Coordinate, GameObject Card)
    {
        Material newMat = new Material(Shader.Find("Diffuse"));
        newMat.mainTexture = CardTexture;
        newMat.mainTextureScale = new Vector2(0.0668f, 0.2f);
        newMat.mainTextureOffset = new Vector2(CARD_TEX_X_OFFSET * -Coordinate.x, CARD_TEX_Y_OFFSET * Coordinate.y);
        Card.renderer.material = newMat;
    }

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

    private void BarControl()
    {

    }

}

public class PlayerSorter : Comparer<Player>
{
    public override int Compare(Player one, Player two)
    {
        if (one.in_game_id < two.in_game_id)
            return -1;
        else if (one.in_game_id > two.in_game_id)
            return 1;
        else
            return 0;
    }
}