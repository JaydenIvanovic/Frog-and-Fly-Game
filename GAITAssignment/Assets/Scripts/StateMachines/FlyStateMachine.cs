using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AppleTreeTargeter))]
[RequireComponent(typeof(GameObjectTargeter))]
[RequireComponent(typeof(Seek))]
[RequireComponent(typeof(Movement))]
public class FlyStateMachine : MonoBehaviour {

	private Targeter appleTreeTargeter;
	private GameObjectTargeter playerTargeter;
	private Seek seekComponent;
	private Movement movement;

	public float fleeDistance;
	public float fleeSpeed;
	public float appleTreeSpeed;

	// Use this for initialization
	void Start () {
		appleTreeTargeter = (AppleTreeTargeter)GetComponent<AppleTreeTargeter>();
		playerTargeter = (GameObjectTargeter)GetComponent<GameObjectTargeter>();
		seekComponent = (Seek)GetComponent<Seek>();
		movement = (Movement)GetComponent<Movement>();

		playerTargeter.Target = GameObject.Find("Player");
	}
	
	// Update is called once per frame
	void Update () {

		float distanceFromPlayer;
		Vector2? playerPos = playerTargeter.GetTarget();

		if (playerPos == null) {
			distanceFromPlayer = float.MaxValue;
		} else {
			distanceFromPlayer = ((Vector2)(transform.position) - (Vector2)(playerTargeter.GetTarget())).magnitude;
		}

		if (distanceFromPlayer < fleeDistance) {
			seekComponent.flee = true;
			seekComponent.SetTargeter(playerTargeter);
			movement.speed = fleeSpeed;
		} else {
			seekComponent.flee = false;
			seekComponent.SetTargeter(appleTreeTargeter);
			movement.speed = appleTreeSpeed;
		}
	}
}
