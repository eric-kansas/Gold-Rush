using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClickHandler : MonoBehaviour {

    private GameObject targetCard;
    private Vector3 normal;
	private GameManager gM;
	public Player tempPlayer;
    List<Color> bodyColor;
	GameObject tempStake;
	private Card tempCard;
	
	private Card lastCard;
	
	public GameObject stakePrefab;
	
	
	public Card TempCard
	{
		get {return tempCard;}
	}
	
    // Use this for initialization
    void Start()
    {
		gM = transform.GetComponent<GameManager>();
        bodyColor = new List<Color>();
        bodyColor.Add(new Color(1.0f, 0.0f, 0.0f, 0.7f));
        bodyColor.Add(new Color(0.0f, 1.0f, 0.0f, 0.7f));
        bodyColor.Add(new Color(0.0f, 0.0f, 1.0f, 0.7f));
        bodyColor.Add(new Color(0.75f, 0.75f, 0.0f, 0.7f));
    }

    // Update is called once per frame
    public void myUpdate()
    {

        // variable for the raycast info
        RaycastHit hit;
		Card tempCard;
		
        //check for click on plane
        if (Input.GetMouseButtonDown(0))
        {
            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
                return;

            //our target is where we clicked
            targetCard = hit.transform.gameObject;

            tempCard = (Card) targetCard.GetComponent<Card>();
            Debug.Log(tempCard.data.Suit + ". " + tempCard.data.Kind);

            switch (GameStateManager.Instance.CurrentGameState)
            {
                case GameStateManager.GameState.GAME_SETUP: 
                    Debug.Log("mouse: SETUP");
                    setupClick(hit, tempCard);
                    break;
                case GameStateManager.GameState.GAME_PROSPECTING_STATE:
                    Debug.Log("game state: PROSPECTING"); 
                    prospectingClick(hit, tempCard);
                    break;
                case GameStateManager.GameState.GAME_MINING_STATE: 
                    Debug.Log("game state: MINING");
                    break;
                case GameStateManager.GameState.GAME_END: 
                    Debug.Log("game state: END");
                    break;
                default: Debug.Log("whoops"); break;
            }

			
        }
    }

    private void prospectingClick(RaycastHit hit, Card tempCard)
    {
        Debug.Log("prospecting click");
        switch (GameStateManager.Instance.CurrentTurnState)
        {
            case GameStateManager.TurnState.TURN_MOVE:
				tempStake = null; 
                moveClick(hit, tempCard);
                break;
            case GameStateManager.TurnState.TURN_STAKE:
                stakeClick(hit, tempCard);
                break;
            default: Debug.Log("whoops"); break;
        }
    }

 private void moveClick(RaycastHit hit, Card tempCard)
    {
        Debug.Log("move click");
		
		Vector3 lastPos = gM.players[gM.CurrentPlayerIndex].transform.position;
		
        foreach (Vector2 pos in gM.moves)
        {
            if(pos.Equals(PositionToVector2(hit.transform.position)))
            {
                Debug.Log(pos + " and " + PositionToVector2(hit.transform.position));
                Debug.Log("HEREHERHEHR");
                Vector3 moveToLocation = new Vector3(hit.transform.position.x,
                                     hit.transform.position.y + 0.25f,
                                     tempCard.transform.position.z);
                gM.players[gM.CurrentPlayerIndex].transform.position = moveToLocation;
				gM.pEnabled = true; //player moved, set prospect to true
				if(lastPos == gM.players[gM.CurrentPlayerIndex].transform.position)
				{
					GameStateManager.Instance.CurrentTurnState = GameStateManager.TurnState.TURN_STAKE;
					
					
					gM.players[gM.CurrentPlayerIndex].CurrentCard = tempCard;
					
					gM.players[gM.CurrentPlayerIndex].Position = PositionToVector2(gM.players[gM.CurrentPlayerIndex].transform.position);
					
					Debug.Log(hit.transform.gameObject.GetComponent<Card>().data.row + ", " +
					          hit.transform.gameObject.GetComponent<Card>().data.col);
					
					int bW = gM.getBoardWidth();
					int bH = gM.getBoardHeight();
					
					for (int i = 0; i < bW; i++)
			        {
			            for (int j = 0; j < bH; j++)
			            {
							 gM.board[i, j].transform.renderer.material.color = new Color(1,1,1,1);
						}
					}

                    gM.CreateMaterial(gM.players[gM.CurrentPlayerIndex].CurrentCard.data.TexCoordinate, gM.board[(int)gM.players[gM.CurrentPlayerIndex].Position.x,
                                                                (int)gM.players[gM.CurrentPlayerIndex].Position.y]);
					
					gM.calculateStakeableCards();
				}
            }
        }
    }

    private void stakeClick(RaycastHit hit, Card tempCard)
    {
        Debug.Log("stake click");
		
		foreach (Vector2 pos in gM.possibleStakes)
        {
			if(pos.Equals(PositionToVector2(hit.transform.position)) && !tempCard.data.staked)
            {
				
				if(tempStake == null)
				{
					tempStake = (GameObject)Instantiate(stakePrefab, hit.transform.position + new Vector3(0.0f, 0.01f, 0.0f),
					                                               Quaternion.identity);
					tempStake.transform.renderer.material.color = 
						gM.players[gM.CurrentPlayerIndex].transform.renderer.material.color;
					Debug.Log(gM.CurrentPlayerIndex);

					gM.sEnabled = true; //stake has been placed, action button should move on to the next turn phase
				}
				else
				{
					tempStake.transform.position = hit.transform.position + new Vector3(0.0f, 0.01f, 0.0f);
					lastCard.data.staked = false;
					gM.sEnabled = false;
				}	
				
				tempCard.data.staked = true;
				
				int bW = gM.getBoardWidth();
				int bH = gM.getBoardHeight();
				
				lastCard = tempCard;
			}
		}
    }

    private void setupClick(RaycastHit hit, Card tempCard)
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
            Debug.Log("huh: " + holderPlayer.Position.x);

            //set color & add to game manager
            holderPlayer.transform.renderer.material.color = bodyColor[gM.players.Count];
            gM.players.Add(holderPlayer);
            if (gM.players.Count == gM.maxPlayers)
            {
                GameStateManager.Instance.CurrentGameState = GameStateManager.GameState.GAME_PROSPECTING_STATE;
            }
        }
    }

    public Vector2 PositionToVector2(Vector3 pos)
    {
        Debug.Log("huh: " +pos.x);


        return new Vector2((int)((pos.x +.0001) / .88f) , (int)((pos.z +.0001) / 1.1f));
    }
}
