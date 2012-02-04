using UnityEngine;
using System.Collections;

public class ClickHandler : MonoBehaviour {

    private GameObject targetCard;
    private Vector3 normal;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
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

            Card tempCard = (Card) targetCard.GetComponent<Card>();
            Debug.Log(tempCard.data.Suit + ". " + tempCard.data.Kind);

            //Camera.main.ScreenPointToRay(-normal);
            //Debug.Log("card: " + tempCard.Suit + ", " + tempCard.Value);

            //*********SEND DATA ABOUT CLICK***********//	
            //SFSObject myData = new SFSObject();		//create an object for sending data

            // data goes into the myData object
            // put x and z in the object, with string keys "x", "z"

            // then use the Send command with the smartFox object 
            // smartFox.Send(...);
        }

    }
}
