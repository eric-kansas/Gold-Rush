using UnityEngine;
using System.Collections;

public class GuiHandler : MonoBehaviour
{
    #region properties

	private GUIStyle GuiStyle = new GUIStyle();

    #region Menu-related
    /* Possible states of the menu GUI:
     *       MAIN: Menu is on the default Main Menu.
     *       OPTIONS: Menu is instead showing the Options Menu.
     *       RULES: Menu is instead showing the rules.
     *       ABOUT: Tell the fans more about ourselves.
     */
    private enum MenuState { MAIN, OPTIONS, RULES, ABOUT }

    /* The current state of the menu */
    private MenuState currentMenuState = MenuState.MAIN;

    /* Possible tabs of the rules menu */
    private enum RulesTab { PROSPECTING, MINING, SCORING }

    /* Current tab in the rules menu */
    private RulesTab currentTab = RulesTab.PROSPECTING;

	private enum TextSize { BULLET, FULL }
	private TextSize textSize = TextSize.BULLET;

	private enum Developer { KANSAS, GARY, JON, PHIL }
	private Developer dev = Developer.KANSAS;

    /* Contains the strings to show on each tab */
    private string[] rulesText = new string[3];
	private string[] fullText = new string[3];
	private string[] aboutText = new string[4];

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

    /* What the buttons will say */
    private string actionText, skipText;

    /* Whether or not the current action can be skipped or cancelled */
    private bool showSkipButton = true;

    #endregion
    #endregion

    // Use this for initialization
    void Start()
    {
		GuiStyle.wordWrap = true;
		GuiStyle.normal.textColor = Color.white;
		GuiStyle.active.textColor = Color.red;

        gM = transform.GetComponent<GameManager>();
        clicker = this.GetComponent<ClickHandler>();

        menuOuterRect = new Rect(Screen.width * .4f, Screen.height * .25f, Screen.width * .2f, Screen.height * .5f);

        float change = .03f;
        float xChange = (menuOuterRect.width * change) / 2;
        float yChange = (menuOuterRect.height * change) / 2;
        buttonRect = new Rect(menuOuterRect.x + xChange, menuOuterRect.y + yChange, menuOuterRect.width * (1 - change), menuOuterRect.height * (1 - change));

		#region Text
		#region Rules
		rulesText[0] = "\n\n" +
			"A. Choose to roll the die or not.\n\n" +
			"	A1- Rolled: Move the player and flip card.\n\n" +
			"	A2- Did Not Roll: Flip card.\n\n" +
			"C. Stake adjacent card.";
        fullText[0] = 
			"The first stage of the game is the prospecting stage.\n\n"+
			"You may roll the die to move, or skip rolling to stay where you are. "+
			"Either way the card underneath the player will be revealed if it is not already.\n\n"+
			"Then you will be able to stake a claim on a card. In the prospecting phase you must stake a claim. "+
			"You may only stake the card under your avatar or on an adjacent card, not including diagonals.";

		rulesText[1] = "\n" +
			"A. Moving: Same as in Prospecting Phase.\n" +
			"	Except: Bumping opponent's stakes is allowed. That card can't be mined for one turn.\n\n" +
			"B. Choose to stake or not stake.\n" +
			"	B1- Staked: Move previously placed stake.\n\n" +
			"C. Choose to mine a staked card or not.\n" +
			"	C1- Mined: Take card into your hand. Move players from empty space to valid card if necessary.";
		fullText[1] =
			"The second stage of the game is the mining stage.\n\n" +
			"Moving works the same, but if you land on an opponent's stake you can bump it to an adjacent space. The card that was staked will be un-minable.\n\n" +
			"You may also mine staked cards, which involves locking them in by removing them from the board. They will be moved to your hand. Any players on the empty space must be moved to a valid card.";

		rulesText[2] =
			"The purpose is to get the highest score.\n\n" +
			"A. Card values:\n\n" +
			"	A1- Face Cards = 10 points.\n" +
			"	A2- Aces = 11 points.\n" +
			"	A3- Numbered cards = Face value.\n\n" +
			"B. Add up the values of the cards in your highest scoring grouping. Types of groups:\n\n" +
			"	Flushes: 2+ cards of the same suit.\n" +
			"	N of a Kind: 2+ cards of the same kind.\n";
		fullText[2] = "After the game ends, each player's score will be calculated. A player wins by having the highest score.\n\n"+
			"Each card has a value. Face cards (Jack, Queen, and King) are worth 10. Aces are worth 11 points. Numbered cards are worth their face value.\n\n"+
			"To calculate a player's score, different groupings of cards in the player's hand are compared. A grouping can be a flush or an N-of-a-kind. The highest scoring group will yield the player's score.";
		#endregion

		#region About Us
		aboutText[0] = "Eric Heaney, a.k.a. Kansas\n\n"+
			"Currently a 4th year Game Design & Development student at RIT.\n\n"+
			"Role: Gold Rush designer and programmer, focusing on database and server code.";

		aboutText[1] = "Name: Gary Lake\n\n"+
			"Currently a 4th year Game Design & Development student at RIT set to graduate in May 2012.\n\n"+
			"Role: Gold Rush programmer, focusing on game code.";

		aboutText[2] = "Name: Jonathan Hughes\n\n" +
			"Currently a 3rd year New Media Interactive Development student at RIT.\n\n" +
			"Role: Gold Rush programmer and user interface designer.";
		
		aboutText[3] = "Name: Philip Moccio\n\n"+
			"Currently a [Placeholder text]\n\n"+
			"Role: Gold Rush programmer.";

		#endregion
		#endregion
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
        else if (currentMenuState == MenuState.ABOUT)
            showAboutUs();
    }

