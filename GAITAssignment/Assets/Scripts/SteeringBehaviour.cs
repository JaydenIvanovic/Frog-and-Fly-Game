using UnityEngine;
using System.Collections;

// Mainly a helper to construct the desired behaviour.
public enum BType 
{
	Seek, 
	Flee,
	Arrive,
	None
}

// The class which should be attached to a gameobject.
// In inspector set the type of behaviour wanted as indicated
// via the BType enum.
[RequireComponent(typeof(Movement))]
public class SteeringBehaviour : MonoBehaviour 
{
	public BType selectedBehaviour;
	private Steering steering;
	private Targeter targeter;

	// Use this for initialization
	void Start () 
	{
		// TO DO: Fix the dodginess of this code... Put it in a general function or something.
		Targeter[] targeters = GetComponents<Targeter>();
		foreach (Targeter t in targeters) {
			targeter = t;
			// Use an AStarTargeter if it exists, otherwise whatever else there is
			if (t.GetType() == typeof(AStarTargeter)) {
				break;
			}
		}

		SetDefaults();
	}
	
	// Update is called once per frame
	void Update () 
	{
		Vector2? target = targeter.GetTarget();

		if (target != null) {

			//Debug.Log ("Target is " + ((Vector2)target).x + ", " + ((Vector2)target).y);
			
			switch(selectedBehaviour)
			{
			case BType.Seek:
				steering = new Steering(transform.position, (Vector2)target);
				break;
			case BType.Flee:
				steering = new Steering((Vector2)target, transform.position);
				break;
			case BType.Arrive:
				steering = new ArriveSteering(transform.position, (Vector2)target);
				break;
			case BType.None:
				return;
			default:
				break;
			}
		}

		if (selectedBehaviour == BType.None || steering == null)
			return;

		SteeringOutput so = steering.getSteering();

		// If we should ignore this behaviour.
		// e.g. we have arrived at our target.
		if(so.ignore)
		{
			rigidbody2D.velocity = Vector2.zero;
			rigidbody2D.angularVelocity = 0.0f;
			return;
		}

		// Update the position and orientation of the gameobject.
		transform.position += new Vector3(rigidbody2D.velocity.x, rigidbody2D.velocity.y, 0.0f) * Time.deltaTime;

		rigidbody2D.rotation += rigidbody2D.angularVelocity * Time.deltaTime;

		// Update the rigidbody velocities for next update.
		rigidbody2D.velocity += (Vector2)(so.linearVel) * Time.deltaTime;
		// Cos for z as we would rather rotate around the z-axis rather than the x-axis from our ortho perspective
		//rigidbody.angularVelocity += new Vector3(0f, Mathf.Sin(so.angularVel), Mathf.Cos(so.angularVel)) * Time.deltaTime;

		// During "Seek" the character moves so we must update their position in the steering class.
		if (selectedBehaviour == BType.Seek || selectedBehaviour == BType.Arrive)
			steering.updatePlayerPosition(transform.position);
	}
	
	// For testing given an arbitrary initial velocity
	private void SetDefaults()
	{
		rigidbody2D.velocity = Vector2.zero;
		rigidbody2D.angularVelocity = 0.0f;
	}
}
