using UnityEngine;
using System.Collections;

public class FeedbackGUI : MonoBehaviour
{

	private GameManager gM;

	private static FeedbackGUI instance;
	public static FeedbackGUI Instance
	{
		get
		{
			if (instance == null)
			{
				instance = new GameObject("FeedbackGUI").AddComponent<FeedbackGUI>();
			}

			return instance;
		}
	}

	/* The position and size of the Player Display */
	public Rect screenPos = new Rect(10, 10, 100, 15);
	/* The string for the Player Display */
	private static ArrayList messages = new ArrayList();

	private static Vector2 ScrollPosition;

	// Use this for initialization
	void Start()
	{
		gM = gameObject.GetComponent<GameManager>();
	}

	// Update is called once per frame
	void Update()
	{
		screenPos = new Rect(10, 10, Screen.width / 2, 175);
	}

	public static void setText(string str)
	{
		Debug.Log("you set " + str);
		messages.Add(str);
		ScrollPosition.y = Mathf.Infinity;
	}

	void OnGUI()
	{
		if (gM.gameState.CurrentGameState != GameStateManager.GameState.BEFORE_GAME)
		{
			GUILayout.BeginArea(screenPos);

			GUI.Box(new Rect(0, 0, screenPos.width, screenPos.height), "");
			ScrollPosition = GUILayout.BeginScrollView(ScrollPosition, GUILayout.Width(screenPos.width));
			GUILayout.BeginVertical();
			foreach (string message in messages)
			{
				GUILayout.Label(message);
			}
			GUILayout.EndVertical();
			GUILayout.EndScrollView();
			GUILayout.EndArea();
		}
	}

	public void OnApplicationQuit()
	{
		instance = null;
	}
}
