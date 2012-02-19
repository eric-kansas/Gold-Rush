using UnityEngine;
using System.Collections;

public class GuiHandler : MonoBehaviour {

    /* The position and size of the action button */
    private Rect actionRect;

    /* The position and size of the Player Display */
    public Rect playerRect = new Rect(10, 10, 100, 15);
    /* The string for the Player Display */
    private string playerText = "";

    /* What the buttons will say */
    private string actionText, skipText;

    /* Whether or not the current action can be skipped or cancelled */
    private bool showSkipButton;

    /* A reference to the GameManager */
    GameManager gM;

    /* A reference to the ClickHandler */
    ClickHandler clicker;

	// Use this for initialization
	void Start () {
        gM = transform.GetComponent<GameManager>();
        clicker = this.GetComponent<ClickHandler>();
        showSkipButton = true;
	}
	
	// Update is called once per frame
	void Update () {

	}

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
                    actionText = "Prospect";
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
                    actionText = "Mine";
                    skipText = "Roll";
                    showSkipButton = true;
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

                        gM.players[gM.CurrentPlayerIndex].CurrentCard = clicker.TempCard; //set the player's current card

                        gM.players[gM.CurrentPlayerIndex].Position = clicker.PositionToVector2(gM.players[gM.CurrentPlayerIndex].transform.position);   //update the player's grid position

                        gM.clearHighlights();
                        gM.calculateStakes(); // based on where the player has moved to, find the adjacent positions he/she can stake a claim

                        gM.gameState.CurrentTurnState = GameStateManager.TurnState.TURN_STAKE;  //move on to the next turn state
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
}
