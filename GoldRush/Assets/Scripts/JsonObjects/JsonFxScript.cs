using UnityEngine;
using System.Collections;
using JsonFx.Json;

public class JsonFxScript : MonoBehaviour {

    public string query = "game/1";
	private static string host = "typhon.csh.rit.edu:9052/";
	public static int latestID;
	public static JsonFxScript instance;

    public JsonGame gameJSON;

	// Use this for initialization
	void Start () {
        //StartCoroutine(PerformSearch(query));
        //PrintResults("{'cards': [{'col': 0, 'game_id': 1, 'id': 1, 'in_game_id': -1, 'is_up': false, 'kind': 0, 'minable': true, 'row': 0, 'suit': 'C', 'type': 'tg_card'}, {'col': 1, 'game_id': 1, 'id': 2, 'in_game_id': -1, 'is_up': false, 'kind': 1, 'minable': true, 'row': 0, 'suit': 'C', 'type': 'tg_card'}, {'col': 2, 'game_id': 1, 'id': 3, 'in_game_id': -1, 'is_up': false, 'kind': 2, 'minable': true, 'row': 0, 'suit': 'C', 'type': 'tg_card'}, {'col': 3, 'game_id': 1, 'id': 4, 'in_game_id': -1, 'is_up': false, 'kind': 3, 'minable': true, 'row': 0, 'suit': 'C', 'type': 'tg_card'}, {'col': 4, 'game_id': 1, 'id': 5, 'in_game_id': -1, 'is_up': false, 'kind': 4, 'minable': true, 'row': 0, 'suit': 'C', 'type': 'tg_card'}, {'col': 5, 'game_id': 1, 'id': 6, 'in_game_id': -1, 'is_up': false, 'kind': 5, 'minable': true, 'row': 0, 'suit': 'C', 'type': 'tg_card'}, {'col': 6, 'game_id': 1, 'id': 7, 'in_game_id': -1, 'is_up': false, 'kind': 6, 'minable': true, 'row': 0, 'suit': 'C', 'type': 'tg_card'}, {'col': 7, 'game_id': 1, 'id': 8, 'in_game_id': -1, 'is_up': false, 'kind': 7, 'minable': true, 'row': 0, 'suit': 'C', 'type': 'tg_card'}, {'col': 8, 'game_id': 1, 'id': 9, 'in_game_id': -1, 'is_up': false, 'kind': 8, 'minable': true, 'row': 0, 'suit': 'C', 'type': 'tg_card'}, {'col': 9, 'game_id': 1, 'id': 10, 'in_game_id': -1, 'is_up': false, 'kind': 9, 'minable': true, 'row': 0, 'suit': 'C', 'type': 'tg_card'}, {'col': 10, 'game_id': 1, 'id': 11, 'in_game_id': -1, 'is_up': false, 'kind': 10, 'minable': true, 'row': 0, 'suit': 'C', 'type': 'tg_card'}, {'col': 11, 'game_id': 1, 'id': 12, 'in_game_id': -1, 'is_up': false, 'kind': 11, 'minable': true, 'row': 0, 'suit': 'C', 'type': 'tg_card'}, {'col': 12, 'game_id': 1, 'id': 13, 'in_game_id': -1, 'is_up': false, 'kind': 12, 'minable': true, 'row': 0, 'suit': 'C', 'type': 'tg_card'}, {'col': 0, 'game_id': 1, 'id': 14, 'in_game_id': -1, 'is_up': false, 'kind': 0, 'minable': true, 'row': 1, 'suit': 'D', 'type': 'tg_card'}, {'col': 1, 'game_id': 1, 'id': 15, 'in_game_id': -1, 'is_up': false, 'kind': 1, 'minable': true, 'row': 1, 'suit': 'D', 'type': 'tg_card'}, {'col': 2, 'game_id': 1, 'id': 16, 'in_game_id': -1, 'is_up': false, 'kind': 2, 'minable': true, 'row': 1, 'suit': 'D', 'type': 'tg_card'}, {'col': 3, 'game_id': 1, 'id': 17, 'in_game_id': -1, 'is_up': false, 'kind': 3, 'minable': true, 'row': 1, 'suit': 'D', 'type': 'tg_card'}, {'col': 4, 'game_id': 1, 'id': 18, 'in_game_id': -1, 'is_up': false, 'kind': 4, 'minable': true, 'row': 1, 'suit': 'D', 'type': 'tg_card'}, {'col': 5, 'game_id': 1, 'id': 19, 'in_game_id': -1, 'is_up': false, 'kind': 5, 'minable': true, 'row': 1, 'suit': 'D', 'type': 'tg_card'}, {'col': 6, 'game_id': 1, 'id': 20, 'in_game_id': -1, 'is_up': false, 'kind': 6, 'minable': true, 'row': 1, 'suit': 'D', 'type': 'tg_card'}, {'col': 7, 'game_id': 1, 'id': 21, 'in_game_id': -1, 'is_up': false, 'kind': 7, 'minable': true, 'row': 1, 'suit': 'D', 'type': 'tg_card'}, {'col': 8, 'game_id': 1, 'id': 22, 'in_game_id': -1, 'is_up': false, 'kind': 8, 'minable': true, 'row': 1, 'suit': 'D', 'type': 'tg_card'}, {'col': 9, 'game_id': 1, 'id': 23, 'in_game_id': -1, 'is_up': false, 'kind': 9, 'minable': true, 'row': 1, 'suit': 'D', 'type': 'tg_card'}, {'col': 10, 'game_id': 1, 'id': 24, 'in_game_id': -1, 'is_up': false, 'kind': 10, 'minable': true, 'row': 1, 'suit': 'D', 'type': 'tg_card'}, {'col': 11, 'game_id': 1, 'id': 25, 'in_game_id': -1, 'is_up': false, 'kind': 11, 'minable': true, 'row': 1, 'suit': 'D', 'type': 'tg_card'}, {'col': 12, 'game_id': 1, 'id': 26, 'in_game_id': -1, 'is_up': false, 'kind': 12, 'minable': true, 'row': 1, 'suit': 'D', 'type': 'tg_card'}, {'col': 0, 'game_id': 1, 'id': 27, 'in_game_id': -1, 'is_up': false, 'kind': 0, 'minable': true, 'row': 2, 'suit': 'H', 'type': 'tg_card'}, {'col': 1, 'game_id': 1, 'id': 28, 'in_game_id': -1, 'is_up': false, 'kind': 1, 'minable': true, 'row': 2, 'suit': 'H', 'type': 'tg_card'}, {'col': 2, 'game_id': 1, 'id': 29, 'in_game_id': -1, 'is_up': false, 'kind': 2, 'minable': true, 'row': 2, 'suit': 'H', 'type': 'tg_card'}, {'col': 3, 'game_id': 1, 'id': 30, 'in_game_id': -1, 'is_up': false, 'kind': 3, 'minable': true, 'row': 2, 'suit': 'H', 'type': 'tg_card'}, {'col': 4, 'game_id': 1, 'id': 31, 'in_game_id': -1, 'is_up': false, 'kind': 4, 'minable': true, 'row': 2, 'suit': 'H', 'type': 'tg_card'}, {'col': 5, 'game_id': 1, 'id': 32, 'in_game_id': -1, 'is_up': false, 'kind': 5, 'minable': true, 'row': 2, 'suit': 'H', 'type': 'tg_card'}, {'col': 6, 'game_id': 1, 'id': 33, 'in_game_id': -1, 'is_up': false, 'kind': 6, 'minable': true, 'row': 2, 'suit': 'H', 'type': 'tg_card'}, {'col': 7, 'game_id': 1, 'id': 34, 'in_game_id': -1, 'is_up': false, 'kind': 7, 'minable': true, 'row': 2, 'suit': 'H', 'type': 'tg_card'}, {'col': 8, 'game_id': 1, 'id': 35, 'in_game_id': -1, 'is_up': false, 'kind': 8, 'minable': true, 'row': 2, 'suit': 'H', 'type': 'tg_card'}, {'col': 9, 'game_id': 1, 'id': 36, 'in_game_id': -1, 'is_up': false, 'kind': 9, 'minable': true, 'row': 2, 'suit': 'H', 'type': 'tg_card'}, {'col': 10, 'game_id': 1, 'id': 37, 'in_game_id': -1, 'is_up': false, 'kind': 10, 'minable': true, 'row': 2, 'suit': 'H', 'type': 'tg_card'}, {'col': 11, 'game_id': 1, 'id': 38, 'in_game_id': -1, 'is_up': false, 'kind': 11, 'minable': true, 'row': 2, 'suit': 'H', 'type': 'tg_card'}, {'col': 12, 'game_id': 1, 'id': 39, 'in_game_id': -1, 'is_up': false, 'kind': 12, 'minable': true, 'row': 2, 'suit': 'H', 'type': 'tg_card'}, {'col': 0, 'game_id': 1, 'id': 40, 'in_game_id': -1, 'is_up': false, 'kind': 0, 'minable': true, 'row': 3, 'suit': 'S', 'type': 'tg_card'}, {'col': 1, 'game_id': 1, 'id': 41, 'in_game_id': -1, 'is_up': false, 'kind': 1, 'minable': true, 'row': 3, 'suit': 'S', 'type': 'tg_card'}, {'col': 2, 'game_id': 1, 'id': 42, 'in_game_id': -1, 'is_up': false, 'kind': 2, 'minable': true, 'row': 3, 'suit': 'S', 'type': 'tg_card'}, {'col': 3, 'game_id': 1, 'id': 43, 'in_game_id': -1, 'is_up': false, 'kind': 3, 'minable': true, 'row': 3, 'suit': 'S', 'type': 'tg_card'}, {'col': 4, 'game_id': 1, 'id': 44, 'in_game_id': -1, 'is_up': false, 'kind': 4, 'minable': true, 'row': 3, 'suit': 'S', 'type': 'tg_card'}, {'col': 5, 'game_id': 1, 'id': 45, 'in_game_id': -1, 'is_up': false, 'kind': 5, 'minable': true, 'row': 3, 'suit': 'S', 'type': 'tg_card'}, {'col': 6, 'game_id': 1, 'id': 46, 'in_game_id': -1, 'is_up': false, 'kind': 6, 'minable': true, 'row': 3, 'suit': 'S', 'type': 'tg_card'}, {'col': 7, 'game_id': 1, 'id': 47, 'in_game_id': -1, 'is_up': false, 'kind': 7, 'minable': true, 'row': 3, 'suit': 'S', 'type': 'tg_card'}, {'col': 8, 'game_id': 1, 'id': 48, 'in_game_id': -1, 'is_up': false, 'kind': 8, 'minable': true, 'row': 3, 'suit': 'S', 'type': 'tg_card'}, {'col': 9, 'game_id': 1, 'id': 49, 'in_game_id': -1, 'is_up': false, 'kind': 9, 'minable': true, 'row': 3, 'suit': 'S', 'type': 'tg_card'}, {'col': 10, 'game_id': 1, 'id': 50, 'in_game_id': -1, 'is_up': false, 'kind': 10, 'minable': true, 'row': 3, 'suit': 'S', 'type': 'tg_card'}, {'col': 11, 'game_id': 1, 'id': 51, 'in_game_id': -1, 'is_up': false, 'kind': 11, 'minable': true, 'row': 3, 'suit': 'S', 'type': 'tg_card'}, {'col': 12, 'game_id': 1, 'id': 52, 'in_game_id': -1, 'is_up': false, 'kind': 12, 'minable': true, 'row': 3, 'suit': 'S', 'type': 'tg_card'}], 'current_player': 0, 'current_roll': 5, 'entities': [{'col': 3, 'game': {'current_player': 0, 'current_roll': 5, 'game_state': 2, 'game_turn': 1, 'id': 1, 'type': 'game'}, 'id': 1, 'in_game_id': 0, 'is_avatar': true, 'row': 2, 'type': 'entity'}, {'col': 3, 'game': {'current_player': 0, 'current_roll': 5, 'game_state': 2, 'game_turn': 1, 'id': 1, 'type': 'game'}, 'id': 2, 'in_game_id': 1, 'is_avatar': true, 'row': 3, 'type': 'entity'}, {'col': 3, 'game': {'current_player': 0, 'current_roll': 5, 'game_state': 2, 'game_turn': 1, 'id': 1, 'type': 'game'}, 'id': 3, 'in_game_id': 2, 'is_avatar': true, 'row': 4, 'type': 'entity'}, {'col': 3, 'game': {'current_player': 0, 'current_roll': 5, 'game_state': 2, 'game_turn': 1, 'id': 1, 'type': 'game'}, 'id': 4, 'in_game_id': 3, 'is_avatar': true, 'row': 5, 'type': 'entity'}], 'game_state': 2, 'game_turn': 1, 'id': 1, 'players': [{'id': 1, 'name': 'Lulzy Guy 0', 'next_turn_in': [], 'type': 'player'}, {'id': 2, 'name': 'Lulzy Guy 1', 'next_turn_in': [], 'type': 'player'}, {'id': 3, 'name': 'Lulzy Guy 2', 'next_turn_in': [], 'type': 'player'}, {'id': 4, 'name': 'Lulzy Guy 3', 'next_turn_in': [{'current_player': 0, 'current_roll': 5, 'game_state': 2, 'game_turn': 1, 'id': 1, 'type': 'game'}], 'type': 'player'}], 'type': 'game', 'whose_turn': {'id': 4, 'name': 'Lulzy Guy 3', 'next_turn_in': [{'current_player': 0, 'current_roll': 5, 'game_state': 2, 'game_turn': 1, 'id': 1, 'type': 'game'}], 'type': 'player'}}");
		instance = this;
		PerformSearch("game/1");

	}

