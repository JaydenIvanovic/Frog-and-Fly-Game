using UnityEngine;
using System.Collections;

public class GameOptions : MonoBehaviour
{
	public static GameOptions gameOptions;
	
	public string[] difficulties = {"Easy", "Normal", "Hard", "Insane"};
	public int difficulty;

	void Awake()
	{
		difficulty = 1;

		if(gameOptions != null)
			Destroy(this.gameObject);
		else
			gameOptions = this;
		
		DontDestroyOnLoad(this);
	}
}