using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AStarTargeter))]
[RequireComponent(typeof(Wander))]
[RequireComponent(typeof(Movement))]
public class PredatorStateMachine : MonoBehaviour {

	private AStarTargeter targeter;
	private Wander wanderer;
	private Movement movement;

	// Use this for initialization
	void Start () {
		targeter = GetComponent<AStarTargeter>();
		wanderer = GetComponent<Wander>();
		movement = GetComponent<Movement>();
	}
	
	// Update is called once per frame
	void Update () {

		Vector2? target = targeter.GetTarget();
		
		if (target != null) { // We're chasing the player
			wanderer.weight = 0.0f;
			movement.angularMaxSpeed = 90.0f;
			movement.speed = 3.0f;
		} else {
			wanderer.weight = 1.0f;
			movement.angularMaxSpeed = 15.0f;
			movement.acceleration = 1.0f;
			movement.speed = 2.0f;
		}
	}
}
