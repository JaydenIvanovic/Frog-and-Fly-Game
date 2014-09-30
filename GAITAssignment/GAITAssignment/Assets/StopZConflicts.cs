using UnityEngine;
using System.Collections;

public class StopZConflicts : MonoBehaviour {

	void Awake () {

		foreach (Transform child in transform)
		{
			child.transform.position = new Vector3(child.transform.position.x,
			                                       child.transform.position.y,
			                                       1.0f + 0.0001f * child.transform.position.y + 0.000001f * child.transform.position.x);

			Debug.Log("Did stuff");
		}
	}
}
