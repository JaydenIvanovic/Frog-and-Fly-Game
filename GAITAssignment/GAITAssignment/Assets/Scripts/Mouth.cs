using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerInfo))]
public class Mouth : MonoBehaviour {

	private PlayerInfo playerInfo;
	private float rotationOffset;

	public GameObject waterProjectilePrefab;
	
	void Awake () {
		playerInfo = GetComponentInParent<PlayerInfo>();
		rotationOffset = transform.parent.rotation.eulerAngles.z;
	}
	
	// Update is called once per frame
	void Update () 
	{
		SprayWater();
	}

	void OnTriggerEnter2D(Collider2D other) {

		if (other.gameObject.tag.Equals ("Fly")) {
			Destroy (other.gameObject.GetComponent<Flocking>());
			Flocking.DestroyFlockMember(other.gameObject);
			Destroy (other.gameObject);
			playerInfo.IncrementScore();
		}
	}

	void SprayWater()
	{
		if (Input.GetMouseButtonDown(0) && playerInfo.IsUnderwater())
		{
			Vector2 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

			float angle = Mathf.Atan2(clickPos.y - transform.position.y, clickPos.x - transform.position.x) * Mathf.Rad2Deg;

			Instantiate(waterProjectilePrefab, transform.position, Quaternion.Euler(0.0f, 0.0f, angle - rotationOffset));
		}
	}
}
