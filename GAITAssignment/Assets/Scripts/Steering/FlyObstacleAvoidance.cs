using UnityEngine;
using System.Collections;

// Steering behaviour specific for flies. This is to 
// get behaviour similar to that which a fly exhibits 
// in real life when attempting to avoid obstacles.
// TODO Still a work in progress...
public class FlyObstacleAvoidance : SteeringBehaviour
{
	public float detectionDist = 2f;
	public float weight = 1f;

	public override Vector2 GetSteering()
	{
		if(PossibleCollision())
		{
			int rand = Random.Range(0, 2);

			if(rand == 1)
				return new Vector2(1,0) * 4f;
			else
				return new Vector2(-1,0) * 4f;
		}

		return Vector2.zero;
	}

	private bool PossibleCollision()
	{
		RaycastHit2D[] rayHits = Physics2D.RaycastAll(transform.position, rigidbody2D.velocity.normalized, detectionDist);
		bool obstacle = false;

		foreach (var rayHit in rayHits)
		{
			// As the ray includes the object itself skip it.
			if(rayHit.collider.gameObject.name == gameObject.name)
				continue;

			obstacle = true;
		}

		return obstacle;
	}
}
