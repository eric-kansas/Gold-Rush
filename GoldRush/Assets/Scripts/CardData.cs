using UnityEngine;
using System.Collections;

public class CardData {
	
	public int row;
	public int col;
	public bool staked;
	
    private char _suit;
    public char Suit{
        get { return _suit; }
        set { _suit = value; }
    }

    private char _kind;
    public char Kind{
        get { return _kind; }
        set { _kind = value; }
    }
	
	private int _value;
	public int Value {
		get { return _value; }
		set { _value = value; }
	}

    private Vector2 _texCoordinate;
    public Vector2 TexCoordinate
    {
        get { return _texCoordinate; }
        set { _texCoordinate = value; }
    }

    public CardData(char kind, char suit, int val, Vector2 coordinate)
    {
        _kind = kind;
        _suit = suit;
		_value = val;
        _texCoordinate = coordinate;
		staked = false;
    }
}