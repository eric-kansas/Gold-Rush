using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using JsonFx.Json;



public class JsonGame
{
    public JsonEntity[] entities;
    public JsonHand[] hands;
    public JsonWhoseTurn whose_turn;
    public JsonCard[] cards;
    public JsonPlayer[] players;
    public int current_player;
    public int current_roll;
    public int game_state;
    public int game_turn;

	public DateTime gameStarted;
	public DateTime lastPlay;
	public JsonPlayer winner;
	public Dictionary<Player, int> scores;
}

