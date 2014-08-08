using UnityEngine;
using System.Collections;

[System.Serializable]
public class PlayerInfo : MonoBehaviour {

	private static int score = 0;

	public static void IncrementScore() {
		score++;
	}

	public static int GetScore() {
		return score;
	}
}
