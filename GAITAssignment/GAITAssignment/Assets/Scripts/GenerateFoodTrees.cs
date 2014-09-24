using UnityEngine;
using System.Collections;

public class GenerateFoodTrees : MonoBehaviour 
{
	public int numAreas;
	public int minGroupSize;
	public float minxBoundary, maxxBoundary;
	public float minyBoundary, maxyBoundary;
	public GameObject appleTreePrefab, flowerTreePrefab;
	public GameObject home;
	public float requiredDistanceFromHome, distanceFromNeighbours;

	// Randomly place the apple and flower trees on the map
	// so the player doesn't have existing knowledge of there
	// location.
	void Start() 
	{
		for (int i = 0; i < numAreas; ++i) {
			Vector3 lastTreePos = Vector3.zero;
			bool lastTreePosInitialised = false;

			for (int j = 0; j < minGroupSize; ++j) {
				
				Vector3 newTreePos;
				if (!lastTreePosInitialised) {
					// Generate and test approach. Find a random position and see if it meets our requirement.
					do {
						newTreePos = new Vector3(Random.Range(minxBoundary, maxxBoundary+1), Random.Range(minyBoundary, maxyBoundary+1), 0f);
					} while (Vector3.Distance(home.transform.position, newTreePos) < requiredDistanceFromHome);
					Debug.Log(newTreePos);
				} else {
					// We want to find a tree close to the last tree.
					newTreePos = new Vector3(Random.Range(lastTreePos.x - 2, lastTreePos.x - 2), Random.Range(lastTreePos.y - 2, lastTreePos.y - 2), 0f);
				}

				// Randomly choose between an apple and flower tree.
				if (Random.value < 0.5) {
					GameObject tree = (GameObject)Instantiate(appleTreePrefab, newTreePos, Quaternion.identity);
					tree.transform.parent = transform;
				} else {
					GameObject tree = (GameObject)Instantiate(flowerTreePrefab, newTreePos, Quaternion.identity);
					tree.transform.parent = transform;
				}

				lastTreePos = newTreePos;
				lastTreePosInitialised = true;
			}

		}
	}

}
