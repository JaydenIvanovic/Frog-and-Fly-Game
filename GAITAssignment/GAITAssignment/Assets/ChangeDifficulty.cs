using UnityEngine;
using System.Collections;

public class ChangeDifficulty : MonoBehaviour {
	
	private TextMesh textMesh;
	private GameOptions options;
	
	void Awake() {
		textMesh = GetComponent<TextMesh>();
		options = GameObject.Find("GameOptions").GetComponent<GameOptions>();
	}
	
	// Use this for initialization
	void Start () {
		textMesh.text = "Difficulty: " + options.difficulties[options.difficulty];
	}
	
	void OnMouseDown()
	{
		options.difficulty = (options.difficulty + 1) % options.difficulties.Length;
		textMesh.text = "Difficulty: " + options.difficulties[options.difficulty];
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
