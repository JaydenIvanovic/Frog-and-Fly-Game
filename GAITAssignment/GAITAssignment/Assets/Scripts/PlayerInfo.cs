﻿using UnityEngine;
using System.Collections;

// Don't attach to more than one GameObject!

[System.Serializable]
public class PlayerInfo : MonoBehaviour {

	private static string deathScreen = "DeathSplash";

	public static int StartingHealth = 3;
	public static float InvulnerableTimeWhenHit = 2.0f;

	private static int eggsDestroyed;
	private static int health;
	private static int score;
	private static float waterLevel;
	private static float invulnerableTime;
	private static bool underwater;

	public void Start() {
		health = StartingHealth;
		score = 0;
		eggsDestroyed = 0;
		invulnerableTime = 0.0f;
		waterLevel = 100f;
	}

	public static int GetHealth() {
		return health;
	}

	public static int GetEggsDestroyed() {
		return eggsDestroyed;
	}

	public static float GetWaterLevel() {
		return waterLevel;
	}

	public static void DecrementHealth() {
		health = Mathf.Max(health - 1, 0);
		// TO DO: Die when zero health!
		if(health == 0) {
			Application.LoadLevel (deathScreen);
		}
	}

	public static void IncrementScore() {
		score++;
	}

	public static void IncrementEggs() {
		eggsDestroyed++;
	}

	public static void SetUnderwater(bool isUnderwater) {
		underwater = isUnderwater;
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

	public static bool IsUnderwater() {
		return underwater;
	}

	public void Update() {
		// If currently invulnerable, decrease invulnerable time left
		invulnerableTime = Mathf.Max(invulnerableTime - Time.deltaTime, 0.0f);

		if(underwater) {
			waterLevel = Mathf.Min(waterLevel + Time.deltaTime * 5, 100.0f);
		}
		else {
			waterLevel = Mathf.Max(waterLevel - Time.deltaTime * 2, 0.0f);
			if(waterLevel <= 0) {
				DecrementHealth();
				if(Random.Range(0, 2) == 0)
					transform.position = GameObject.Find("Pond_Left").transform.position;
				else
					transform.position = GameObject.Find("Pond_Right").transform.position;
			}
		}
	}
}
