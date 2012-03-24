using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClickHandler : MonoBehaviour
{

    #region Properties
    /* The game object the player clicked on */
    private GameObject target;

    /* An imaginary line coming out of the card */
    private Vector3 normal;

    /* GameManager - handles game logic */
    private GameManager gM;

    /* A list of colors that the players can be. */
    public List<Color> bodyColor;

    /*  A temporary player object before it is added to the players list */
    public Player tempPlayer;

    private Card tempCard;              //Current clicked card
    public Card TempCard
    {
        get { return tempCard; }
        set { tempCard = value; }
    }

    private Card lastCard;              // The last card the player staked, which isn't yet locked in (previous click)
    private int tempCardIndex;  /* Temporary objects */

    #region Prospecting

    /*Card the player just moved to, to set facedown if they rolled a one */
    private Card movedTo = null;
    public Card MovedTo
    {
        get { return movedTo; }
        set { movedTo = value; }
    }

    /* The index of the stake that will be bumped */
    private int stakeIndex = -1;
    public int StakeIndex
    {
        get { return stakeIndex; }
        set { stakeIndex = value; }
    }

    /* The index of the player owning the stake that will be bumped */
    private int stakeOwnerIndex = -1;
    public int StakeOwnerIndex
    {
        get { return stakeOwnerIndex; }
        set { stakeOwnerIndex = value; }
    }

    /* Whether the stake has been bumped yet */
    private bool movedStake;
    public bool MovedStake
    {
        get { return movedStake; }
        set { movedStake = value; }
    }

    #endregion

    #region Staking

    /* Stake asset */
    public GameObject stakePrefab;

    /* Whether or not the player has selected a card to stake (in mining phase) */
    private bool selectedCard = false;
    public bool SelectedCard
    {
        get { return selectedCard; }
        set { selectedCard = value; }
    }

    /* Temporary stake that can be moved around the board before the player locks it in */
    private GameObject tempStake;
    public GameObject TempStake
    {
        get { return tempStake; }
        set { tempStake = value; }
    }

    /* Card the player staked but hasn't locked in yet */
    private Card tempStakedCard;

    /* Card the player moved an existing stake from */
    private Card oldStakedCard;

    #endregion

    #region Mining

    /* A staked card the player has marked for mining but hasn't locked in yet */
    private Card minedCard;
    public Card MinedCard
    {
        get { return minedCard; }
        set { minedCard = value; }
    }

    /* Whether or not the player has selected a card to mine */
    private bool hasChosenMine;
    public bool HasChosenMine
    {
        get { return hasChosenMine; }
        set { hasChosenMine = value; }
    }

    /* How many players will need to be moved to an adjacent space because a card was mined */
    private int numToMove = 0;
    public int NumToMove
    {
        get { return numToMove; }
        set { numToMove = value; }
    }

    /* The index of the first player that will need to be moved */
    public int indexToMove = -1;

    #endregion


    #endregion

    // Use this for initialization
    void Start()
    {
        gM = transform.GetComponent<GameManager>();
        bodyColor = new List<Color>();
        bodyColor.Add(new Color(1.0f, 0.0f, 0.0f));
        bodyColor.Add(new Color(0.0f, 1.0f, 0.0f));
        bodyColor.Add(new Color(0.0f, 0.0f, 1.0f));
        bodyColor.Add(new Color(0.75f, 0.75f, 0.0f));
    }

    // Update is called once per frame
    public void myUpdate()
    {

        // variable for the raycast info
        RaycastHit hit;

        //check for click on plane
        if (Input.GetMouseButtonDown(0))
        {
            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
                return;

            //our target is where we clicked
            target = hit.transform.gameObject;

            tempCard = (Card)target.GetComponent<Card>();

            switch (GameStateManager.Instance.CurrentGameState)
            {
                case GameStateManager.GameState.GAME_SETUP:
                    Debug.Log("mouse: SETUP");
                    setupClick(hit);
                    break;
                case GameStateManager.GameState.GAME_PROSPECTING_STATE:
                    Debug.Log("mouse: PROSPECTING");
                    gameClick(hit);
                    break;
                case GameStateManager.GameState.GAME_MINING_STATE:
                    Debug.Log("mouse: MINING");
                    gameClick(hit);
                    break;
                case GameStateManager.GameState.GAME_END:
                    Debug.Log("mouse: END");
                    break;
                default:
                    Debug.Log("whoops");
                    break;
            }


        }
    }

    /// <summary>
    ///  Calls helper function based on curent game state and turn
    /// </summary>
    /// <param name="hit">The click</param>
    private void gameClick(RaycastHit hit)
    {
        switch (GameStateManager.Instance.CurrentTurnState)
        {
            case GameStateManager.TurnState.TURN_MOVE:
                if (gM.gameState.CurrentGameState != GameStateManager.GameState.GAME_MINING_STATE || stakeOwnerIndex == -1)
                    moveClick(hit);
                else
                    bumpStake(hit);
                break;
            case GameStateManager.TurnState.TURN_STAKE:
                //see what phase we're in
                if (GameStateManager.Instance.CurrentGameState == GameStateManager.GameState.GAME_PROSPECTING_STATE)
                    stakeClickProspectingPhase(hit);
                else
                    stakeClickMiningPhase(hit);
                break;
            case GameStateManager.TurnState.TURN_MINE:
                if (numToMove == 0)
                    mineClick(hit);
                else
                    moveOpponent(hit);
                break;
            default:
                Debug.Log("whoops");
                break;
        }
    }

    /// <summary>
    ///  Checks if the click is valid, then calls the MovePlayer helper function
    /// </summary>
    /// <param name="hit"></param>
    private void moveClick(RaycastHit hit)
    {
        //current player position in unity coordinates
        Vector3 lastPos = gM.players[gM.CurrentPlayerIndex].transform.position;

        //loop through possible moves
        foreach (Vector2 pos in gM.moves)
        {
            if (pos.Equals(PositionToVector2(hit.transform.position)))   //check if the card clicked is a valid move
            {
                //move player to position
                Vector3 moveToLocation = new Vector3(hit.transform.position.x,
                                     hit.transform.position.y + 0.025f,
                                     tempCard.transform.position.z);
                gM.players[gM.CurrentPlayerIndex].transform.position = moveToLocation;

                gM.pEnabled = true; //player moved, set prospect to true

                //handle double click
                if (lastPos == gM.players[gM.CurrentPlayerIndex].transform.position)
                {
                    movePlayer();
                }
            }
        }
    }

    /// <summary>
    /// Helper function - Actually moves the player
    /// </summary>
    public void movePlayer()
    {
        FeedbackGUI.setText("Moving player.");

        if (gM.CurrentRoll == 1)     //save to turn this card back over again
            movedTo = tempCard;

        //update the player's grid position
        Vector2 position = PositionToVector2(gM.players[gM.CurrentPlayerIndex].transform.position);
        gM.players[gM.CurrentPlayerIndex].Position = position;

        //set the player's current card
        gM.players[gM.CurrentPlayerIndex].CurrentCard = tempCard;
        gM.players[gM.CurrentPlayerIndex].CurrentCard = gM.board[(int)position.x, (int)position.y].GetComponent<Card>();

        //send changes to server
        if (gM.isOnline)
        {
            gM.jsonFx.PerformUpdate("update_entity_pos/" + gM.players[gM.CurrentPlayerIndex].Position.x + "/" + gM.players[gM.CurrentPlayerIndex].Position.y + "/1");
            gM.jsonFx.PerformUpdate("update_card_up/1/" + gM.players[gM.CurrentPlayerIndex].CurrentCard.data.serverID);
            //gM.jsonFx.PerformUpdate("update_card_up/" + gM.CurrentPlayerIndex + "/" + gM.players[gM.CurrentPlayerIndex].CurrentCard.data.serverID);
        }

        gM.clearHighlights();   //clear the board

        //prospect
        gM.CreateMaterial(gM.players[gM.CurrentPlayerIndex].CurrentCard.data.TexCoordinate, gM.board[(int)gM.players[gM.CurrentPlayerIndex].Position.x,
                                                    (int)gM.players[gM.CurrentPlayerIndex].Position.y]);
        gM.players[gM.CurrentPlayerIndex].CurrentCard.data.isUp = true;

        gM.UpdateBars();

        //clear moves
        gM.moves.Clear();

        //if the card is staked by another player, the current player can bump it
        if (gM.gameState.CurrentGameState == GameStateManager.GameState.GAME_MINING_STATE && gM.players[gM.CurrentPlayerIndex].CurrentCard.data.staked)
        {
            FeedbackGUI.setText("This card has been staked out by somebody.");
            prepareBump();
        }
        else //otherwise move on to staking
        {
            gM.calculateStakeableCards(); // based on where the player has moved to, find the adjacent positions he/she can stake a claim

            GameStateManager.Instance.CurrentTurnState = GameStateManager.TurnState.TURN_STAKE; //go to next turn state
            if (gM.isOnline)
                gM.jsonFx.PerformUpdate("update_game_state/" + (int)gM.gameState.CurrentTurnState + "/" + gM.ID);
        }
    }

    public void prepareBump()
    {
        gM.possibleStakes.Clear();

        gM.clearHighlights();
        movedStake = false;

        //find necessary indicies
        for (int i = 0; i < gM.players.Count; i++)
        {
            if (gM.players[i].stakedCards.Contains(tempCard))
            {
                stakeOwnerIndex = i;

                //leave if it's your own stake
                if (gM.CurrentPlayerIndex == i)
                {
                    gM.clearHighlights();
                    gM.calculateStakeableCards();
                    gM.gameState.CurrentTurnState = GameStateManager.TurnState.TURN_STAKE;
                    if (gM.isOnline)
                        gM.jsonFx.PerformUpdate("update_game_state/" + (int)gM.gameState.CurrentTurnState + "/" + gM.ID);

                    stakeOwnerIndex = stakeIndex = -1;
                    movedStake = false;
                    return;
                }

                for (int n = 0; n < gM.players[i].stakedCards.Count; n++)
                {
                    if (tempCard == gM.players[i].stakedCards[n])
                        stakeIndex = n;
                }
            }
        }

        FeedbackGUI.setText("You have landed on an opponent's stake. You may move it if you so choose.");
        gM.calculateStakeableCards(new Vector2(gM.players[gM.CurrentPlayerIndex].CurrentCard.data.row, gM.players[gM.CurrentPlayerIndex].CurrentCard.data.col));
        Debug.Log("Should see stakable cards");
    }

    private void bumpStake(RaycastHit hit)
    {
        Debug.Log("In Bump stake");
        //selecting where to place a stake
        foreach (Vector2 pos in gM.possibleStakes)  // go through all possible stake locations
        {
            if (pos.Equals(PositionToVector2(hit.transform.position)) && !tempCard.data.staked) //see if card is valid and not already staked
            {
                tempStake = gM.players[stakeOwnerIndex].stakes[stakeIndex]; //save stake to move

                //free old stake
                gM.players[stakeOwnerIndex].stakedCards[stakeIndex].data.staked = false;
                gM.players[gM.CurrentPlayerIndex].CurrentCard.data.Minable = false; //don't let the current player pick this card this turn
                gM.players[stakeOwnerIndex].stakedCards.Remove(gM.players[stakeOwnerIndex].stakedCards[stakeIndex]);
                gM.players[stakeOwnerIndex].stakes.Remove(gM.players[stakeOwnerIndex].stakes[stakeIndex]);

                // move the stake
                tempStake.transform.position = tempCard.transform.position + new Vector3(0.0f, 0.01f, 0.0f);

                //mark new stake
                tempCard.data.staked = true;
                gM.players[stakeOwnerIndex].stakedCards.Add(tempCard);
                gM.players[stakeOwnerIndex].stakes.Add(tempStake);

                //after we remove and re-add, index will change
                stakeIndex = gM.players[stakeOwnerIndex].stakedCards.Count - 1;

                movedStake = true; //stake has been placed, action button should move on to the next turn phase

                lastCard = tempCard;    //sets the last card equal to the current card
                tempStake = null;
                Debug.Log("Moving stake?");
            }
        }
    }

    private void moveOpponent(RaycastHit hit)
    {
        //current player position in unity coordinates
        Vector3 lastPos = gM.players[gM.CurrentPlayerIndex].transform.position;

        //loop through possible moves
        foreach (Vector2 pos in gM.moves)
        {
            if (pos.Equals(PositionToVector2(hit.transform.position)))   //check if the card clicked is a valid move
            {
                Vector3 moveToLocation = new Vector3(hit.transform.position.x,
                                     hit.transform.position.y + 0.025f,
                                     tempCard.transform.position.z);

                //move the other player
                gM.players[indexToMove].transform.position = moveToLocation;

                //update the player's grid position
                gM.players[indexToMove].Position = PositionToVector2(gM.players[indexToMove].transform.position);

                //confirm move - double click
                if (lastPos == gM.players[indexToMove].transform.position)
                {
                    FeedbackGUI.setText("Opponent was pushed away!");
                    if (gM.isOnline)
                        gM.jsonFx.PerformUpdate("update_entity_pos/" + gM.players[indexToMove].Position.x + "/" + gM.players[indexToMove].Position.y + "/" + gM.players[indexToMove].ID);

                    numToMove--; //one less that needs to be looked at

                    //end the turn if all players are on valid spots now
                    if (numToMove <= 0)
                    {
                        gM.endTurn();
                    }
                    else
                    {
                        //someone isn't, find their index
                        for (int index = 0; index < gM.players.Count; index++)
                        {
                            if (gM.players[index].Position == new Vector2(-1, -1))
                                indexToMove = index;
                        }
                    }
                }
            }
        }
    }

    private void stakeClickProspectingPhase(RaycastHit hit)
    {
        FeedbackGUI.setText("Staking a claim! Press the Action button to confirm.");
        foreach (Vector2 pos in gM.possibleStakes)  // go through all possible stake locations (should be 5 at most)
        {
            if (pos.Equals(PositionToVector2(hit.transform.position)) && !tempCard.data.staked) //see if card is valid and not already staked
            {
                if (tempStake == null) //create a stake if they haven't placed it
                {
                    tempStake = (GameObject)Instantiate(stakePrefab, hit.transform.position + new Vector3(0.0f, 0.01f, 0.0f),   //create the stake
                                                                   Quaternion.identity);
                    tempStake.transform.renderer.material.color =               //set to current player's color
                        gM.players[gM.CurrentPlayerIndex].transform.renderer.material.color;
                    int id = gM.jsonFx.PerformUpdate("add_entity/" + pos.x + "/" + pos.y + "/0/" + gM.CurrentPlayerIndex + "/1/" + gM.ID);

                    Debug.Log("JSON ID: " + id);
                    if (id != -1)
                        tempStake.GetComponent<Stake>().ID = id;
                    else
                        Debug.Log("Something's wrong with the ID");

                }
                else //otherwise move it around
                {
                    tempStake.transform.position = hit.transform.position + new Vector3(0.0f, 0.01f, 0.0f); // move the stake
                    lastCard.data.staked = false; //mark the previously staked card as free
                    gM.players[gM.CurrentPlayerIndex].stakedCards.Remove(lastCard);
                    gM.players[gM.CurrentPlayerIndex].stakes.RemoveAt(gM.players[gM.CurrentPlayerIndex].stakes.Count - 1);
                    Debug.Log("TEST ID: " + tempStake.GetComponent<Stake>().ID);
                    if (gM.isOnline)
                        gM.jsonFx.PerformUpdate("update_entity_pos/" + pos.x + "/" + pos.y + tempStake.GetComponent<Stake>().ID);

                }

                gM.sEnabled = true; //stake has been placed, action button should move on to the next turn phase
                tempCard.data.staked = true;    // mark the currently staked card as staked

                //add stake
                gM.players[gM.CurrentPlayerIndex].stakes.Add(tempStake);
                gM.players[gM.CurrentPlayerIndex].stakedCards.Add(tempCard);

                lastCard = tempCard;    //sets the last card equal to the current card
            }
        }
    }

    private void stakeClickMiningPhase(RaycastHit hit)
    {
        FeedbackGUI.setText("Staking a claim! Press the Action button to confirm.");
        if (!selectedCard)
        {
            //selecting where to place a stake
            foreach (Vector2 pos in gM.possibleStakes)  // go through all possible stake locations (should be 5 at most)
            {
                if (pos.Equals(PositionToVector2(hit.transform.position)) && !tempCard.data.staked) //see if card is valid and not already staked
                {
                    if (tempStake == null)
                    {
                        selectedCard = true;    //player has selected a card to stake
                        lastCard = tempCard;    //save selected card
                        Debug.Log("last card 1: " + lastCard.transform.position);
                        gM.clearHighlights();   //clear board highlights
                        gM.calculateMines();    //now calculate already staked cards so a stake can be moved
                        gM.cancelStake = true;
                    }
                    else
                    {
                        tempCardIndex = gM.players[gM.CurrentPlayerIndex].stakedCards.Count - 1;

                        //free old card
                        lastCard.data.staked = false;
                        gM.players[gM.CurrentPlayerIndex].stakedCards[tempCardIndex].data.staked = false;
                        gM.players[gM.CurrentPlayerIndex].stakedCards.Remove(gM.players[gM.CurrentPlayerIndex].stakedCards[tempCardIndex]);
                        gM.players[gM.CurrentPlayerIndex].stakes.Remove(gM.players[gM.CurrentPlayerIndex].stakes[tempCardIndex]);

                        // move the stake
                        tempStake.transform.position = tempCard.transform.position + new Vector3(0.0f, 0.01f, 0.0f);

                        //mark new card
                        tempCard.data.staked = true;
                        gM.players[gM.CurrentPlayerIndex].stakedCards.Add(tempCard);
                        gM.players[gM.CurrentPlayerIndex].stakes.Add(tempStake);

                        if (gM.isOnline)
                            gM.jsonFx.PerformUpdate("update_entity_pos/" + pos.x + "/" + pos.y + tempStake.GetComponent<Stake>().ID);

                        lastCard = tempCard;    //sets the last card equal to the current card
                        gM.clearHighlights();
                        gM.calculateStakeableCards();
                        break;
                    }
                }
            }
        }
        else
        {
            //loop through number of stakes
            for (int i = 0; i < gM.players[gM.CurrentPlayerIndex].stakedCards.Count; i++)
            {
                //check if that staked card is the one they clicked
                if (gM.players[gM.CurrentPlayerIndex].stakedCards[i] == tempCard)
                {

                    tempStake = gM.players[gM.CurrentPlayerIndex].stakes[i]; //save stake to move
                    oldStakedCard = tempCard;

                    //free old stake
                    oldStakedCard = tempCard;
                    tempCardIndex = i;

                    tempCard.data.staked = false;
                    gM.players[gM.CurrentPlayerIndex].stakedCards.Remove(gM.players[gM.CurrentPlayerIndex].stakedCards[i]);
                    gM.players[gM.CurrentPlayerIndex].stakes.Remove(gM.players[gM.CurrentPlayerIndex].stakes[i]);

                    // move the stake
                    tempStake.transform.position = lastCard.transform.position + new Vector3(0.0f, 0.01f, 0.0f);

                    //mark new stake
                    lastCard.data.staked = true;
                    gM.players[gM.CurrentPlayerIndex].stakedCards.Add(lastCard);
                    gM.players[gM.CurrentPlayerIndex].stakes.Add(tempStake);

                    gM.sEnabled = true; //stake has been placed, action button should move on to the next turn phase

                    if (gM.isOnline)
                        gM.jsonFx.PerformUpdate("update_entity_pos/" + tempCard.data.row + "/" + tempCard.data.col + tempStake.GetComponent<Stake>().ID);

                    lastCard = tempCard;    //sets the last card equal to the current card
                    selectedCard = false;

                    gM.clearHighlights();
                    gM.calculateStakeableCards();
                }
            }
        }
    }

    public void resetStaking()
    {
        Debug.Log("here in reset");

        if (tempStake != null) //handle cancelling when the player has already moved the stake
        {
            //free newly staked card
            gM.players[gM.CurrentPlayerIndex].stakedCards[tempCardIndex].data.staked = false;
            gM.players[gM.CurrentPlayerIndex].stakedCards.Remove(gM.players[gM.CurrentPlayerIndex].stakedCards[tempCardIndex]);
            gM.players[gM.CurrentPlayerIndex].stakes.Remove(gM.players[gM.CurrentPlayerIndex].stakes[tempCardIndex]);
            ;

            // move the stake
            tempStake.transform.position = oldStakedCard.transform.position + new Vector3(0.0f, 0.01f, 0.0f);

            //mark old staked card as staked once again
            oldStakedCard.data.staked = true;
            gM.players[gM.CurrentPlayerIndex].stakedCards.Add(oldStakedCard);
            gM.players[gM.CurrentPlayerIndex].stakes.Add(tempStake);

            if (gM.isOnline)
                gM.jsonFx.PerformUpdate("update_entity_pos/" + oldStakedCard.data.row + "/" + oldStakedCard.data.col + "/" + tempStake.GetComponent<Stake>().ID);

        }

        selectedCard = false;
        tempStake = null;
        gM.clearHighlights();
        gM.calculateStakeableCards();
    }

    private void mineClick(RaycastHit hit)
    {
        //Debug.Log("mine click");

        for (int i = 0; i < gM.players[gM.CurrentPlayerIndex].stakedCards.Count; i++)
        {
            //check if that staked card is the one they clicked
            if (gM.players[gM.CurrentPlayerIndex].stakedCards[i] == tempCard && tempCard.data.Minable)
            {
                int row = tempCard.data.row;
                int col = tempCard.data.col;

                //clear the color of the mined card
                tempCard.transform.renderer.material.color = new Color(1, 1, 1, 1);

                //add the card to the player's hand
                gM.players[gM.CurrentPlayerIndex].hand.Add(tempCard);
                gM.UpdateBars();
                //send the board position to null
                gM.board[tempCard.data.row, tempCard.data.col] = null;

                //do not mark the card as staked any more
                tempCard.data.staked = false;

                //move the card to the side of the board
                tempCard.transform.position = findHandPosition(gM.CurrentPlayerIndex);
                if (gM.players.Count > 2 && gM.CurrentPlayerIndex == 1)
                    gM.players[gM.CurrentPlayerIndex].hand[gM.players[gM.CurrentPlayerIndex].hand.Count - 1].transform.Rotate(new Vector3(0, 1, 0), 90.0f);
                else if (gM.CurrentPlayerIndex == 3)
                    gM.players[gM.CurrentPlayerIndex].hand[gM.players[gM.CurrentPlayerIndex].hand.Count - 1].transform.Rotate(new Vector3(0, 1, 0), -90.0f);

                //remove the card from the list of staked cards
                gM.players[gM.CurrentPlayerIndex].stakedCards.Remove(tempCard);

                GameObject stake = gM.players[gM.CurrentPlayerIndex].stakes[i];

                //remove the stake from the list of stakes
                gM.players[gM.CurrentPlayerIndex].stakes.Remove(gM.players[gM.CurrentPlayerIndex].stakes[i]);

                //get rid of the stake GameObject
                Destroy(stake);

                if (gM.isOnline)
                    gM.jsonFx.PerformUpdate("update_card_mine/" + gM.CurrentPlayerIndex + "/" + tempCard.data.serverID);



                //check to see if the mined card leaves any players on an empty space
                for (int n = 0; n < gM.players.Count; n++)
                {
                    if (gM.players[n].Position == new Vector2(row, col))
                    {
                        gM.players[n].Position = new Vector2(-1, -1);
                        numToMove++;
                        if (numToMove == 1)
                        {
                            indexToMove = n;
                            gM.clearHighlights();
                            gM.calculateMoveLocations(new Vector2(row, col), 1);
                        }
                    }
                }

                //move on to the next player's turn
                if (numToMove == 0)
                    gM.endTurn();
            }
        }

        FeedbackGUI.setText("You have mined one of your staked claims.");
    }

    //Index should be
    public Vector3 findHandPosition(int index)
    {
        Debug.Log("Index: " + index);
        if (gM.players.Count == 2)
        {
            if (index == 0)
                return new Vector3((0.8f * gM.players[gM.CurrentPlayerIndex].hand.Count) - 2.0f, 0.0f, (gM.CurrentPlayerIndex * -1.1f) - 1.2f); //old
            else
                return new Vector3(12.0f - (0.8f * (gM.players[1].hand.Count - 1)), 0.0f, 4.75f);
        }
        else
        {
            if (index == 0)
                return new Vector3((0.8f * gM.players[0].hand.Count) - 2.0f, 0.0f, (gM.CurrentPlayerIndex * -1.1f) - 1.2f); //old
            else if (index == 1)
                return new Vector3(-1.5f, 0.0f, 4.75f - (gM.players[1].hand.Count * 0.8f));
            else if (index == 2)
                return new Vector3(12.0f - (0.8f * (gM.players[2].hand.Count - 1)), 0.0f, 4.75f);
            else //index == 3
                return new Vector3(12.0f, 0.0f, -1.2f + (gM.players[3].hand.Count * 0.8f));
        }

    }

    private void setupClick(RaycastHit hit)
    {
        Player holderPlayer = new Player();

        // continue to add players while we are smaller than max players
        if (gM.players.Count < gM.maxPlayers)
        {
            Vector3 pos = new Vector3(gM.board[6, 0].transform.position.x,
                                      hit.transform.position.y + 0.025f,
                                      tempCard.transform.position.z);
            //first time was don't have to check is that space is avalible
            if (gM.players.Count <= 0)
            {
                holderPlayer = (Player)Instantiate(tempPlayer, pos, Quaternion.identity);
            }
            else
            {//have to check is that space is avalible
                for (int i = 0; i < gM.players.Count; i++)
                {
                    if (pos == gM.players[i].transform.position)
                        return;
                }

                holderPlayer = (Player)Instantiate(tempPlayer, pos, Quaternion.identity);
            }
            holderPlayer.Position = PositionToVector2(pos);
            FeedbackGUI.setText("Player avatar placed.");
            if (gM.isOnline)
                gM.jsonFx.PerformUpdate("add_entity/" + holderPlayer.Position.x + "/" + holderPlayer.Position.y + "/1/" + gM.players.Count + "/1/" + gM.ID);


            //set color & add to game manager
            holderPlayer.transform.renderer.material.color = bodyColor[gM.players.Count];
            gM.players.Add(holderPlayer);
            if (gM.players.Count == gM.maxPlayers)
            {
                Debug.Log("MAX");
                GameStateManager.Instance.CurrentGameState = GameStateManager.GameState.GAME_PROSPECTING_STATE;
                if (gM.isOnline)
                    gM.jsonFx.PerformUpdate("update_game_state/" + (int)gM.gameState.CurrentGameState + "/" + gM.ID);
            }
        }
    }



    public Vector2 PositionToVector2(Vector3 pos)
    {
        return new Vector2((int)((pos.x + .0001) / .88f), (int)((pos.z + .0001) / 1.1f));
    }

    public Vector3 Vector2ToPosition(Vector2 v, float yVal)
    {
        return new Vector3(((v.x) * 0.88f), yVal, ((v.y) * 1.1f));
    }
}
