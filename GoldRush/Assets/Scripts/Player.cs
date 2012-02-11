using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {

    private Vector2 position = new Vector2();
    public Card[] hand;

    public Vector2 Position
    {
        get { return position; }
        set { position = value; }
    }

	// Use this for initialization
	void Start () {
        hand = new Card[5];
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
