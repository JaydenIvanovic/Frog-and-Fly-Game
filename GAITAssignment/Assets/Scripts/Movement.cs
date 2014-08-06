using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Movement : MonoBehaviour 
{
	public float speed = 3f;
	public float acceleration = 10f;
	
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
	}
}