    private void mainMenu()
    {
        //contain everything else inside of it
        GUI.BeginGroup(buttonRect);

        //set the button dimensions
        float width = buttonRect.width;
        float height = buttonRect.height * .25f; //multiply by 1 over number of the buttons
        GUILayoutOption[] options = { GUILayout.Width(width), GUILayout.Height(height) };

        if (GUILayout.Button("Start Game", options)) //start the game
        {
            gM.gameState.CurrentGameState = GameStateManager.GameState.GAME_SETUP;
            gM.setUpBoard();
            //gM.loadGameFromJson();
        }
        else if (GUILayout.Button("Options", options)) //load the options menu instead
            currentMenuState = MenuState.OPTIONS;
        else if (GUILayout.Button("How To Play", options)) // show the rules instead
            currentMenuState = MenuState.RULES;
        else if (GUILayout.Button("About the Developers", options))
            currentMenuState = MenuState.ABOUT;

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
        string[] selectionStrings = { "Normal", "Easy" };

        GUI.EndGroup();
    }

    private void showRules()
    {
        //contain everything else inside of it
        GUI.BeginGroup(buttonRect);

        //tabs
        GUILayout.BeginHorizontal(GUILayout.Height(buttonRect.height * 0.1f));

        if (GUILayout.Button("Prospecting"))
            currentTab = RulesTab.PROSPECTING;
		else if (GUILayout.Button("Mining"))
            currentTab = RulesTab.MINING;
		else if (GUILayout.Button("Scoring"))
            currentTab = RulesTab.SCORING;
		else if (GUILayout.Button("Back"))
        {
			//return rules to default
            currentTab = RulesTab.PROSPECTING;
			textSize = TextSize.BULLET;

            currentMenuState = MenuState.MAIN;
        }
        GUILayout.EndHorizontal();

        GUILayoutOption[] options = { GUILayout.Width(buttonRect.width), GUILayout.Height(buttonRect.height * 0.75f) };
		if (textSize == TextSize.BULLET)
			GUILayout.Box(rulesText[(int)currentTab], GuiStyle, options);
		else
			GUILayout.Box(fullText[(int)currentTab], GuiStyle, options);

		string title = "Bullet Form";
		if (textSize == TextSize.BULLET)
			title = "More Info";

		if (GUILayout.Button(title))
		{
			if (textSize == TextSize.BULLET)
				textSize = TextSize.FULL;
			else
				textSize = TextSize.BULLET;
		}

        GUI.EndGroup();
    }

