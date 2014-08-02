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

		steering.linearVel = target - character;
		steering.linearVel.Normalize();
		Debug.Log(steering.linearVel.ToString());
		steering.linearVel *= maxAcceleration;

		// The angular velocity will be handled by other
		// behaviours such as 'align'.
		steering.angularVel = 0;

		return steering;
	}

	public void updatePlayerPosition(Vector3 position)
	{
		character = position;
	}
}
