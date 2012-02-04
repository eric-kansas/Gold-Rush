using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClickHandler : MonoBehaviour {

    private GameObject targetCard;
    private Vector3 normal;
	private GameManager gM;
	public Player tempPlayer;
	
    // Use this for initialization
    void Start()
    {
		gM = transform.GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {

        // variable for the raycast info
        RaycastHit hit;
		Card tempCard = new Card();
		
        //check for click on plane
        if (Input.GetMouseButtonDown(0))
        {
            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100))
                return;

            //our target is where we clicked
            targetCard = hit.transform.gameObject;

            tempCard = (Card) targetCard.GetComponent<Card>();
            Debug.Log(tempCard.data.Suit + ". " + tempCard.data.Kind);

            //Camera.main.ScreenPointToRay(-normal);
            //Debug.Log("card: " + tempCard.Suit + ", " + tempCard.Value);

            //*********SEND DATA ABOUT CLICK***********//	
            //SFSObject myData = new SFSObject();		//create an object for sending data

            // data goes into the myData object
            // put x and z in the object, with string keys "x", "z"

            // then use the Send command with the smartFox object 
            // smartFox.Send(...);
			
			
			Player holderPlayer = new Player();
			List<Color> bodyColor = new List<Color>();
			bodyColor.Add(new Color(1.0f, 0.0f, 0.0f));
			bodyColor.Add(new Color(0.0f, 1.0f, 0.0f));
			bodyColor.Add(new Color(0.0f, 0.0f, 1.0f));
			bodyColor.Add(new Color(0.75f, 0.75f, 0.0f));
			
			if(gM.players.Count < gM.maxPlayers)
			{
				Vector3 pos = new Vector3(gM.board[6, 0].transform.position.x, 
				                          hit.transform.position.y + 0.25f, 
				                          tempCard.transform.position.z);
				
				if(gM.players.Count <= 0)
				{
					holderPlayer = (Player)Instantiate(tempPlayer, pos, Quaternion.identity);
				}
				else
				{
					for(int i = 0; i < gM.players.Count;i++)
					{
						if(pos == gM.players[i].transform.position)
							return;
					}
				
					holderPlayer = (Player)Instantiate(tempPlayer, pos, Quaternion.identity);
				}
				
				holderPlayer.transform.renderer.material.color = bodyColor[gM.players.Count];
				gM.players.Add(holderPlayer);
			}
        }
		
		

    }
}
