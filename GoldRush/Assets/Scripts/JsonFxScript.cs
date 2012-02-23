using UnityEngine;
using System.Collections;
using JsonFx.Json;

public class JsonFxScript : MonoBehaviour {

    public string query = "game/1";

	// Use this for initialization
	void Start () {
        //StartCoroutine(PerformSearch(query));
        PrintResults("{'players':[{'friends': [], 'id': 1, 'name': 'Lulzy Guy 0'}, {'friends': [], 'id': 2, 'name': 'Lulzy Guy 1'}, {'friends': [], 'id': 3, 'name': 'Lulzy Guy 2'}, {'friends': [], 'id': 4, 'name': 'Lulzy Guy 3'}], 'hands': [{'cards': [{'id': 2, 'is_up': false, 'kind': 1, 'suit': 0}], 'id': 1}], 'id': 1, 'entities': [{'col': 3, 'id': 1, 'is_avatar': true, 'is_stake': false, 'row': 2}], 'whose_turn': {'id': 4,'name': 'Lulzy Guy 3'}}");
	}


    IEnumerator PerformSearch(string query)
    {
        query = WWW.EscapeURL(query);

        using (var www = new WWW(string.Format("typhon.csh.rit.edu/", query)))
        {
            //string.Format("typhon.csh.rit.edu/", query)
            while (!www.isDone)
            {
                yield return null;
            }

            PrintResults(www.text);
        }
    }

    void PrintResults(string rawJson)
    {
        // Raw output:
        Debug.Log("******** raw string from Twitter ********");
        Debug.Log(rawJson);

        // Turn the JSON into C# objects
        var gameJSON = JsonReader.Deserialize<JsonGame>(rawJson);

        // iterate through the array of results;
        Debug.Log("******** search results ********");

        foreach (var player in gameJSON.players)
        {
            Debug.Log("player id: " + player.id + ", player name: " + player.name);
            foreach (var friend in player.friends)
            {
                //todo
            }
        }
        foreach (var hand in gameJSON.hands)
        {
            Debug.Log("hand id: " + hand.id);
            foreach (var card in hand.cards)
            {
                Debug.Log("card id: " + card.id);
                Debug.Log("card is up: " + card.is_up);
                Debug.Log("card kind: " + card.kind);
                Debug.Log("card suit: " + card.suit);
            }

        }

        foreach (var entity in gameJSON.entities)
        {    
            Debug.Log("entity col and row: " + entity.col + ", " + entity.row);
            Debug.Log("entity.id" + " : " + entity.id);
            Debug.Log("entity bools: " + entity.is_avatar + " : " + entity.is_stake);
        }
        Debug.Log(gameJSON.whose_turn.id + " : " + gameJSON.whose_turn.name);


        Debug.Log("******** serialize an entity ********");

        // this turns a C# object into a JSON string.
        string json = JsonWriter.Serialize(gameJSON);

        Debug.Log(json);
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
