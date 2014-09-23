using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlyPlayerInfo : MonoBehaviour 
{
	private float resource1;
	private float resource2;
	private float score;
	private List<GameObject> flies;
	private List<GameObject> resourceObjects;
	public GameObject resource;
	public float interactionDistance;
	public float scoringInteractionDistance;
	public int resourceIncrement;
	public int scoringIncrementModifier;
	public int maxResource;
	public GameObject scoringLocation;

	public int Resource1 { get{return (int)resource1;} }
	public int Resource2 { get{return (int)resource2;} }
	public int PlayerScore { get{return (int)score;} }
	
	public int GetNumFlies()
	{
		return flies.Count;
	}

	void Start()
	{
		flies = new List<GameObject>();
		
		foreach (Transform fly in GetComponentsInChildren<Transform>()) {
			flies.Add(fly.gameObject);
		}

		resourceObjects = new List<GameObject>();
		
		foreach (Transform r in resource.GetComponentsInChildren<Transform>()) {
			resourceObjects.Add(r.gameObject);
		}
		
		resource1 = 0;
		resource2 = 0;
		score = 0;
	}

	void Update()
	{
		UpdateResources();
		UpdateScore();
	}

	// Just check if a fly is next to a resource and if so increment the appropriate one.
	private void UpdateResources()
	{
		foreach (GameObject resource in resourceObjects) {
			foreach(GameObject fly in flies) {
				if (Vector2.Distance(resource.transform.position, fly.transform.position) < interactionDistance) {
					if (resource.tag == "FlowerTree")
						resource1 = Mathf.Clamp(resource1 + Time.deltaTime * resourceIncrement, 0f, maxResource);
					else if (resource.tag == "AppleTree")
						resource2 = Mathf.Clamp(resource2 + Time.deltaTime * resourceIncrement, 0f, maxResource);
				}
			}
		}
	}

	// At the moment all flies need to be near
	// the main tree to start updating the score from resources.
	private void UpdateScore()
	{
		bool allHome = true;

		// Make sure all flies are home.
		foreach(GameObject fly in flies) {
			if (Vector2.Distance(scoringLocation.transform.position, fly.transform.position) > scoringInteractionDistance) {
				allHome = false;
				break;
			}
		}

		// Start scoring from the resources earned.
		if (allHome) {
			float oldResource1 = resource1; 
			float oldResource2 = resource2;

			resource1 = Mathf.Max(0, resource1 - (Time.deltaTime * resourceIncrement * scoringIncrementModifier));
			resource2 = Mathf.Max(0, resource2 - (Time.deltaTime * resourceIncrement * scoringIncrementModifier));

			score += (oldResource1 - resource1) + (oldResource2 - resource2);
		}
	}
}
