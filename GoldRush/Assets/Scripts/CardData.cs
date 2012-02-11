using UnityEngine;
using System.Collections;

public class CardData {
	
	public int row;
	public int col;
	
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

    public CardData(char kind, char suit, int val)
    {
        _kind = kind;
        _suit = suit;
		_value = val;
    }
}