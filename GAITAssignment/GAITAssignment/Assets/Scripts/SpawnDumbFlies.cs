using UnityEngine;
using System.Collections;

public class SpawnDumbFlies : MonoBehaviour {
	
	public GameObject dumbFlyPrefab;
	public GameObject frog;
	public GameObject snake;

	public int numFlies;
	public int minFlies;
	public float minDistanceFromFrog;
	
	// Use this for initialization
	void Start () {
		
		// Create flies
		for (int i = 0; i < numFlies; i++) {
			CreateFly(GetSpawnPosition());
		}
	}

	public GameObject getClosestFly(Vector2 position) {

		GameObject closestFly = null;
		float smallestDistance = float.MaxValue;
		float currentDistance;

		foreach (Transform child in transform)
		{
			if (child.gameObject != null) {

				currentDistance = (position - (Vector2)(child.position)).magnitude;

				if (currentDistance < smallestDistance) {
					closestFly = child.gameObject;
					smallestDistance = currentDistance;
				}
			}
		}

		return closestFly;
	}
	
	void Update () {

		int flyCount = 0;
		foreach (Transform child in transform) {
			if (child.gameObject.tag == "Fly") {
				flyCount++;
			}
		}

		if (flyCount < minFlies) {
			CreateFly(GetSpawnPosition());
		}
	}
	
	private void CreateFly(Vector3 position) {
		
		GameObject fly = Instantiate (dumbFlyPrefab, position, Quaternion.identity) as GameObject;
		fly.transform.parent = gameObject.transform;
		fly.tag = "Fly";
	}
	
	private Vector3 GetSpawnPosition() {
		
		Vector3 spawnLocation;

		// Make the flies spawn away from the frog
		do {
			spawnLocation = new Vector3(Random.Range(transform.position.x - transform.localScale.x / 2, transform.position.x + transform.localScale.x / 2),
			                            Random.Range(transform.position.y - transform.localScale.y / 2, transform.position.y + transform.localScale.y / 2),
			                            10.0f);

		} while (((Vector2)(spawnLocation - frog.transform.position)).magnitude < minDistanceFromFrog);

		return spawnLocation;
	}
}
