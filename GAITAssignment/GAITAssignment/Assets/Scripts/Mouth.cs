using UnityEngine;
using System.Collections;

public class Mouth : MonoBehaviour
{
	public GameObject waterProjectilePrefab;

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		SprayWater();
	}

	void OnTriggerEnter2D(Collider2D other) 
	{
		if (other.gameObject.tag.Equals ("Fly")) {
			Destroy (other.gameObject.GetComponent<Flocking>());
			Flocking.DestroyFlockMember(other.gameObject);
			Destroy (other.gameObject);
			PlayerInfo.IncrementScore();
		}
	}

	void SprayWater()
	{
		if (Input.GetMouseButtonDown(0) && PlayerInfo.IsUnderwater())
		{
			GameObject water = (GameObject)Instantiate(waterProjectilePrefab, transform.position, transform.rotation);
		}
	}
}
