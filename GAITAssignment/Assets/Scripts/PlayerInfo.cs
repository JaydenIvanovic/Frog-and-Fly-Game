using UnityEngine;
using System.Collections;

// Don't attach to more than one GameObject!

[System.Serializable]
public class PlayerInfo : MonoBehaviour {

	public static int StartingHealth = 3;
	public static float InvulnerableTimeWhenHit = 2.0f;

	private static int heath;
	private static int score = 0;
	private static float invulnerableTime = 0.0f;

	public void Start() {
		heath = StartingHealth;
	}

	public static int GetHealth() {
		return heath;
	}

	public static void DecrementHealth() {
		heath = Mathf.Max(heath - 1, 0);
		// TO DO: Die when zero health!
	}

	public static void IncrementScore() {
		score++;
	}

	public static int GetScore() {
		return score;
	}

	public static bool IsInvulnerable() {
		return invulnerableTime > 0.0f;
	}

	public static void MakeInvulnerable() {
		invulnerableTime = InvulnerableTimeWhenHit;
	}

	public void Update() {
		// If currently invulnerable, decrease invulnerable time left
		invulnerableTime = Mathf.Max(invulnerableTime - Time.deltaTime, 0.0f);
	}
}
