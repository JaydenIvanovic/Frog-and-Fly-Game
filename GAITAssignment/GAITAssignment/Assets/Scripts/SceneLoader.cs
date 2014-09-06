using UnityEngine;
using System.Collections;

public class SceneLoader : MonoBehaviour 
{
	public string sceneName;
	public int difficultyIncrement = 0;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseDown()
	{
		// Increase the difficulty if required
		GameObject optionsGameObj = GameObject.Find("GameOptions");
		if (optionsGameObj != null) {
			GameOptions options = optionsGameObj.GetComponent<GameOptions>();
			options.difficulty += difficultyIncrement;
			options.difficulty = Mathf.Clamp(options.difficulty, 0, options.difficulties.Length - 1);
		}

		if (sceneName == "") {
			if (PlayerPrefs.HasKey("CurrentLevel")) {
				Application.LoadLevel(PlayerPrefs.GetString("CurrentLevel"));
			} else {
				// Default if we don't know where to go
				Application.LoadLevel("Menu");
			}
		} else {
			PlayerPrefs.SetString("CurrentLevel", sceneName);
			Application.LoadLevel(sceneName);
		}	
	}

	void OnMouseEnter()
	{
		transform.localScale *= 1.5f;
	}

	void OnMouseExit()
	{
		transform.localScale /= 1.5f;
	}
}
