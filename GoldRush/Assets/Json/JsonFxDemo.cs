using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DB = UnityEngine.Debug;
using DC = DebugConsole;
using JsonFx.Json;

public class Tweet {
	public System.DateTime created_at;
	public string from_user;
	public long from_user_id;
	public string from_user_id_str;
	public string from_user_name;
	public string geo;
	public long id;
	public string id_str;
	public string iso_language_code;
	public Dictionary<string, string> metadata;
	public string profile_image_url;
	public string source;
	public string text;
	public long to_user_id;
	public string to_user_id_str;
}

public class TwitterSearchResults {
	public float completed_in;
	public long max_id;
	public string max_id_str;
	public string next_page;
	public int page;
	public string query;
	public string refresh_url;
	public Tweet[] results;
	public int results_per_page;
	public long since_id;
	public string since_id_str;
}

public class JsonFxDemo : MonoBehaviour {

	public string query = "#Unity3d";

	// Use this for initialization
	void Start () {
		DC.IsOpen = true;

		StartCoroutine(PerformSearch(query));
	}

	void PrintResults(string rawJson) {
		// Raw output:
		DB.Log(DC.Log("******** raw string from Twitter ********"));
		DB.Log(DC.Log(rawJson));

		// Turn the JSON into C# objects
		var search = JsonReader.Deserialize<TwitterSearchResults>(rawJson);

		// iterate through the array of results;
		DB.Log(DC.Log("******** search results ********"));


		foreach (var tweet in search.results) {
			DB.Log(DC.Log(tweet.from_user_name + " : " + tweet.text));
		}

		DB.Log(DC.Log("******** serialize an entity ********"));

		// this turns a C# object into a JSON string.
		string json = JsonWriter.Serialize(search.results[0]);

		DB.Log(DC.Log(json));
	}
	
	IEnumerator PerformSearch(string query) {
		query = WWW.EscapeURL(query);

		using (var www = new WWW(string.Format("http://search.twitter.com/search.json?q={0}", query))) {
			while (!www.isDone) {
				yield return null;
			}

			PrintResults(www.text);
		}
	}
}
