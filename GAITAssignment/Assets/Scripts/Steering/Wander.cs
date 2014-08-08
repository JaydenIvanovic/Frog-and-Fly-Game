using UnityEngine;
using System.Collections;

// Could make this more complex, but it seems fairly alright atm.
public class Wander : SteeringBehaviour
{
	public float weight;
	public float wanderingSpeed;

	public override Vector2 GetSteering()
	{
		var newDir =  new Vector2(Random.Range(-2f, 2f), Random.Range(-2f, 2f)).normalized;

		return newDir * wanderingSpeed * weight;
	}
}
