using UnityEngine;
using System.Collections;

public class GuiHandler : MonoBehaviour
{
    #region properties
    #region Menu-related
    /* Possible states of the menu GUI:
     *       MAIN: Menu is on the default Main Menu.
     *       OPTIONS: Menu is instead showing the Options Menu.
     *       RULES: Menu is instead showing the rules.
     */
    public enum MenuState { MAIN, OPTIONS, RULES }

    /* The current state of the menu */
    public MenuState currentMenuState = MenuState.MAIN;

    /* The Rectangle to use for the outer menu box */
    private Rect menuOuterRect;

    /* The Rectangle to use for the button grouping - slightly smaller */
    private Rect buttonRect;

    #endregion

    #region In-game

    /* A reference to the GameManager */
    GameManager gM;

    /* A reference to the ClickHandler */
    ClickHandler clicker;

    /* The position and size of the action button */
    private Rect actionRect;

    /* The position and size of the Player Display */
    public Rect playerRect = new Rect(10, 10, 100, 15);
    /* The string for the Player Display */
    private string playerText = "";

    /* What the buttons will say */
    private string actionText, skipText;

    /* Whether or not the current action can be skipped or cancelled */
    private bool showSkipButton = true;

    #endregion
    #endregion

    // Use this for initialization
    void Start()
    {
        gM = transform.GetComponent<GameManager>();
        clicker = this.GetComponent<ClickHandler>();

        menuOuterRect = new Rect(Screen.width * .4f, Screen.height * .25f, Screen.width * .2f, Screen.height * .5f);

        float change = .03f;
        float xChange = (menuOuterRect.width * change) / 2;
        float yChange = (menuOuterRect.height * change) / 2;
        buttonRect = new Rect(menuOuterRect.x + xChange, menuOuterRect.y + yChange, menuOuterRect.width * (1 - change), menuOuterRect.height * (1 - change));
    }

    // Update is called once per frame
    void Update()
    {

    }

    #region Menu
    public void ShowMenu()
    {
        //draw an outer box
        GUI.Box(menuOuterRect, "");

        if (currentMenuState == MenuState.MAIN)
            mainMenu();
        else if (currentMenuState == MenuState.OPTIONS)
            optionsMenu();
        else if (currentMenuState == MenuState.RULES)
            showRules();
    }

    private void mainMenu()
    {
        //contain everything else inside of it
        GUI.BeginGroup(buttonRect);

        //set the button dimensions
        float width = buttonRect.width;
        float height = buttonRect.height * .33f; //multiply by 1 over number of the buttons
        GUILayoutOption[] options = { GUILayout.Width(width), GUILayout.Height(height) };

        if (GUILayout.Button("Start Game", options)) //start the game
        {
            gM.setUpBoard();
            //gM.loadGameFromJson();
        }
        else if (GUILayout.Button("Options (Coming Soon)", options)) //load the options menu instead
        { /*currentMenuState = MenuState.OPTIONS; */ }
        else if (GUILayout.Button("How To Play", options)) // show the rules instead
            currentMenuState = MenuState.RULES;

        GUI.EndGroup();
    }

    private void optionsMenu()
    {
        //contain everything else inside of it
        GUI.BeginGroup(buttonRect);

        //set the button dimensions
        float width = buttonRect.width;
        float height = buttonRect.height * .33f; //multiply by 1 over number of the buttons

        int selectionGrid = 0;
        string[] selectionStrings = { "Easy", "Normal" };

        GUI.EndGroup();
    }

    private void showRules()
    {

    }
    #endregion

    #region In-game GUI
    public void adjustLocation()
    {
        actionRect = new Rect((Screen.width * .8f), (Screen.height * .82f), 150, 75);
    }