    private void showAboutUs()
    {
		//contain everything else inside of it
		GUI.BeginGroup(buttonRect);

		//tabs
		GUILayout.BeginHorizontal(GUILayout.Height(buttonRect.height * 0.1f));

		if (GUILayout.Button("Kansas"))
			dev = Developer.KANSAS;
		else if (GUILayout.Button("Gary"))
			dev = Developer.GARY;
		else if (GUILayout.Button("Jon"))
			dev = Developer.JON;
		else if (GUILayout.Button("Phil"))
			dev = Developer.PHIL;
		else if (GUILayout.Button("Back"))
		{
			dev = Developer.KANSAS;
			currentMenuState = MenuState.MAIN;
		}
		GUILayout.EndHorizontal();

		GUILayoutOption[] options = { GUILayout.Width(buttonRect.width), GUILayout.Height(buttonRect.height * 0.75f) };
		GUILayout.Box(aboutText[(int)dev], GuiStyle, options);

		GUI.EndGroup();
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
                    {
                        actionText = "Prospect";
                        showSkipButton = false;
                    }
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
            if (gM.gameState.CurrentGameState == GameStateManager.GameState.GAME_END)
                return;
            //don't do anythign after game ends

            if (gM.players.Count <= 1) //if there aren't enough players, the button should not do anything
                return;

            //start the game if there are enough players and a button is hit
            if (gM.gameState.CurrentGameState == GameStateManager.GameState.GAME_SETUP)
                gM.gameState.CurrentGameState = GameStateManager.GameState.GAME_PROSPECTING_STATE;

            switch (gM.gameState.CurrentTurnState)
            {
                case GameStateManager.TurnState.TURN_ROLL:		// The player is choosing to roll

                    gM.CurrentRoll = gM.Roll();		// roll the dice

                    FeedbackGUI.setText("Please click on a space to move to. Double click to confirm, or click on the Action button.");

                    gM.calculateMoveLocations();	// calculate where the player can move as a result of the dice roll

                    // At the beginning, this bool is true - player can stay where he/she is by choosing not to roll. 
                    // Once the player rolls, he must move, so set this bool to false.
                    showSkipButton = false;

                    gM.gameState.CurrentTurnState = GameStateManager.TurnState.TURN_MOVE; //move on to the next turn state
                    break;
                case GameStateManager.TurnState.TURN_MOVE:	// the player is moving after rolling the dice

                    if (gM.pEnabled) //make sure the player has actually moved - he/she cannot sit on the same spot after rolling the dice
                    {			//pEnabled is set to true in clickHandler's moveClick()

                        FeedbackGUI.setText("Moving player.");

                        //save to turn this card back over again
                        if (gM.CurrentRoll == 1)
                            clicker.movedTo = clicker.TempCard;

                        gM.players[gM.CurrentPlayerIndex].CurrentCard = clicker.TempCard; //set the player's current card

                        gM.players[gM.CurrentPlayerIndex].Position = clicker.PositionToVector2(gM.players[gM.CurrentPlayerIndex].transform.position);   //update the player's grid position

						int JsonIndex = gM.jsonFx.findPlayerJsonIndex(gM.CurrentPlayerIndex);
						gM.jsonFx.gameJSON.entities[JsonIndex].row = (int)gM.players[gM.CurrentPlayerIndex].Position.x;
						gM.jsonFx.gameJSON.entities[JsonIndex].col = (int)gM.players[gM.CurrentPlayerIndex].Position.y;

                        gM.clearHighlights();

                        //prospect
                        gM.CreateMaterial(gM.players[gM.CurrentPlayerIndex].CurrentCard.data.TexCoordinate, gM.board[(int)gM.players[gM.CurrentPlayerIndex].Position.x,
                                                                    (int)gM.players[gM.CurrentPlayerIndex].Position.y]);

                        this.clicker.TempCard.data.isUp = true;
                        gM.UpdateBars();

                        if (gM.gameState.CurrentGameState != GameStateManager.GameState.GAME_MINING_STATE || !clicker.TempCard.data.staked)
                        {
                            FeedbackGUI.setText("Please press a location to stake a claim on.");
                            gM.calculateStakes(); // based on where the player has moved to, find the adjacent positions he/she can stake a claim

                            gM.gameState.CurrentTurnState = GameStateManager.TurnState.TURN_STAKE;  //move on to the next turn state
                        }
                        else
                        {
                            if (clicker.stakeOwnerIndex == -1)
                            {
                                FeedbackGUI.setText("You have landed on an opponent's stake. You may move it if you so choose.");
                                clicker.prepareBump();
                            }

                            if (clicker.movedStake)
                            {
                                FeedbackGUI.setText("Opponent's stake was moved.");
                                FeedbackGUI.setText("You may stake this position but you will not be able to mine it this turn.");
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
                        FeedbackGUI.setText("Once you roll you must move.");

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
                    {
                        if (gM.gameState.CurrentGameState == GameStateManager.GameState.GAME_MINING_STATE)
                            FeedbackGUI.setText("Please stake a claim or press skip.");
                        else if (gM.gameState.CurrentGameState == GameStateManager.GameState.GAME_PROSPECTING_STATE)
                            FeedbackGUI.setText("In the prospecting phase you must stake a claim.");
                    }
                    break;
                case GameStateManager.TurnState.TURN_MINE:
                    Debug.Log("turn state: TURN_MINE");

                    if (clicker.numToMove == 0)
                    {

                    }
                    else
                    {
                        FeedbackGUI.setText("A player is in a dangerous mine! Please move them to solid ground.");
                        if (gM.players[clicker.indexToMove].Position != new Vector2(-1, -1))
                        {
                            clicker.numToMove--; //one less that needs to be looked at

                            //end the turn if all players are on valid spots now
                            if (clicker.numToMove <= 0)
                            {
                                FeedbackGUI.setText("Good thing. It's safe now.");
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

                if (gM.gameState.CurrentGameState == GameStateManager.GameState.GAME_END)
                    return;
                //don't do anythign after game ends

                //start the game if there are enough players and a button is hit
                if (gM.players.Count > 1 && gM.gameState.CurrentGameState == GameStateManager.GameState.GAME_SETUP)
                    gM.gameState.CurrentGameState = GameStateManager.GameState.GAME_PROSPECTING_STATE;

                switch (gM.gameState.CurrentTurnState)
                {
                    case GameStateManager.TurnState.TURN_ROLL:		// the player is choosing to stay where they are and not roll the dice this turn
                        FeedbackGUI.setText("You must like your current location.");

                        gM.pEnabled = true; //player is skipping rolling, meaning they can prospect without moving
                        gM.sEnabled = false; //reset stake boolean, player still needs to do this

                        gM.clearHighlights();
                        gM.calculateStakes(); // based on where the player has moved to, find the adjacent positions he/she can stake a claim

                        gM.gameState.CurrentTurnState = GameStateManager.TurnState.TURN_STAKE; // move on to the next turn state
                        break;
                    case GameStateManager.TurnState.TURN_MOVE:
                        FeedbackGUI.setText("Wouldn't be nice to interfere, would it?");

                        gM.players[clicker.stakeOwnerIndex].stakedCards[clicker.stakeIndex].transform.position = gM.players[gM.CurrentPlayerIndex].Position;
                        clicker.stakeOwnerIndex = clicker.stakeIndex = -1;
                        clicker.movedStake = false;

                        gM.clearHighlights();
                        gM.calculateStakes(); // based on where the player has moved to, find the adjacent positions he/she can stake a claim
                        gM.gameState.CurrentTurnState = GameStateManager.TurnState.TURN_STAKE;  //move on to the next turn state

                        break;
                    case GameStateManager.TurnState.TURN_STAKE: //player is not moving his stakes his turn (option is available in mining phase only)
                        FeedbackGUI.setText("The claims you have are better anyway.");
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
                        FeedbackGUI.setText("You'll find something better. No worries.");
                        gM.endTurn(); //after the player takes a card, the turn ends
                        break;
                }
            }
        }
    }

    #endregion
}
