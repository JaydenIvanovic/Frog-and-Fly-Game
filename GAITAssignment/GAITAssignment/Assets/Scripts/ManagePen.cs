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

	public List<GameObject> getFliesSortedByDistance(Vector2 position) {

		GameObject closestFly = null;
		Vector2 frogMouthPos = (Vector2)(frog.transform.FindChild("Mouth").position);
		float smallestDistance = float.MaxValue;
		float currentDistance;
		float existingDistance;

		List<GameObject> sortedFlies = new List<GameObject>();

		for (int i = 0; i < transform.childCount; i++) {

			GameObject currentFly = transform.GetChild(i).gameObject;
			currentDistance = (frogMouthPos - (Vector2)(currentFly.transform.position)).magnitude;

			for (int j = 0; j < transform.childCount; j++) {

				if (j >= sortedFlies.Count) {
					sortedFlies.Insert(j, currentFly);
					break;
				}

				existingDistance = (frogMouthPos - (Vector2)(sortedFlies[j].transform.position)).magnitude;

				if (currentDistance < existingDistance) {
					sortedFlies.Insert(j, currentFly);
					break;
				}
			}
		}

		/*
		Debug.Log("Printing distances");
		foreach (GameObject fly in sortedFlies) {
			currentDistance = (frogMouthPos - (Vector2)(fly.transform.position)).magnitude;
			Debug.Log(fly.transform.position.x + ", " + fly.transform.position.y + ": " + currentDistance);
		}
		*/

		return sortedFlies;
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
