using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Targeter))]
[RequireComponent(typeof(Animator))]
public class FrogMovement : MonoBehaviour {

	private Targeter targeter;
	private Rigidbody2D rb;
	private Animator animator;
	private float angularPosition = 0.0f;

	public float acceleration = 2.0f;
	public float angularAccel = 5.0f;
	public float maxVel = 2.0f;

	public TargeterType primaryTargeter;

	// Use this for initialization
	void Start () {

		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();

		System.Type primaryTargeterType = null;

		if (primaryTargeter == TargeterType.AStar) {
			primaryTargeterType = typeof(AStarTargeter);
		} else if (primaryTargeter == TargeterType.Mouse) {
			primaryTargeterType = typeof(MouseTargeter);
		}

		// TO DO: Ensure that there are 2 targeters (self and the original targeter)
		Targeter[] targeters = GetComponents<Targeter>();
		foreach (Targeter t in targeters) {
			if (t.GetType() == primaryTargeterType) {
				targeter = t;
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
		Vector2 desiredVel;
		float targetAngularVel;

		Vector2? target = targeter.GetTarget();
		
		if (target != null) {
			
			animator.SetBool("Sitting", false);
			
			desiredVel = ((Vector2)target - (Vector2)transform.position).normalized * maxVel;
			
			// Angular acceleration
			targetAngularVel = (Mathf.Rad2Deg * Mathf.Atan2(desiredVel.y, desiredVel.x) - 90.0f) - angularPosition;
			
			while (targetAngularVel > 180.0f)
				targetAngularVel -= 360.0f;
			
			while (targetAngularVel < -180.0f)
				targetAngularVel += 360.0f;
			
			angularPosition = angularPosition + targetAngularVel * angularAccel * Time.deltaTime;
			rb.transform.localEulerAngles = new Vector3(0.0f, 0.0f, angularPosition);
			
		} else {
			animator.SetBool("Sitting", true);
			
			if (rb.velocity.magnitude > 0.05f) { // TO DO: Fix magic number!
				desiredVel = Vector2.zero;
				Vector2 stoppingForce = (desiredVel - rb.velocity).normalized * acceleration;
				rb.AddForce(stoppingForce);
			} else { // TO DO: Fix magic number!
				rb.velocity = Vector2.zero;
			}
			return;
		}
		Vector2 velChange = (desiredVel - rb.velocity).normalized * acceleration;
		
		rb.AddForce(velChange);
		
		rb.angularVelocity = 0.0f; // We're doing this manually
	}
}
