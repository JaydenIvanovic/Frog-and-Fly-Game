using UnityEngine;
using System.Collections;

public class SceneLoader : MonoBehaviour 
{
	public string sceneName;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseDown()
	{
		Application.LoadLevel (sceneName);
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
