using UnityEngine;
using System.Collections;

// Don't attach to more than one GameObject!

[System.Serializable]
public class PlayerInfo : MonoBehaviour {

	private static string deathScreen = "DeathSplash";
	private static string winScreen = "Win";

	public static float BUBBLE_COST = 20.0f;

	public static bool isPaused = false;

	public int StartingHealth = 3;
	public float InvulnerableTimeWhenHit = 2.0f;
	public float InvulnerableFlickerFrequency = 8.0f;
	public AudioClip HurtSound;
	public AudioClip EatSound;
	public AudioClip SplashSound;
	public float WaterLossRate = 2.0f;
	public float WaterRefillRate = 10.0f;

	private static int snakesDrowned;
	private static int eggsDestroyed;
	private static int health;
	private static int score;
	private static int requiredFlies;
	private static float waterLevel;
	private static float invulnerableTime;
	private static bool _isUnderwater;

	private AStarTargeter targeter;
	private static AudioSource SoundSource;

	// For flickering
	private static Animator animator;
	private static SpriteRenderer spriteRenderer;
	private static Animator tongueAnimator;
	private static SpriteRenderer tongueSpriteRenderer;
	
	// Static copies
	private static float _InvulnerableTimeWhenHit;
	private static AudioClip _HurtSound;
	private static AudioClip _EatSound;
	private static AudioClip _SplashSound;

	void Awake()
	{
		targeter = GetComponent<AStarTargeter>();

		SoundSource = gameObject.AddComponent<AudioSource>();
		SoundSource.loop = false;

		// So we can access from static functions... Ugly but it works
		_InvulnerableTimeWhenHit = InvulnerableTimeWhenHit;
		_HurtSound = HurtSound;
		_EatSound = EatSound;
		_SplashSound = SplashSound;

		// Set animators & sprite renderers
		animator = GetComponent<Animator>();
		spriteRenderer = GetComponent<SpriteRenderer>();

		Animator[] animators = GetComponentsInChildren<Animator>();
		foreach (Animator a in animators) {
			if (a.gameObject.tag == "Tongue") {
				tongueAnimator = a;
			}
		}

		SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
		foreach (SpriteRenderer sr in renderers) {
			if (sr.gameObject.tag == "Tongue") {
				tongueSpriteRenderer = sr;
			}
		}

		// Difficulty settings
		int difficulty = 1;
		GameObject optionsGameObj = GameObject.Find("GameOptions");
		
		// We won't be able to find the options GameObject if we haven't gone via the main menu
		if (optionsGameObj != null) {
			GameOptions options = optionsGameObj.GetComponent<GameOptions>();
			difficulty = options.difficulty;
		}
		
		switch (difficulty) {
		case 0:
			// Easy
			requiredFlies = 10;
			break;
		case 1:
		default:
			// Normal
			requiredFlies = 15;
			break;
		case 2:
			// Hard
			requiredFlies = 20;
			break;
		case 3:
			// Insane
			requiredFlies = 20;
			break;
		}
	}

	public void Start() {
		health = StartingHealth;
		score = 0;
		eggsDestroyed = 0;
		snakesDrowned = 0;
		invulnerableTime = 0.0f;
		waterLevel = 100f;
		_isUnderwater = false;
	}

	public static int GetHealth() {
		return health;
	}

	public static int GetEggsDestroyed() {
		return eggsDestroyed;
	}

	public static int GetSnakesDrowned() {
		return snakesDrowned;
	}

	public static float GetWaterLevel() {
		return waterLevel;
	}

	public static void DecrementHealth() {

		health = Mathf.Max(health - 1, 0);
		SoundSource.clip = _HurtSound;
		SoundSource.Play();

		if(health == 0) {
			Application.LoadLevel (deathScreen);
		}
	}

	public static void IncrementScore() {

		score++;
		tongueAnimator.SetTrigger("Eating");
		SoundSource.clip = _EatSound;
		SoundSource.Play();

		if (score >= requiredFlies) {
			Application.LoadLevel (winScreen);
		}
	}

	public static void IncrementEggs() {
		eggsDestroyed++;
	}

	public static void IncrementSnakesDrowned() {
		snakesDrowned++;
	}

	public static void ReduceWaterAfterBubble() {
		waterLevel -= BUBBLE_COST;
	}

	public static void SetUnderwater(bool isUnderwater) {

		if (_isUnderwater!= isUnderwater) {
			SoundSource.clip = _SplashSound;
			SoundSource.Play();
		}

		_isUnderwater = isUnderwater;
	}

	public static int GetScore() {
		return score;
	}

	public static int GetRequiredFlies() {
		return requiredFlies;
	}

	public static bool IsInvulnerable() {
		return invulnerableTime > 0.0f;
	}

	public static void MakeInvulnerable() {
		invulnerableTime = _InvulnerableTimeWhenHit;
	}

	public static bool IsUnderwater() {
		return _isUnderwater;
	}

	public void OnTriggerStay2D(Collider2D other) 
	{
		// Try to ensure we don't get stuck ramming against an obstacle
		if (LayerMask.LayerToName(other.gameObject.layer) == "Obstacles") {
			
			AStarTargeter ast = GetComponent<AStarTargeter>();
			
			if (ast != null) {
				ast.ForceRecalculate();
			}
		}
	}

	public void Update() {

		// Defaults
		spriteRenderer.enabled = true;
		tongueSpriteRenderer.enabled = true;
		
		// Flicker when invulnerable
		if (PlayerInfo.IsInvulnerable()) {
			if (((int)(Time.unscaledTime * InvulnerableFlickerFrequency * 2.0f)) % 2 == 0) {
				spriteRenderer.enabled = false;
				tongueSpriteRenderer.enabled = false;
			}
		}

		// Hide tongue if underwater
		if (_isUnderwater) {
			tongueSpriteRenderer.enabled = false;
			animator.SetBool("Underwater", true);
		} else {
			animator.SetBool("Underwater", false);
		}

		// Sitting or walking
		Vector2? target = targeter.GetTarget();
		if (target != null) {
			animator.SetBool("Sitting", false);
		} else {
			animator.SetBool("Sitting", true);
		}

		// Make the music follow the player (you get a weird panning effect otherwise)
		GameObject musicPlayer = GameObject.Find("Music");
		if (musicPlayer != null) {
			musicPlayer.transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, 0.0f);
		}

		// If currently invulnerable, decrease invulnerable time left
		invulnerableTime = Mathf.Max(invulnerableTime - Time.deltaTime, 0.0f);

		if (_isUnderwater) {
			waterLevel = Mathf.Min(waterLevel + Time.deltaTime * WaterRefillRate, 100.0f);
		} else {
			waterLevel = Mathf.Max(waterLevel - Time.deltaTime * WaterLossRate, 0.0f);

			// You don't lose health now, you just can't shoot any more bubbles
			/*
			if(waterLevel <= 0) {
				DecrementHealth();
				Vector3 pondPos;
				if(Random.Range(0, 2) == 0)
					pondPos = GameObject.Find("Pond_Left").transform.position;
				else
					pondPos = GameObject.Find("Pond_Right").transform.position;
				transform.position = new Vector3(pondPos.x, pondPos.y, transform.position.z);
				Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);
			}
			*/
		}
	}
}
