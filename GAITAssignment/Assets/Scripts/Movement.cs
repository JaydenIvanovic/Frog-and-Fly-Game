using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour 
{
	// Linear motion
	public float speed = 3f;
	public float acceleration = 10f;

	// Angular motion
	public float angularAccelelation = 5.0f;
	public float angularMaxSpeed = 180.0f;
	public float angleAdjustment = 0.0f; // In case the sprite isn't facing left (0 degrees) to begin with

	private float angularPosition;

	public void Start() {

		angularPosition = rigidbody2D.transform.localEulerAngles.z;

		// Rotate to the initial position
		rigidbody2D.transform.localEulerAngles = new Vector3(0.0f, 0.0f, angularPosition + angleAdjustment);
	}
	
	public void Move(Vector3 velocity)
	{
		// Get the vectors direction.
		if (velocity.sqrMagnitude > 1)
			velocity = velocity.normalized;
		
		// Give it some speed.
		velocity *= speed;

		var accel = ((Vector2)velocity - rigidbody2D.velocity) * acceleration;
		// Square acceleration as we have the sqrMagnitude of accel.
		if (accel.sqrMagnitude > acceleration * acceleration)
			accel = accel.normalized * acceleration;

		rigidbody2D.velocity += accel * Time.deltaTime;
		
		// Angular movement
		if (velocity != Vector3.zero) { // Don't rotate unless we're actually going somewhere

			float targetAngularVel = Mathf.Rad2Deg * Mathf.Atan2(velocity.y, velocity.x) - angularPosition;

			while (targetAngularVel > 180.0f)
				targetAngularVel -= 360.0f;
			
			while (targetAngularVel < -180.0f)
				targetAngularVel += 360.0f;

			if (Mathf.Abs(targetAngularVel) > angularMaxSpeed) {
				targetAngularVel *= (angularMaxSpeed / Mathf.Abs(targetAngularVel));
			}

			targetAngularVel *= angularAccelelation;
			
			angularPosition = angularPosition + targetAngularVel * Time.deltaTime;
			rigidbody2D.transform.localEulerAngles = new Vector3(0.0f, 0.0f, angularPosition + angleAdjustment);
		}
	}
}
