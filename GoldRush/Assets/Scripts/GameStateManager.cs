using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameStateManager
{
    //BEFPRE_GAME -- in menu prior to game
    //GAME_SETUP -- players select starting location //players select colors???
    //GAME_PROSPECTING_STATE -- Players can roll, move, and stake claims
    //GAME_MINING_STATE -- Players can roll, move, stake, and mine claims
    //GAME_END -- High scores; rematch
    public enum GameState { BEFORE_GAME, GAME_SETUP, GAME_PROSPECTING_STATE, GAME_MINING_STATE, GAME_END };

    //TURN_MOVE -- roll, move and reveal. 
    //TURN_STACK -- place a claim
    //TURN_MINE -- remove claimed card from board
    public enum TurnState {TURN_ROLL, TURN_MOVE, TURN_STAKE, TURN_MINE };

    /* Game Difficulty:
     *       EASY: ones show up for player permanently 
     */
    public enum GameDifficulty { EASY, NORMAL }

    private static GameStateManager instance;
    public static GameStateManager Instance
    {
        get { return instance; }
    }

    private GameState currentGameState = GameState.BEFORE_GAME;
    private TurnState currentTurnState = TurnState.TURN_ROLL;

    public GameState CurrentGameState
    {
        get { return currentGameState; }
        set { currentGameState = value; }
    }

    public TurnState CurrentTurnState
    {
        get { return currentTurnState; }
        set { currentTurnState = value; }
    }


    public GameStateManager()
    {
        instance = this;
    }
}
