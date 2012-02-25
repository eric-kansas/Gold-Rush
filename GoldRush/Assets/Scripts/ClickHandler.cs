using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClickHandler : MonoBehaviour
{

    #region Properties
    /* The card the player clicked on */
    private GameObject targetCard;

    /* An imaginary line coming out of the card */
    private Vector3 normal;

    /* GameManager - handles game logic */
    private GameManager gM;

    /* A list of colors that the players can be. */
    public List<Color> bodyColor;

    /* Temporary objects */
    public Player tempPlayer;            // A newly created player before it is added to the players list
    GameObject tempStake;                // Temporary stake that can be moved around the board before the player locks it in
    private Card lastCard;              // The last card the player staked, which isn't yet locked in (previous click)
    private Card tempCard;              //Current clicked card
    private int tempCardIndex;

    /* Stake asset */
    public GameObject stakePrefab;

    /* Whether or not the player has selected a card to stake (in mining phase) */
    public bool selectedCard = false;

    private Card oldStakedCard;

    /* How many players will need to be moved to an adjacent space because a card was mined */
    public int numToMove = 0;

    /* The index of the first player that will need to be moved */
    public int indexToMove = -1;

    /* The index of the player owning the stake that will be bumped */
    public int stakeOwnerIndex = -1;

    /* The index of that stake */
    public int stakeIndex = -1;

    /* Whether the stake has been bumped yet */
    public bool movedStake;

    /*Card the player just moved to, to set facedown if they rolled a one */
    public Card movedTo = null;

    public GameObject TempStake
    {
        get { return tempStake; }
        set { tempStake = value; }
    }

    public Card TempCard
    {
        get { return tempCard; }
        set { tempCard = value; }
    }

    #endregion


    // Use this for initialization
    void Start()
    {
        gM = transform.GetComponent<GameManager>();
        bodyColor = new List<Color>();
        bodyColor.Add(new Color(1.0f, 0.0f, 0.0f, 0.5f));
        bodyColor.Add(new Color(0.0f, 1.0f, 0.0f, 0.5f));
        bodyColor.Add(new Color(0.0f, 0.0f, 1.0f, 0.5f));
        bodyColor.Add(new Color(0.75f, 0.75f, 0.0f, 0.5f));
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
            targetCard = hit.transform.gameObject;

            tempCard = (Card)targetCard.GetComponent<Card>();

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
                default: Debug.Log("whoops"); break;
            }


        }
    }

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
            default: Debug.Log("whoops"); break;
        }
    }

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
                                     hit.transform.position.y + 0.25f,
                                     tempCard.transform.position.z);
                gM.players[gM.CurrentPlayerIndex].transform.position = moveToLocation;

                gM.pEnabled = true; //player moved, set prospect to true

                //handle double click
                if (lastPos == gM.players[gM.CurrentPlayerIndex].transform.position)
                {
                    //save to turn this card back over again
                    if (gM.CurrentRoll == 1)
                        movedTo = tempCard;

                    gM.players[gM.CurrentPlayerIndex].CurrentCard = tempCard;//set the player's current card

                    gM.players[gM.CurrentPlayerIndex].Position = PositionToVector2(gM.players[gM.CurrentPlayerIndex].transform.position);   //update the player's grid position

                    gM.clearHighlights();   //clear the board

                    //prospect
                    gM.CreateMaterial(gM.players[gM.CurrentPlayerIndex].CurrentCard.data.TexCoordinate, gM.board[(int)gM.players[gM.CurrentPlayerIndex].Position.x,
                                                                (int)gM.players[gM.CurrentPlayerIndex].Position.y]);
                    //if the card is staked by another player, the current player can bump it
                    if (tempCard.data.staked)
                    {
                        prepareBump();
                    }
                    else //otherwise move on to staking
                    {
                        gM.calculateStakeableCards(); // based on where the player has moved to, find the adjacent positions he/she can stake a claim

                        GameStateManager.Instance.CurrentTurnState = GameStateManager.TurnState.TURN_STAKE; //go to next turn state
                    }
                }
            }
        }
    }

    public void prepareBump()
    {
        Debug.Log("Staked out already!");
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

        gM.calculateStakeableCards(new Vector2(tempCard.data.row, tempCard.data.col));
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
                                     hit.transform.position.y + 0.25f,
                                     tempCard.transform.position.z);

                //move the other player
                gM.players[indexToMove].transform.position = moveToLocation;

                //update the player's grid position
                gM.players[indexToMove].Position = PositionToVector2(gM.players[indexToMove].transform.position);

                //confirm move - double click
                if (lastPos == gM.players[indexToMove].transform.position)
                {
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
                    Debug.Log(gM.CurrentPlayerIndex);
                }
                else //otherwise move it around
                {
                    tempStake.transform.position = hit.transform.position + new Vector3(0.0f, 0.01f, 0.0f); // move the stake
                    lastCard.data.staked = false; //mark the previously staked card as free
                    gM.players[gM.CurrentPlayerIndex].stakedCards.Remove(lastCard);
                    gM.players[gM.CurrentPlayerIndex].stakes.RemoveAt(gM.players[gM.CurrentPlayerIndex].stakes.Count - 1);
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
                        /*
                        for (int i = 0; i < gM.numProspectingTurns; i++)
                        {
                            //check if that staked card is the one they clicked
                            if (gM.players[gM.CurrentPlayerIndex].stakedCards[i] == tempCard)
                            {
                                tempCardIndex = i;
                            }
                        }
                         */
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
            gM.players[gM.CurrentPlayerIndex].stakes.Remove(gM.players[gM.CurrentPlayerIndex].stakes[tempCardIndex]); ;

            // move the stake
            tempStake.transform.position = oldStakedCard.transform.position + new Vector3(0.0f, 0.01f, 0.0f);

            //mark old staked card as staked once again
            oldStakedCard.data.staked = true;
            gM.players[gM.CurrentPlayerIndex].stakedCards.Add(oldStakedCard);
            gM.players[gM.CurrentPlayerIndex].stakes.Add(tempStake);
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
                gM.board[row, col].transform.renderer.material.color = new Color(1, 1, 1, 1);

                //add the card to the player's hand
                gM.players[gM.CurrentPlayerIndex].hand.Add(tempCard);

                //send the board position to null
                gM.board[tempCard.data.row, tempCard.data.col] = null;

                //do not mark the card as staked any more
                tempCard.data.staked = false;

                //move the card to the side of the board
                tempCard.transform.position = findHandPosition();

                //remove the card from the list of staked cards
                gM.players[gM.CurrentPlayerIndex].stakedCards.Remove(tempCard);

                GameObject stake = gM.players[gM.CurrentPlayerIndex].stakes[i];

                //remove the stake from the list of stakes
                gM.players[gM.CurrentPlayerIndex].stakes.Remove(gM.players[gM.CurrentPlayerIndex].stakes[i]);

                //get rid of the stake GameObject
                Destroy(stake);

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
    }

    private Vector3 findHandPosition()
    {
        if (gM.players.Count == 2)
        {
            if (gM.CurrentPlayerIndex == 0)
                return new Vector3((0.8f * gM.players[gM.CurrentPlayerIndex].hand.Count) - 2.0f, 0.0f, (gM.CurrentPlayerIndex * -1.1f) - 1.2f); //old
            else
                return new Vector3(12.0f - (0.8f * (gM.players[1].hand.Count - 1)), 0.0f, 4.75f);
        }
        else
        {
            if (gM.CurrentPlayerIndex == 0)
                return new Vector3((0.8f * gM.players[gM.CurrentPlayerIndex].hand.Count) - 2.0f, 0.0f, (gM.CurrentPlayerIndex * -1.1f) - 1.2f); //old
            else if (gM.CurrentPlayerIndex == 1) { }
            else if (gM.CurrentPlayerIndex == 2)
                return new Vector3(12.0f - (0.8f * (gM.players[1].hand.Count - 1)), 0.0f, 4.75f);
            else if (gM.CurrentPlayerIndex == 3) { }
        }

        return new Vector3((0.8f * gM.players[gM.CurrentPlayerIndex].hand.Count) - 2.0f, 0.0f, (gM.CurrentPlayerIndex * -1.1f) - 1.2f); //old

    }

    private void setupClick(RaycastHit hit)
    {
        Player holderPlayer = new Player();

        // continue to add players while we are smaller than max players
        if (gM.players.Count < gM.maxPlayers)
        {
            Vector3 pos = new Vector3(gM.board[6, 0].transform.position.x,
                                      hit.transform.position.y + 0.25f,
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
            //Debug.Log("huh: " + holderPlayer.Position.x);

            //set color & add to game manager
            holderPlayer.transform.renderer.material.color = bodyColor[gM.players.Count];
            gM.players.Add(holderPlayer);
            if (gM.players.Count == gM.maxPlayers)
            {
                Debug.Log("MAX");
                GameStateManager.Instance.CurrentGameState = GameStateManager.GameState.GAME_PROSPECTING_STATE;
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
