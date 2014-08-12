using UnityEngine;
using System.Collections;

// The main class which combines the other steering behaviour
// components together.
[RequireComponent(typeof(Movement))]
public class SteeringController : MonoBehaviour
{
	private SteeringBehaviour[] steeringBehaviours;
	private Movement movement;

	protected void Awake()
	{
		steeringBehaviours = GetComponents<SteeringBehaviour>();
		movement = GetComponent<Movement>();
	}

	protected void Update()
	{
		Vector2 steering = Vector2.zero;

		foreach (var steeringBehaviour in steeringBehaviours)
			steering += steeringBehaviour.GetSteering();

		movement.Move(steering);
	}
}
