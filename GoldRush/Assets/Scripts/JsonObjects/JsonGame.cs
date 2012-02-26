using UnityEngine;
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
}

