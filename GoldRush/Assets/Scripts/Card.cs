using UnityEngine;
using System.Collections;

public class Card {

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

    public Card(char kind, char suit)
    {
        _kind = kind;
        _suit = suit;
        
    }
}
