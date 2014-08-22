using UnityEngine;
using System.Collections;

public class Mouth : MonoBehaviour
{
	public GameObject waterSprayPrefab;

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
		if (Input.GetMouseButtonDown(0))
		{
			Vector3 faceDir = transform.rotation * Vector3.up;
			faceDir.Normalize();
			GameObject water = (GameObject)Instantiate(waterSprayPrefab, transform.position + faceDir * 1.1f, transform.rotation);

			// So it follows the player
			water.transform.parent = transform;
		}
	}
}
