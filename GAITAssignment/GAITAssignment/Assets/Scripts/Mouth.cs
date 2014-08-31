using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerInfo))]
public class Mouth : MonoBehaviour {
	
	private float rotationOffset;

	public float BubbleCost = 20.0f;
	public float BubbleLaunchDistance = 0.3f;
	public GameObject waterProjectilePrefab;
	
	void Awake () {
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
			PlayerInfo.IncrementScore();
		}
	}

	void SprayWater()
	{
		if (!PlayerInfo.isPaused && Input.GetMouseButtonDown(0) && PlayerInfo.GetWaterLevel() > PlayerInfo.BUBBLE_COST)
		{
			Vector2 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			
			Vector2 shotDirection = clickPos - (Vector2)(transform.position);
			
			float angle = Mathf.Atan2(shotDirection.y, shotDirection.x) * Mathf.Rad2Deg;

			if (transform.parent.GetComponent<Animator>().GetBool("Sitting")) {
				transform.parent.GetComponent<Animator>().SetBool("Sitting", false);
			}

			transform.parent.GetComponent<Movement>().OverrideRotation(angle);
			transform.parent.GetComponent<MouseTargeter>().StopTargeting();
			transform.parent.GetComponent<AStarTargeter>().StopTargeting();
			shotDirection.Normalize();
			
			Instantiate(waterProjectilePrefab,
			            new Vector3(transform.position.x + shotDirection.x * BubbleLaunchDistance, transform.position.y + shotDirection.y * BubbleLaunchDistance, transform.position.z),
			            Quaternion.Euler(0.0f, 0.0f, angle - rotationOffset));

			PlayerInfo.ReduceWaterAfterBubble();
		}
	}
}
