using UnityEngine;
using System.Collections;

public enum SnakeDirections
{
	Up = 0,
	Left = 1,
	Down = 2,
	Right = 3
};

[RequireComponent(typeof(GameObjectTargeter))]
[RequireComponent(typeof(AStarTargeter))]
[RequireComponent(typeof(Wander))]
[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(Animator))]
public class PredatorStateMachine : MonoBehaviour {

	private GameObjectTargeter underlyingTargeter;
	private AStarTargeter targeter;
	private Wander wanderer;
	private Movement movement;
	private Animator animator;
	private float timeSinceWentHome;
	private bool wasChasing = false;

	public GameObject Home;
	public GameObject Player;
	public float LeashLength = 6.0f;
	public float GiveUpDistance = 4.0f;
	public float GoHomeTimeout = 1.5f;
	public float KnockForce = 250.0f;

	// Use this for initialization
	void Start () {
		underlyingTargeter = GetComponent<GameObjectTargeter>();
		targeter = GetComponent<AStarTargeter>();
		wanderer = GetComponent<Wander>();
		movement = GetComponent<Movement>();
		animator = GetComponent<Animator>();

		timeSinceWentHome = GoHomeTimeout;
	}
	
	// Update is called once per frame
	void Update () {

		timeSinceWentHome += Time.deltaTime;

		if ((((Vector2)(transform.position) - (Vector2)(Home.transform.position)).magnitude > LeashLength)
		    && (((Vector2)(transform.position) - (Vector2)(Player.transform.position)).magnitude > GiveUpDistance)) {

			// Go home if we've gone too far
			targeter.enabled = true;
			underlyingTargeter.Target = Home;
			wanderer.weight = 0.0f;
			movement.acceleration = 1.0f;
			movement.speed = 2.0f;

			if (wasChasing) {
				timeSinceWentHome = 0.0f;
				wasChasing = false;
			}

		} else {

			if (timeSinceWentHome > GoHomeTimeout) {

				// Target the player
				targeter.enabled = true;
				underlyingTargeter.Target = Player;
				Vector2? target = targeter.GetTarget();

				if (target != null) {
					
					// Check if we're gonna chase
					if ((((Vector2)(Player.transform.position) - (Vector2)(Home.transform.position)).magnitude < LeashLength)
				    	|| (((Vector2)(transform.position) - (Vector2)(Player.transform.position)).magnitude < GiveUpDistance)) {

						// Chase
						wanderer.weight = 0.0f;
						movement.acceleration = 5.0f;
						movement.speed = 3.0f;
						wasChasing = true;

					} else {
						// Don't chase if we're too far away and the player is not near home
						targeter.enabled = false;
					}
				}
			}
		}

		// Wander if there's no specific target
		if (targeter.enabled == false) {
			wanderer.weight = 1.0f;
			movement.acceleration = 1.0f;
			movement.speed = 2.0f;
			wasChasing = false;
		}

		// Update animation

		float actualRotation = transform.localEulerAngles.z - movement.angleAdjustment;

		while (actualRotation < 0.0f)
			actualRotation += 360.0f;

		while (actualRotation > 360.0f)
			actualRotation -= 360.0f;

		SnakeDirections dir = SnakeDirections.Up;

		if ((actualRotation > 45.0f) && (actualRotation < 135.0f)) {
			dir = SnakeDirections.Up;
		} else if ((actualRotation > 135.0f) && (actualRotation < 225.0f)) {
			dir = SnakeDirections.Left;
		} else if ((actualRotation > 225.0f) && (actualRotation < 315.0f)) {
			dir = SnakeDirections.Down;
		} else if ((actualRotation > 315.0f) || (actualRotation < 45.0f)) {
			dir = SnakeDirections.Right;
		}

		animator.SetInteger("Direction", (int)dir);
	}

	private void CheckIfHitPlayer(Collider2D other) {

		if (other.gameObject.tag.Equals ("Player") && !PlayerInfo.IsInvulnerable()) {

			PlayerInfo.DecrementHealth();
			PlayerInfo.MakeInvulnerable();

			// Knock the player
			GameObject player = GameObject.FindGameObjectWithTag("Player");
			Vector2 knockDirection = ((Vector2)(player.transform.position - transform.position)).normalized;
			player.rigidbody2D.AddForce(knockDirection * KnockForce);
		}
	}

	public void OnTriggerEnter2D(Collider2D other) {
		CheckIfHitPlayer(other);
	}

	public void OnTriggerStay2D(Collider2D other) {
		CheckIfHitPlayer(other);
	}
}
