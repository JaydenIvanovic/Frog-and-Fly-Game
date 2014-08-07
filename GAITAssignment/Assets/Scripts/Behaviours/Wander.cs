using UnityEngine;
using System.Collections;

public class Wander : Steering 
{
	private Vector3 currentVelocity;

	public Wander(Vector3 currentVelocity)
	{
		this.currentVelocity = currentVelocity;
	}

	public override SteeringOutput getSteering()
	{
		var steering = new SteeringOutput();
		var randVec = new Vector3( Random.Range(-0.7f, 0.7f), Random.Range(-0.7f, 0.7f), 0f );

		steering.linearVel = randVec.normalized + currentVelocity.normalized;
	
		steering.ignore = false;

		return steering;
	}
}
