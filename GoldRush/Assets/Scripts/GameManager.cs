using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

    private const int BOARD_WIDTH = 13;
    private const int BOARD_HEIGHT = 4;

    public GameObject Card;

    private GameObject[,] board = new GameObject[BOARD_WIDTH, BOARD_HEIGHT];

	// Use this for initialization
	void Start () {
        if (!Card)
            Debug.LogError("No card prefab set.");

        //shuffle cards
        //build board
        BuildBoard();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    private void BuildBoard(){

        for (int i = 0; i < BOARD_WIDTH; i++)
        {
            for (int j = 0; j < BOARD_HEIGHT; j++)
            {
                Vector3 pos = new Vector3(.88f* i , .5f, 1.1f * j);

                board[i, j] = (GameObject)Instantiate(Card, pos, Quaternion.identity);
            }
        }
    }
}
