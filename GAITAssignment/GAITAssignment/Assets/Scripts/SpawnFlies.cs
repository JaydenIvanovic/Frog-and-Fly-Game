using UnityEngine;
using System.Collections;

public class SpawnFlies : MonoBehaviour {

	public GameObject flyPrefab;
	public int numFlies;

	// Use this for initialization
	void Start () {
		for (int i = 0; i < numFlies; i++) {
			GameObject fly = Instantiate (flyPrefab, new Vector3(Random.Range(-28.0f, 28.0f), Random.Range(-8.0f, 8.0f), flyPrefab.transform.position.z), Quaternion.identity) as GameObject;
			fly.transform.parent = gameObject.transform;
		}
	}
}
