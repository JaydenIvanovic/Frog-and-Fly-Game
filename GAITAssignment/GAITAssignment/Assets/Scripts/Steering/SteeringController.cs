using UnityEngine;
using System.Collections;

// The main class which combines the other steering behaviour
// components together.
[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(BoxCollider2D))]
public class SteeringController : MonoBehaviour
{
	private SteeringBehaviour[] steeringBehaviours;
	private Movement movement;
	private ObstacleAvoider avoider;

	public bool avoidObstacles;
	
	protected void Awake()
	{
		steeringBehaviours = GetComponents<SteeringBehaviour>();
		movement = GetComponent<Movement>();
		avoider = GetComponent<ObstacleAvoider>();
	}

	protected void Update()
	{

		Vector2 steering = Vector2.zero;

		foreach (var steeringBehaviour in steeringBehaviours)
			steering += steeringBehaviour.GetSteering();

		if (avoidObstacles && (avoider != null)) {
			steering = avoider.AvoidObstacles(steering);
		}

		movement.Move(steering);
	}
}
