using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ManagePen : MonoBehaviour {
	
	public GameObject dumbFlyPrefab;
	public GameObject frog;
	public GameObject snake;

	public bool spawnFlies = true;
	public int numFlies;
	public int minFlies;
	public float minDistanceFromFrog;

	private static List<Vector3> sharedSpawnPositions;
	private static bool initialised = false;
	public int currentSpawnPosition = 0;

	public void ResetSpawnPositions(int listLen) {

		sharedSpawnPositions = new List<Vector3>();
		currentSpawnPosition = 0;

		for (int i = 0; i < listLen; i++) {
			sharedSpawnPositions.Add(GetSpawnPosition());
		}
	}

	// Use this for initialization
	void Start () {

		if (!initialised) {
			ResetSpawnPositions(100); // TO DO: Remove magic number
			initialised = true;
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

		if (spawnFlies && (flyCount < minFlies)) {
			CreateFly(transform.position + sharedSpawnPositions[currentSpawnPosition]);
			currentSpawnPosition++;
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
			spawnLocation = new Vector3(0.7f * Random.Range(-transform.localScale.x / 2, transform.localScale.x / 2),
			                            0.7f * Random.Range(-transform.localScale.y / 2, transform.localScale.y / 2),
			                            10.0f);

		} while (((Vector2)(spawnLocation + transform.position - frog.transform.position)).magnitude < minDistanceFromFrog);

		return spawnLocation;
	}
}
