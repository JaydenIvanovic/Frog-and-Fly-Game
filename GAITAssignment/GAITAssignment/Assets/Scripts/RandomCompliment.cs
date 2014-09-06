using UnityEngine;
using System.Collections;

public class RandomCompliment : MonoBehaviour 
{
	public string[] compliments;
	
	// Use this for initialization
	void Start () 
	{
		GameObject musicPlayer = GameObject.Find("Music");
		if (musicPlayer != null) {
			musicPlayer.transform.position = Vector3.zero;
		}

		GameObject optionsGameObj = GameObject.Find("GameOptions");
		
		// Give a different continue message if we're already on the hardest difficulty setting
		if (optionsGameObj != null) {
			GameOptions options = optionsGameObj.GetComponent<GameOptions>();
			if (options.difficulty >= (options.difficulties.Length - 1)) {
				GameObject continueButton = GameObject.Find("Continue");
				if (continueButton != null) {
					continueButton.GetComponent<TextMesh>().text = "Try that again...";
				}
			}
		}
		GetComponent<TextMesh>().text = compliments[Random.Range(0, compliments.Length)];
	}
}
