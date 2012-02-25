using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScoringSystem : MonoBehaviour {

	private ScoringRules _rules;

	// Use this for initialization
	void Start () {

	}

	public void selectSystem(ScoringRules rules) {
		_rules = rules;	
	}

    public int score(List<Card> hand)
    {
		return _rules.calculateScore(hand);	
	}

	// Update is called once per frame
	void Update () {
	
	}
}



public abstract class ScoringRules : MonoBehaviour {

    //Show winning hand for debugging
    private List<Card> winningHand = new List<Card>();
    private List<Card> tempHand1 = new List<Card>();
    private List<Card> tempHand2 = new List<Card>();

	public virtual int calculateScore(List<Card> hand) {
		//impossible score will indicate there is a problem with an actual rule system for scoring being set
		return -1;	
	}

	protected int nOfAKind(List<Card> hand) {
		Card matching = null; // card being matched
		List<Card> non_matching = new List<Card>();
		int count = 0, score = 0, highestScore = 0;

		//loop through cards in hand
		for(int i = 0; i < hand.Count; i++) {
			//make sure values are reset
			matching = null;
			non_matching.Clear();
			count = 1;
			score = 0;
			
			//check against other cards that haven't been checked against this card
			for(int j = i+1; j < hand.Count; j++) {

                tempHand1.Add(hand[i]);

				if (hand[i].data.Kind == hand[j].data.Kind) 
                {
					matching = hand[i];
					count++;
				} 
                else 
                {
					non_matching.Add(hand[j]);	
				}

				//at the end of the internal loop, score if a match was found
				if (j == hand.Count - 1 && matching != null) 
                {
					score = count * matching.data.Value;

					//add value of lowest other card
					int lowestRemaining = 1000;
					for(int k = 0; k < non_matching.Count; k++) 
                    {

						if (non_matching[k].data.Value < lowestRemaining)
							lowestRemaining = non_matching[k].data.Value;

					}

					score += lowestRemaining;

					if (score > highestScore)
						highestScore = score;
				}
			}
		}

		return highestScore;
	}



    protected int checkSuits(List<Card> hand)
    {
		int score = 0, highestScore = 0;
		char[] suits = { 'D', 'H', 'C', 'S' };

		for(int i = 0; i < 4; i++) {	//check all suits
			score = 0;
			for (int j = 0; j < hand.Count; j++) { //check all cards in hand
				if (hand[j].data.Suit == suits[i]) {
					score += hand[j].data.Value;	
				}
			}

			if (score > highestScore)
				highestScore = score;
		}
		
		return highestScore;
	}
}

public class PokerRules : ScoringRules {
    public override int calculateScore(List<Card> hand)
    {
		//todo: implement rules
		return -1;
	}
}

public class Grouping : ScoringRules {
	public override int calculateScore(List<Card> hand) 
    {
		int score1= nOfAKind(hand);
		int score2 = checkSuits(hand);
		return (score1 > score2) ? score1 : score2;
	}
}

