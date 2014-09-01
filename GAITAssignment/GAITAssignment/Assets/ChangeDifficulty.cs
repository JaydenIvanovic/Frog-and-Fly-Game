using UnityEngine;
using System.Collections;

public class ChangeDifficulty : MonoBehaviour {

	public int increment;
	public GameObject otherArrow;

	private TextMesh textMesh;
	private GameOptions options;
	
	void Awake() {
		textMesh = GameObject.Find("DifficultyDisplay").GetComponent<TextMesh>();
		options = GameObject.Find("GameOptions").GetComponent<GameOptions>();
	}
	
	// Use this for initialization
	void Start () {
		textMesh.text = "Difficulty: " + options.difficulties[options.difficulty];
	}
	
	void OnMouseDown()
	{
		options.difficulty = options.difficulty + increment;

		if ((options.difficulty <= 0) || (options.difficulty >= (options.difficulties.Length - 1))) {
			options.difficulty = Mathf.Clamp(options.difficulty, 0, options.difficulties.Length - 1);
			gameObject.SetActive(false);
		}

		textMesh.text = "Difficulty: " + options.difficulties[options.difficulty];

		otherArrow.SetActive(true);
	}
}