    /// <summary>
    /// Set the text for the two buttons.
    /// </summary>
    public void setText()
    {
        //set the text fields on the buttons
        actionText = "";
        skipText = "";
        if (gM.players.Count <= 1)
        {
            showSkipButton = false;
            actionText = "Please place players.";		// If not enough player objects have been placed, let the players know.
            playerText = "Player " + gM.CurrentPlayerIndex + 1 + ": pleace place your peice on the board";
        }
        else
        {
            switch (gM.gameState.CurrentTurnState)
            {
                case GameStateManager.TurnState.TURN_ROLL:
                    actionText = "Roll";
                    skipText = "Prospect";
                    showSkipButton = true;
                    break;
                case GameStateManager.TurnState.TURN_MOVE:
                    if (gM.gameState.CurrentGameState != GameStateManager.GameState.GAME_MINING_STATE || clicker.stakeOwnerIndex == -1)
                        actionText = "Prospect";
                    else
                    {
                        actionText = "Move Opponent Stake";
                        showSkipButton = true;
                        skipText = "Cancel";
                    }

                    break;
                case GameStateManager.TurnState.TURN_STAKE:
                    actionText = "Stake";
                    showSkipButton = false;
                    if (gM.gameState.CurrentGameState == GameStateManager.GameState.GAME_MINING_STATE)
                    {
                        showSkipButton = true;		// only once all a player's stakes have been placed and it goes into the mining phase,
                        if (gM.cancelStake)
                        {
                            skipText = "Cancel";			// players can skip the option to move their stakes around
                        }
                        else
                        {
                            skipText = "Mine";			// players can skip the option to move their stakes around
                        }
                    }
                    break;
                case GameStateManager.TurnState.TURN_MINE:
                    if (clicker.numToMove == 0)
                    {
                        actionText = "Mine";
                        skipText = "End Turn";
                        showSkipButton = true;
                    }
                    else
                    {
                        showSkipButton = false;
                        actionText = "Move Player " + clicker.indexToMove;
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Handles the results of a player clicking on the Action button.
    /// </summary>
    public void handleAction()
    {
        if (GUI.Button(actionRect, actionText))
        {
            if (gM.players.Count <= 1) //if there aren't enough players, the button should not do anything
                return;

            //start the game if there are enough players and a button is hit
            if (gM.gameState.CurrentGameState == GameStateManager.GameState.GAME_SETUP)
                gM.gameState.CurrentGameState = GameStateManager.GameState.GAME_PROSPECTING_STATE;

            switch (gM.gameState.CurrentTurnState)
            {
                case GameStateManager.TurnState.TURN_ROLL:		// The player is choosing to roll

                    gM.CurrentRoll = gM.Roll();		// roll the dice
                    gM.calculateMoveLocations();	// calculate where the player can move as a result of the dice roll

                    // At the beginning, this bool is true - player can stay where he/she is by choosing not to roll. 
                    // Once the player rolls, he must move, so set this bool to false.
                    showSkipButton = false;

                    gM.gameState.CurrentTurnState = GameStateManager.TurnState.TURN_MOVE; //move on to the next turn state
                    break;
                case GameStateManager.TurnState.TURN_MOVE:	// the player is moving after rolling the dice

                    if (gM.pEnabled) //make sure the player has actually moved - he/she cannot sit on the same spot after rolling the dice
                    {			//pEnabled is set to true in clickHandler's moveClick()

                        //save to turn this card back over again
                        if (gM.CurrentRoll == 1)
                            clicker.movedTo = clicker.TempCard;

                        gM.players[gM.CurrentPlayerIndex].CurrentCard = clicker.TempCard; //set the player's current card

                        gM.players[gM.CurrentPlayerIndex].Position = clicker.PositionToVector2(gM.players[gM.CurrentPlayerIndex].transform.position);   //update the player's grid position

                        gM.clearHighlights();

                        //prospect
                        gM.CreateMaterial(gM.players[gM.CurrentPlayerIndex].CurrentCard.data.TexCoordinate, gM.board[(int)gM.players[gM.CurrentPlayerIndex].Position.x,
                                                                    (int)gM.players[gM.CurrentPlayerIndex].Position.y]);

                        if (gM.gameState.CurrentGameState != GameStateManager.GameState.GAME_MINING_STATE || !clicker.TempCard.data.staked)
                        {
                            gM.calculateStakes(); // based on where the player has moved to, find the adjacent positions he/she can stake a claim

                            gM.gameState.CurrentTurnState = GameStateManager.TurnState.TURN_STAKE;  //move on to the next turn state
                        }
                        else
                        {
                            if (clicker.stakeOwnerIndex == -1)
                                clicker.prepareBump();

                            if (clicker.movedStake)
                            {
                                Debug.Log("Confirm bump!");
                                clicker.stakeOwnerIndex = clicker.stakeIndex = -1;
                                clicker.movedStake = false;
                                clicker.TempStake = null; //set to null for normal staking
                                Vector2 pos = gM.players[gM.CurrentPlayerIndex].Position;
                                clicker.TempCard = gM.board[(int)pos.x, (int)pos.y].GetComponent<Card>(); //reset to player's card


                                gM.clearHighlights();
                                gM.calculateStakes(); // based on where the player has moved to, find the adjacent positions he/she can stake a claim
                                gM.gameState.CurrentTurnState = GameStateManager.TurnState.TURN_STAKE;  //move on to the next turn state
                            }
                        }
                    }
                    else
                        Debug.Log("Player rolled, they need to actually move");

                    break;
                case GameStateManager.TurnState.TURN_STAKE: //player is staking a claim after placing a marker

                    if (gM.sEnabled) // make sure the player has actually placed a marker
                    { //sEnabled is set to true in clickHandler's stakeClick()

                        //clicker.selectedCard = false;

                        // Either move on or let the next player go
                        if (gM.gameState.CurrentGameState == GameStateManager.GameState.GAME_MINING_STATE)
                        {
                            gM.gameState.CurrentTurnState = GameStateManager.TurnState.TURN_MINE;
                            showSkipButton = true;		//the player can choose not to pick up a card this turn if he/she wants

                            //clear board and show new highlights
                            gM.clearHighlights();
                            gM.calculateMines();
                        }
                        else
                            gM.endTurn();
                    }
                    else
                        Debug.Log("Pressed with no stake placed");
                    break;
                case GameStateManager.TurnState.TURN_MINE:
                    Debug.Log("turn state: TURN_MINE");

                    if (clicker.numToMove == 0)
                    {

                    }
                    else
                    {
                        if (gM.players[clicker.indexToMove].Position != new Vector2(-1, -1))
                        {
                            clicker.numToMove--; //one less that needs to be looked at

                            //end the turn if all players are on valid spots now
                            if (clicker.numToMove <= 0)
                            {
                                gM.endTurn();
                            }
                            else
                            {
                                //someone isn't, find their index
                                for (int index = 0; index < gM.players.Count; index++)
                                {
                                    if (gM.players[index].Position == new Vector2(-1, -1))
                                        clicker.indexToMove = index;
                                }
                            }
                        }
                    }

                    //endTurn();
                    break;
                default: Debug.Log("whoops"); break;
            }
        }
    }

    /// <summary>
    /// Handles the results of a player clicking on the Skip button.
    /// </summary>
    public void handleSkip()
    {

        float width = 70;
        if (showSkipButton) //only show skip button if player can choose not to do this action
        {
            if (GUI.Button(new Rect(new Rect((actionRect.x + actionRect.width - width), (actionRect.y + actionRect.height), width, 20)), skipText))
            {
                //start the game if there are enough players and a button is hit
                if (gM.players.Count > 1 && gM.gameState.CurrentGameState == GameStateManager.GameState.GAME_SETUP)
                    gM.gameState.CurrentGameState = GameStateManager.GameState.GAME_PROSPECTING_STATE;

                switch (gM.gameState.CurrentTurnState)
                {
                    case GameStateManager.TurnState.TURN_ROLL:		// the player is choosing to stay where they are and not roll the dice this turn
                        Debug.Log("Skipped movement");

                        gM.pEnabled = true; //player is skipping rolling, meaning they can prospect without moving
                        gM.sEnabled = false; //reset stake boolean, player still needs to do this

                        gM.clearHighlights();
                        gM.calculateStakes(); // based on where the player has moved to, find the adjacent positions he/she can stake a claim

                        gM.gameState.CurrentTurnState = GameStateManager.TurnState.TURN_STAKE; // move on to the next turn state
                        break;
                    case GameStateManager.TurnState.TURN_MOVE:
                        Debug.Log("Canceling bump");

                        gM.players[clicker.stakeOwnerIndex].stakedCards[clicker.stakeIndex].transform.position = gM.players[gM.CurrentPlayerIndex].Position;
                        clicker.stakeOwnerIndex = clicker.stakeIndex = -1;
                        clicker.movedStake = false;

                        gM.clearHighlights();
                        gM.calculateStakes(); // based on where the player has moved to, find the adjacent positions he/she can stake a claim
                        gM.gameState.CurrentTurnState = GameStateManager.TurnState.TURN_STAKE;  //move on to the next turn state

                        break;
                    case GameStateManager.TurnState.TURN_STAKE: //player is not moving his stakes his turn (option is available in mining phase only)
                        if (gM.cancelStake)
                        {
                            gM.clicker.resetStaking();
                            gM.cancelStake = false;
                            gM.sEnabled = false;
                        }
                        else
                        {
                            gM.gameState.CurrentTurnState = GameStateManager.TurnState.TURN_MINE;
                            gM.clearHighlights();
                            gM.calculateMines();
                        }
                        break;
                    case GameStateManager.TurnState.TURN_MINE:
                        gM.endTurn(); //after the player takes a card, the turn ends
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Shows the Current player in a gui text object
    /// </summary>
    public void playerDisplay()
    {
        GUI.Box(playerRect, "");
        GUI.Label(playerRect, playerText);
    }

    /// <summary>
    /// Add text to playerText.
    /// </summary>
    public void printToGUI(string text)
    {
        playerText += "\n" + text;
    }
    #endregion
}