	public int PerformUpdate(string query)
	{
		// query = WWW.EscapeURL(query);
		JsonFxScript.instance.StartCoroutine(PerformRealUpdate(query));
		return latestID;
	}

	private IEnumerator PerformRealUpdate(string query)
	{
		using (var www = new WWW(host + query))
		{
			//string.Format("typhon.csh.rit.edu/", query)
			while (!www.isDone)
			{
				yield return null;
			}

			Debug.Log("here: " + host + query);

			int.TryParse(www.text, out latestID);
		}

		JsonFxScript.instance.StartCoroutine(PerformSearch("game/1"));
	}

    IEnumerator PerformSearch(string query)
    {
        using (var www = new WWW(host + query))
        {
            //string.Format("typhon.csh.rit.edu/", query)
            while (!www.isDone)
            {
                yield return null;
            }

			string rawJson = www.text;
			rawJson = rawJson.Replace("u'", "'");
			rawJson = rawJson.Replace("True", "true");
			rawJson = rawJson.Replace("False", "false");

			PrintResults(rawJson);
        }
    }

    void PrintResults(string rawJson)
    {
        // Raw output:
        Debug.Log("******** raw string from Twitter ********");
        Debug.Log(rawJson);

        // Turn the JSON into C# objects
        gameJSON = JsonReader.Deserialize<JsonGame>(rawJson);

        // iterate through the array of results;
        Debug.Log("******** search results ********");

        Debug.Log("Players: " + gameJSON.players.Length);
        Debug.Log("Entities: " + gameJSON.entities.Length);
        Debug.Log("Cards: " + gameJSON.entities.Length);

        /*
        foreach (var player in gameJSON.players)
        {
            //Debug.Log("player id: " + player.id + ", player name: " + player.name);
            
        }

        foreach (var entity in gameJSON.entities)
        {
            Debug.Log("entity row and col: " + entity.row + ", " + entity.col);
            Debug.Log("entity.id" + " : " + entity.id);
            Debug.Log("entity owners: " + entity.player_id + ", " + entity.game_id);
        }
        Debug.Log(gameJSON.whose_turn.id + " : " + gameJSON.whose_turn.name);


        Debug.Log("******** serialize an entity ********");
        
        // this turns a C# object into a JSON string.
        string json = JsonWriter.Serialize(gameJSON);

        Debug.Log(json);
         */
    }

	public int findPlayerJsonIndex(int in_game_index)
	{
		for( int i = 0; i < gameJSON.entities.Length; i++)
		{
			if (gameJSON.entities[i].in_game_id == in_game_index)
				return i;
		}
		return gameJSON.entities.Length;
	}

    void createGame()
    {

    }

	// Update is called once per frame
	void Update () {
	
	}
}
