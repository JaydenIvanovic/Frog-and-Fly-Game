using UnityEngine;
using System.Collections;

// Don't attach to more than one GameObject!

[System.Serializable]
public class PlayerInfo : MonoBehaviour {

	private string deathScreen = "DeathSplash";

	public int StartingHealth = 3;
	public float InvulnerableTimeWhenHit = 2.0f;
	public AudioClip HurtSound;
	public AudioClip EatSound;
	public AudioClip SplashSound;

	private int eggsDestroyed;
	private int health;
	private int score;
	private float waterLevel;
	private float invulnerableTime;
	private bool isUnderwater;
	private AudioSource SoundSource;

	void Awake()
	{
		SoundSource = gameObject.AddComponent<AudioSource>();
		SoundSource.loop = false;
	}

	public void Start() {
		health = StartingHealth;
		score = 0;
		eggsDestroyed = 0;
		invulnerableTime = 0.0f;
		waterLevel = 100f;
	}

	public int GetHealth() {
		return health;
	}

	public int GetEggsDestroyed() {
		return eggsDestroyed;
	}

	public float GetWaterLevel() {
		return waterLevel;
	}

	public void DecrementHealth() {

		health = Mathf.Max(health - 1, 0);
		if(health == 0) {
			Application.LoadLevel (deathScreen);
		} else {
			SoundSource.clip = HurtSound;
			SoundSource.Play();
		}
	}

	public void IncrementScore() {
		score++;
		GameObject.Find("Tongue").GetComponent<Animator>().SetTrigger("Eating");
		SoundSource.clip = EatSound;
		SoundSource.Play();
	}

	public void IncrementEggs() {
		eggsDestroyed++;
	}

	public void SetUnderwater(bool isUnderwater) {

		if (this.isUnderwater != isUnderwater) {
			SoundSource.clip = SplashSound;
			SoundSource.Play();
		}

		this.isUnderwater = isUnderwater;
	}

	public int GetScore() {
		return score;
	}

	public bool IsInvulnerable() {
		return invulnerableTime > 0.0f;
	}

	public void MakeInvulnerable() {
		invulnerableTime = InvulnerableTimeWhenHit;
	}

	public bool IsUnderwater() {
		return isUnderwater;
	}

	public void Update() {
		// If currently invulnerable, decrease invulnerable time left
		invulnerableTime = Mathf.Max(invulnerableTime - Time.deltaTime, 0.0f);

		if (isUnderwater) {
			waterLevel = Mathf.Min(waterLevel + Time.deltaTime * 5, 100.0f);
		} else {
			waterLevel = Mathf.Max(waterLevel - Time.deltaTime * 2, 0.0f);
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
		}
	}
}
