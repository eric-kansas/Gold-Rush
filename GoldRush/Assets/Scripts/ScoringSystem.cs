using UnityEngine;
using System.Collections;

public class ScoringSystem : MonoBehaviour {
	
	private ScoringRules _rules;
	
	// Use this for initialization
	void Start () {
	
	}
	
	void selectSystem(ScoringRules rules) {
		_rules = rules;	
	}
	
	int score(Card[] hand) {
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
	
	protected int nOfAKind(Card[] hand) {
		//List<Card> matching = new List<Card>(); //keep track of cards of matching kind
		//List<Card> not_matching = new List<Card>(); //keep track of non-matching cards
		
		int count = 0, score = 0, highestScore = 0;
		for(int i = 0; i < hand.Length; i++) {
			count = 0;
			score = 0;
			for(int j = i; j < hand.Length; j++) {
				if (hand[i].Kind == hand[j].Kind) {
					count++;
					//score += hand[i].Value;
					if (score > highestScore)
						highestScore = score;
				}
			}
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
		
		return -1;
	}
}
