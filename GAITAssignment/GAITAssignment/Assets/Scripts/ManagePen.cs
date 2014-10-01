using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ManagePen : MonoBehaviour {
	
	public GameObject dumbFlyPrefab;
	public GameObject frog;

	[HideInInspector]
	public List<GameObject> snakes;

	public GameObject frogHome;
	public GameObject flySpawnPoint;
	public List<Transform> obstaclesParents;
	public List<Transform> lakes;
	public Transform playerFliesParent;

	public bool spawnFlies = true;
	public bool flyMovement = true;
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

		foreach (Transform obstaclesParent in obstaclesParents) {

			for (int i = 0; i < obstaclesParent.childCount; i++) {
				
				if (obstaclesParent.GetChild(i).gameObject.layer == obstacleLayerNum) {

					GameObject currentObstacle = obstaclesParent.GetChild(i).gameObject;
					CircleCollider2D currentCollider = currentObstacle.GetComponent<CircleCollider2D>();
					float distance = (position - (Vector2)(currentObstacle.transform.position)).magnitude - currentCollider.radius;
					pq.Add(new KeyValuePair<float, GameObject>(distance, currentObstacle));
				}
			}
		}
		
		return pq;
	}

	public PriorityQueue<float, GameObject> getLakeMarkersSortedByDistance(Vector2 position) {
		
		PriorityQueue<float, GameObject> pq = new PriorityQueue<float, GameObject>();
		
		foreach (Transform lake in lakes) {
			
			for (int i = 0; i < lake.childCount; i++) {
				
				if (lake.GetChild(i).name == "PondMarker") {
					
					GameObject currentLake = lake.GetChild(i).gameObject;
					float distance = (position - (Vector2)(currentLake.transform.position)).magnitude;
					pq.Add(new KeyValuePair<float, GameObject>(distance, currentLake));
				}
			}
		}
		
		return pq;
	}

	// TO DO: Make this like the above? Although insertion sort might be ok for small lists
	public List<GameObject> getFliesSortedByDistance(Vector2 position) {

		float currentDistance;
		float existingDistance;

		List<GameObject> sortedFlies = new List<GameObject>();

		Transform fliesParent = playerFliesParent;

		if (fliesParent == null) {
			fliesParent = transform;
		}

		for (int i = 0; i < fliesParent.childCount; i++) {

			if (fliesParent.GetChild(i).tag == "Fly") {

				GameObject currentFly = fliesParent.GetChild(i).gameObject;
				currentDistance = (position - (Vector2)(currentFly.transform.position)).magnitude;

				for (int j = 0; j < fliesParent.childCount; j++) {

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

	public PriorityQueue<float, GameObject> getSnakesSortedByDistance(Vector2 position) {

		PriorityQueue<float, GameObject> pq = new PriorityQueue<float, GameObject>();
		
		for (int i = 0; i < transform.childCount; i++) {

			if (transform.GetChild(i).tag == "Predator") {
				GameObject currentSnake = transform.GetChild(i).gameObject;
				float bubbleTimeLeft = Mathf.Max(0.0f, currentSnake.GetComponent<PredatorStateMachine>().bubbleTimeLeft);
				float bubbleAdjustment = bubbleTimeLeft * frog.GetComponent<Movement>().speed;
				float currentDistance = (position - (Vector2)(currentSnake.transform.position)).magnitude + bubbleAdjustment;
				pq.Add(new KeyValuePair<float, GameObject>(currentDistance, currentSnake));
			}
		}
		
		return pq;
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
		fly.GetComponent<Movement>().speed = flyMovement ? 3.0f : 0.0f;
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
