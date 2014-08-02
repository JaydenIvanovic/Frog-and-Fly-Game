using UnityEngine;
using System.Collections;

public struct SteeringOutput
{
	public Vector3 linearVel;
	public float angularVel;
}

public class Steering 
{
	private Vector3 character, target;
	public float maxAcceleration = 5f;

	public Steering(Vector3 character, Vector3 target)
	{
		this.character = character;
		this.target = target;
	}

	public SteeringOutput getSteering()
	{
		var steering = new SteeringOutput();

		// Get direction to target and make it a unit length
		// vector before heading there at maximum speed.
		steering.linearVel = target - character;
		steering.linearVel.Normalize();
		steering.linearVel *= maxAcceleration;

		// As we are working in 2D, ensure z is 0
		steering.linearVel.z = 0f;

		// The angular velocity will be handled by other
		// behaviours such as 'align'.
		steering.angularVel = 0f;

		return steering;
	}

	public void updatePlayerPosition(Vector3 position)
	{
		character = position;
	}
}
