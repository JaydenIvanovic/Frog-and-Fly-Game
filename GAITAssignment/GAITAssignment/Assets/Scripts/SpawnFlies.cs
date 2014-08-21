using UnityEngine;
using System.Collections;

public class SpawnFlies : MonoBehaviour {

	public GameObject flyPrefab;
	public int numFlies;
	public float minDistanceFromPlayer = 5.0f;

	// Use this for initialization
	void Start () {
		for (int i = 0; i < numFlies; i++) {

			GameObject player = GameObject.FindGameObjectWithTag("Player");
			Vector3 spawnLocation;

			// Michael: For safety make the flies spawn away from the player
			// I got an error one time because a fly was destroyed before Flocking had initialised
			do {
				spawnLocation = new Vector3(Random.Range(-28.0f, 28.0f), Random.Range(-8.0f, 8.0f), flyPrefab.transform.position.z);
			} while ((spawnLocation - player.transform.position).magnitude < minDistanceFromPlayer);

			GameObject fly = Instantiate (flyPrefab, spawnLocation, Quaternion.identity) as GameObject;
			fly.transform.parent = gameObject.transform;
			fly.tag = "Fly";
		}
	}
}
