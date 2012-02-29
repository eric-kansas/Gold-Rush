using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using JsonFx.Json;



public class JsonGame
{
	public int id;
    public JsonEntity[] entities;
    public JsonCard[] cards;
    public JsonPlayer[] players;
    public int current_player;
    public int current_roll;
    public int game_state;
    public int game_turn;

	public DateTime gameStarted;
	public DateTime lastPlay;
	public JsonPlayer winner;
	public Dictionary<JsonPlayer, int> scores;
}

