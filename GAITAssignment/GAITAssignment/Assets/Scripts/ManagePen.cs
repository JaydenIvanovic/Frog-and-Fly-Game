using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ManagePen : MonoBehaviour {
	
	public GameObject dumbFlyPrefab;
	public GameObject frog;
	public List<GameObject> snakes;
	public GameObject frogHome;
	public GameObject flySpawnPoint;
	public Transform obstaclesParent;

	public bool spawnFlies = true;
	public int numFlies;
	public int minFlies;
	public float minDistanceFromFrog;

	private int obstacleLayerNum;
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

		obstacleLayerNum = LayerMask.NameToLayer("Obstacles");
	}

	public PriorityQueue<float, GameObject> getObstaclesSortedByDistance(Vector2 position) {

		PriorityQueue<float, GameObject> pq = new PriorityQueue<float, GameObject>();

		for (int i = 0; i < obstaclesParent.childCount; i++) {
			
			if (obstaclesParent.GetChild(i).gameObject.layer == obstacleLayerNum) {

				GameObject currentObstacle = obstaclesParent.GetChild(i).gameObject;
				CircleCollider2D currentCollider = currentObstacle.GetComponent<CircleCollider2D>();
				float distance = (position - (Vector2)(currentObstacle.transform.position)).magnitude - currentCollider.radius;
				pq.Add(new KeyValuePair<float, GameObject>(distance, currentObstacle));
			}
		}
		
		return pq;
	}

	// TO DO: Make this like the above? Although insertion sort might be ok for small lists
	public List<GameObject> getFliesSortedByDistance(Vector2 position) {

		float currentDistance;
		float existingDistance;

		List<GameObject> sortedFlies = new List<GameObject>();

		for (int i = 0; i < transform.childCount; i++) {

			if (transform.GetChild(i).tag == "Fly") {

				GameObject currentFly = transform.GetChild(i).gameObject;
				currentDistance = (position - (Vector2)(currentFly.transform.position)).magnitude;

				for (int j = 0; j < transform.childCount; j++) {

					if (j >= sortedFlies.Count) {
						sortedFlies.Insert(j, currentFly);
						break;
					}

					existingDistance = (position - (Vector2)(sortedFlies[j].transform.position)).magnitude;

					if (currentDistance < existingDistance) {
						sortedFlies.Insert(j, currentFly);
						break;
					}
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

	public List<GameObject> getSnakesSortedByDistance(Vector2 position) {

		float currentDistance;
		float existingDistance;
		
		List<GameObject> sortedSnakes = new List<GameObject>();
		
		for (int i = 0; i < transform.childCount; i++) {

			if (transform.GetChild(i).tag == "Predator") {
			
				GameObject currentSnake = transform.GetChild(i).gameObject;
				currentDistance = (position - (Vector2)(currentSnake.transform.position)).magnitude;
				
				for (int j = 0; j < transform.childCount; j++) {
					
					if (j >= sortedSnakes.Count) {
						sortedSnakes.Insert(j, currentSnake);
						break;
					}
					
					existingDistance = (position - (Vector2)(sortedSnakes[j].transform.position)).magnitude;
					
					if (currentDistance < existingDistance) {
						sortedSnakes.Insert(j, currentSnake);
						break;
					}
				}
			}
		}
		
		return sortedSnakes;
	}
	
	void Update () {

		int flyCount = 0;
		foreach (Transform child in transform) {
			if (child.gameObject.tag == "Fly") {
				flyCount++;
			}
		}

		if (spawnFlies && (flyCount < minFlies)) {
			CreateFly(flySpawnPoint.transform.position + sharedSpawnPositions[currentSpawnPosition]);
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
			spawnLocation = new Vector3(Random.Range(-flySpawnPoint.transform.localScale.x, flySpawnPoint.transform.localScale.x),
			                            Random.Range(-flySpawnPoint.transform.localScale.y, flySpawnPoint.transform.localScale.y),
			                            10.0f);

		} while (((Vector2)(spawnLocation + flySpawnPoint.transform.position - frog.transform.position)).magnitude < minDistanceFromFrog);

		return spawnLocation;
	}
}
