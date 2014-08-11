using UnityEngine;
using System.Collections;

public class InsultDict : MonoBehaviour 
{
	public string[] insults;

	// Use this for initialization
	void Start () 
	{
		GetComponent<TextMesh>().text = insults[Random.Range(0, insults.Length)];
	}
}
