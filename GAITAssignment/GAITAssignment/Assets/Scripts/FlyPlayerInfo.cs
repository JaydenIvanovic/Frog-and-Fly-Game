using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlyPlayerInfo : MonoBehaviour 
{
	private float resource1;
	private float resource2;
	private static float score;
	private static int numFlies;
	private static List<FlyPlayerInfo> flies_info;
	private static List<MouseTargeter> flies_mousetarget;
	private static List<GameObject> resourceObjects;
	public GameObject resource;
	public float interactionDistance;
	public float scoringInteractionDistance;
	public int resourceIncrement;
	public int scoringIncrementModifier;
	public int maxResource;
	public GameObject scoringLocation;

	public int Resource1 {get{return (int)resource1;} }
	public int Resource2 {get{return (int)resource2;} }
	public static int PlayerScore { get{return (int)score;} }
	public static int NumFlies { get{return (int)numFlies;}}

	public static int SelectedFliesResource1 
	{ 
		get {
			float r = 0f;

			for (int i = 0; i < flies_mousetarget.Count; ++i) {
				MouseTargeter m = flies_mousetarget[i];
				if(m.selected) {
					r += flies_info[i].Resource1;
				}
			}

			return (int)r;
		}
	}

	public static int SelectedFliesResource2 
	{ 
		get {
			float r = 0f;
			
			for (int i = 0; i < flies_mousetarget.Count; ++i) {
				MouseTargeter m = flies_mousetarget[i];
				if(m.selected) {
					r += flies_info[i].Resource2;
				}
			}
			
			return (int)r;
		}
	}

	void Start()
	{
		// Singleton style for the resource trees.
		if (resourceObjects == null) {
			resourceObjects = new List<GameObject>();
			
			foreach (Transform r in resource.GetComponentsInChildren<Transform>()) {
				resourceObjects.Add(r.gameObject);
			}

			score = 0;
		}

		if(flies_info == null)
			flies_info = new List<FlyPlayerInfo> ();
		if(flies_mousetarget == null)
			flies_mousetarget = new List<MouseTargeter> ();

		numFlies += 1;
		flies_info.Add((FlyPlayerInfo)GetComponent<FlyPlayerInfo>());
		flies_mousetarget.Add((MouseTargeter)GetComponent<MouseTargeter>());

		resource1 = 0;
		resource2 = 0;
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
<<<<<<< HEAD
				if (Vector2.Distance(resource.transform.position, transform.position) < interactionDistance) {
					if (resource.tag == "FlowerTree")
						resource1 = Mathf.Clamp(resource1 + Time.deltaTime * resourceIncrement, 0f, maxResource);
					else if (resource.tag == "AppleTree")
						resource2 = Mathf.Clamp(resource2 + Time.deltaTime * resourceIncrement, 0f, maxResource);
=======
			foreach(GameObject fly in flies) {
				if (fly != null) {
					if (Vector2.Distance(resource.transform.position, fly.transform.position) < interactionDistance) {
						if (resource.tag == "FlowerTree")
							resource1 = Mathf.Clamp(resource1 + Time.deltaTime * resourceIncrement, 0f, maxResource);
						else if (resource.tag == "AppleTree")
							resource2 = Mathf.Clamp(resource2 + Time.deltaTime * resourceIncrement, 0f, maxResource);
					}
>>>>>>> 8e388515b6408c7dfe8b408940d18a7c4e42b551
				}
		}
	}

	// At the moment all flies need to be near
	// the main tree to start updating the score from resources.
	private void UpdateScore()
	{
<<<<<<< HEAD
=======
		bool allHome = true;

		// Make sure all flies are home.
		foreach(GameObject fly in flies) {
			if (fly != null) {
				if (Vector2.Distance(scoringLocation.transform.position, fly.transform.position) > scoringInteractionDistance) {
					allHome = false;
					break;
				}
			}
		}

>>>>>>> 8e388515b6408c7dfe8b408940d18a7c4e42b551
		// Start scoring from the resources earned.
		if (Vector2.Distance(scoringLocation.transform.position, transform.position) < scoringInteractionDistance) {
			float oldResource1 = resource1; 
			float oldResource2 = resource2;

			resource1 = Mathf.Max(0, resource1 - (Time.deltaTime * resourceIncrement * scoringIncrementModifier));
			resource2 = Mathf.Max(0, resource2 - (Time.deltaTime * resourceIncrement * scoringIncrementModifier));

			score += (oldResource1 - resource1) + (oldResource2 - resource2);
		}
	}
}
