using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerInfo))]
public class Mouth : MonoBehaviour {
	
	private float rotationOffset;
	private PlayerInfo playerInfo;

	public float BubbleCost = 20.0f;
	public float BubbleLaunchDistance = 0.3f;
	public GameObject waterProjectilePrefab;
	
	void Awake () {
		rotationOffset = transform.parent.rotation.eulerAngles.z;
		playerInfo = transform.parent.gameObject.GetComponent<PlayerInfo>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		SprayWater();
	}

	void OnTriggerEnter2D(Collider2D other) {

		if (other.gameObject.tag.Equals ("Fly")) {

			Flocking flocker = other.gameObject.GetComponent<Flocking>();

			if (flocker != null) {
				Destroy (other.gameObject.GetComponent<Flocking>());
				Flocking.DestroyFlockMember(other.gameObject);
			}

			Destroy (other.gameObject);

			transform.parent.gameObject.GetComponent<PlayerInfo>().IncrementScore();
		}
	}

	void SprayWater()
	{
		if (!PlayerInfo.isPaused && Input.GetMouseButtonDown(0) && playerInfo.waterLevel > PlayerInfo.BUBBLE_COST)
		{
			Vector2 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			
			Vector2 shotDirection = clickPos - (Vector2)(transform.position);
			
			float angle = Mathf.Atan2(shotDirection.y, shotDirection.x) * Mathf.Rad2Deg;

			if (transform.parent.GetComponent<Animator>().GetBool("Sitting")) {
				transform.parent.GetComponent<Animator>().SetBool("Sitting", false);
			}

			Movement movement = transform.parent.GetComponent<Movement>();
			if (movement != null) {
				movement.OverrideRotation(angle);
			}

			MouseTargeter mouseTargeter = transform.parent.GetComponent<MouseTargeter>();
			if (mouseTargeter != null) {
				mouseTargeter.StopTargeting();
			}

			AStarTargeter aStarTargeter = transform.parent.GetComponent<AStarTargeter>();
			if (aStarTargeter != null) {
				aStarTargeter.StopTargeting();
			}

			shotDirection.Normalize();
			
			Instantiate(waterProjectilePrefab,
			            new Vector3(transform.position.x + shotDirection.x * BubbleLaunchDistance, transform.position.y + shotDirection.y * BubbleLaunchDistance, transform.position.z),
			            Quaternion.Euler(0.0f, 0.0f, angle - rotationOffset));

			playerInfo.ReduceWaterAfterBubble();
		}
	}
}
