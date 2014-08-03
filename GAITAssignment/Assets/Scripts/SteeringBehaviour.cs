using UnityEngine;
using System.Collections;

// Mainly a helper to construct the desired behaviour.
public enum BType 
{
	Seek, 
	Flee,
	Arrive
}

// The class which should be attached to a gameobject.
// In inspector set the type of behaviour wanted as indicated
// via the BType enum.
[RequireComponent(typeof(Movement))]
public class SteeringBehaviour : MonoBehaviour 
{
	public BType selectedBehaviour;
	private Steering steering;
	public GameObject targetObj;

	// Use this for initialization
	void Start () 
	{
		switch(selectedBehaviour)
		{
			case BType.Seek:
				steering = new Steering(transform.position, targetObj.transform.position);
				break;
			case BType.Flee:
				steering = new Steering(targetObj.transform.position, transform.position);
				break;
			case BType.Arrive:
				steering = new ArriveSteering(transform.position, targetObj.transform.position);
				break;
			default:
				break;
		}

		SetDefaults();
	}
	
	// Update is called once per frame
	void Update () 
	{
		SteeringOutput so = steering.getSteering();

		// If we should ignore this behaviour.
		// e.g. we have arrived at our target.
		if(so.ignore)
		{
			rigidbody.velocity = Vector3.zero;
			rigidbody.angularVelocity = Vector3.zero;
			return;
		}

		// Update the position and orientation of the gameobject.
		transform.position += rigidbody.velocity * Time.deltaTime;
		// Multiply as rotation is a quaternion
		rigidbody.rotation *= Quaternion.Euler(rigidbody.angularVelocity * Time.deltaTime);

		// Update the rigidbody velocities for next update.
		rigidbody.velocity += so.linearVel * Time.deltaTime;
		// Cos for z as we would rather rotate around the z-axis rather than the x-axis from our ortho perspective
		//rigidbody.angularVelocity += new Vector3(0f, Mathf.Sin(so.angularVel), Mathf.Cos(so.angularVel)) * Time.deltaTime;

		// During "Seek" the character moves so we must update their position in the steering class.
		if (selectedBehaviour == BType.Seek || selectedBehaviour == BType.Arrive)
			steering.updatePlayerPosition(transform.position);
	}
	
	// For testing given an arbitrary initial velocity
	private void SetDefaults()
	{
		rigidbody.velocity = Vector3.left; 
		rigidbody.angularVelocity = Vector3.zero;
	}
}
