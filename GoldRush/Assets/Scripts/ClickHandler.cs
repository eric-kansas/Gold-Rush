using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClickHandler : MonoBehaviour {

    private GameObject targetCard;
    private Vector3 normal;
	private GameManager gM;
	public Player tempPlayer;
    List<Color> bodyColor;

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
    }

    private void stakeClick(RaycastHit hit, Card tempCard)
    {
        Debug.Log("stake click");
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
            holderPlayer.Position = new Vector2((int)(pos.x / 0.88f)+1, (int)(pos.z / 1.1f));

            //set color & add to game manager
            holderPlayer.transform.renderer.material.color = bodyColor[gM.players.Count];
            gM.players.Add(holderPlayer);
            if (gM.players.Count == gM.maxPlayers)
            {
                GameStateManager.Instance.CurrentGameState = GameStateManager.GameState.GAME_PROSPECTING_STATE;
            }
        }
    }
}
