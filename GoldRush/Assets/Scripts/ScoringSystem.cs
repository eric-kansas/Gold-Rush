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

	public int score(Card[] hand) {
		return _rules.calculateScore(hand);	
	}

	// Update is called once per frame
	void Update () {
	
	}
}



public abstract class ScoringRules : MonoBehaviour {

	public virtual int calculateScore(Card[] hand) {
		//impossible score will indicate there is a problem with an actual rule system for scoring being set
		return -1;	
	}

	protected int check(Card[] hand) {
		List<List<Card>> tests = new List<List<Card>>();
		List<List<int>> scores = new List<List<int>>();
		
		int i;
		for(i = 0; i < 4; i++) //add four lists for suits
			tests.Add(new List<Card>());
		
		for(i = 0; i < hand.Length; i++) {
			
			//check suits
			switch (hand[i].Suit) {
				case 'D':
					tests[0].Add(hand[i]);
					break;
				case 'H':
					tests[1].Add(hand[i]);
					break;
				case 'C':
					tests[2].Add(hand[i]);
					break;
				case 'S':
					tests[3].Add(hand[i]);
					break;
			}
			
			//check matches
			bool added = false;
			if (tests.Count > 4) {
				for(int j = 4; j < tests.Count; j++) {
					if (tests[j][0].Kind == hand[i].Kind) {
						tests[j].Add(hand[i]);	
					} else if (j = tests.Count) {
						tests.add(new List<Card>());
						tests[tests.Count - 1].Add(hand[i]);
					}
				}
			}
		}

		int highestScore = 0, score = 0;
		for(i = 0; i < tests.Count; i++) {
			for(int j = 0; j < tests[i].Count; j++) {
				if (i < 4) { //suits
					score += tests[i][j].Value;
				} else {
					score += tests[i][j].Value;
					//todo - add lowest
					
				}
			}
		}
	}

	protected int nOfAKind(Card[] hand) {
		Card matching = null; // card being matched
		List<Card> non_matching = new List<Card>();
		int count = 0, score = 0, highestScore = 0;

		//loop through cards in hand
		for(int i = 0; i < hand.Length; i++) {
			//make sure values are reset
			matching = null;
			non_matching.Clear();
			count = 0;
			score = 0;
			
			//check against other cards that haven't been checked against this card
			for(int j = i+1; j < hand.Length; j++) {
				if (hand[i].Kind == hand[j].Kind) {
					matching = hand[i];
					count++;
				} else {
					non_matching.Add(hand[j]);	
				}

				//at the end of the internal loop, score if a match was found
				if (j == hand.Length && matching != null) {
					score = count * matching.Value;

					//add value of lowest other card
					int lowestRemaining = 1000;
					for(int k = 0; k < non_matching.Count; k++) {
						if (non_matching[k].Value < lowestRemaining)
							lowestRemaining = non_matching[k].Value;
					}

					score += lowestRemaining;

					if (score > highestScore)
						highestScore = score;
				}
			}
		}

		return highestScore;
	}

	

	protected int checkSuits(Card[] hand) {
		int score = 0, highestScore = 0;
		char[] suits = { 'D', 'H', 'C', 'S' };

		for(int i = 0; i < 4; i++) {	//check all suits
			score = 0;
			for (int j = 0; j < hand.Length; j++) { //check all cards in hand
				if (hand[j].Suit == suits[i]) {
					score += hand[j].Value;	
				}
			}

			if (score > highestScore)
				highestScore = score;
		}
		
		return highestScore;
	}
}

public class PokerRules : ScoringRules {
	public override int calculateScore(Card[] hand) {
		//todo: implement rules
		return -1;
	}
}

public class Grouping : ScoringRules {
	public override int calculateScore(Card[] hand) {
		int score1= nOfAKind(hand);
		int score2 = checkSuits(hand);
		return (score1 > score2) ? score1 : score2;
	}
}

