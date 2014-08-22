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
[RequireComponent(typeof(HuntTargeter))]
[RequireComponent(typeof(AStarTargeter))]
[RequireComponent(typeof(Wander))]
[RequireComponent(typeof(Seek))]
[RequireComponent(typeof(Movement))]
[RequireComponent(typeof(Animator))]
public class PredatorStateMachine : MonoBehaviour 
{
	private GameObjectTargeter homeTargeter;
	private HuntTargeter huntTargeter;
	private AStarTargeter aStarTargeter;
	private Wander wanderer;
	private Seek seek;
	private Movement movement;
	private Animator animator;
	private float timeSinceWentHome;
	private bool wasChasing = false;
	private GameObject child = null;
	private float parentingTimer;
	private State currentState;
	private static AudioSource SoundSource; // Static so we don't get a weird chorus effect when all snakes attack at once
	private float bubbleTimeLeft = 0.0f;
	
	public GameObject Home;
	public GameObject Player;
	public GameObject Egg;
	public float ParentAge = 30f; // age in seconds
	public float ParentDesire = 0.3f;
	public float LeashLength = 6.0f;
	public float GiveUpDistance = 4.0f;
	public float GoHomeTimeout = 1.5f;
	public float KnockForce = 250.0f;
	public float BubbleTime = 3.0f;
	public SpriteRenderer bubble;
	public AudioClip AttackSound;

	private enum State
	{
		Chasing,
		Wandering,
		HeadingHome,
		Parenting,
		Bubbled
	};

	void Awake ()
	{
		homeTargeter = GetComponent<GameObjectTargeter>();
		huntTargeter = GetComponent<HuntTargeter>();
		aStarTargeter = GetComponent<AStarTargeter>();
		wanderer = GetComponent<Wander>();
		seek = GetComponent<Seek>();
		movement = GetComponent<Movement>();
		animator = GetComponent<Animator>();

		parentingTimer = 0f;
		timeSinceWentHome = GoHomeTimeout;

		// Ensure that the snake has someone to target and a home.
		if (Home == null || Player == null)
		{
			// Place in predator hierarchy.
			transform.parent = GameObject.Find("Predators").transform;
			
			// Set the player for the predator to chase and this predators home base.
			var predStateMac = GetComponent<PredatorStateMachine>();
			predStateMac.Player = GameObject.FindGameObjectWithTag("Player");
			if (Random.Range (0, 2) == 0)
				predStateMac.Home = GameObject.Find ("SnakeHomeLeft");
			else
				predStateMac.Home = GameObject.Find ("SnakeHomeRight");
		}

		huntTargeter.Target = Player;
		homeTargeter.Target = Home;

		currentState = State.HeadingHome; // So we don't play the chasing sound immediately!
		SoundSource = gameObject.AddComponent<AudioSource>();
		SoundSource.loop = false;
	}

	// Update is called once per frame
	void Update () 
	{
		UpdateState();

		// Defaults
		seek.weight = 1.0f;
		aStarTargeter.underlyingTargeter = homeTargeter;
		bubble.enabled = false;

		switch(currentState)
		{
			case State.Chasing:
				aStarTargeter.underlyingTargeter = huntTargeter;
				child = null;
				wanderer.weight = 0.0f;
				movement.acceleration = 5.0f;
				movement.speed = 3.0f;
				if (!wasChasing) {
					SoundSource.clip = AttackSound;
					SoundSource.Play();
				}
				wasChasing = true;
				break;
			case State.HeadingHome:
				child = null;
				homeTargeter.Target = Home;
				wanderer.weight = 0.0f;
				movement.acceleration = 1.0f;
				movement.speed = 2.0f;
				
				if (wasChasing) {
					timeSinceWentHome = 0.0f;
					wasChasing = false;
				}
				break;
			case State.Parenting:
				homeTargeter.Target = child;
				wanderer.weight = 0.2f;
				break;
			case State.Wandering:
				child = null;
				seek.weight = 0.0f;
				wanderer.weight = 1.0f;
				movement.acceleration = 1.0f;
				movement.speed = 2.0f;
				wasChasing = false;
				break;
			case State.Bubbled:
				bubble.enabled = true;
				child = null;
				seek.weight = 0.0f;
				wanderer.weight = 0.0f;
				movement.acceleration = 0.0f;
				movement.speed = 0.0f;
				wasChasing = false;
				break;
		}

		UpdateAnimation();
	}


	private void UpdateState()
	{
		if (bubbleTimeLeft > 0.0f) {
			bubbleTimeLeft -= Time.deltaTime;
			return;
		}

		PlayerInfo playerInfo = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInfo>();

		timeSinceWentHome += Time.deltaTime;
		parentingTimer += Time.deltaTime;

		// Let the parent remain with the egg
		if(child)
		{
			//Debug.Log("with child");
			if( ((Vector2)(transform.position) - (Vector2)(Player.transform.position)).magnitude < GiveUpDistance)
			{
				currentState = State.Chasing;
				return;
			}
			else 
			{
				currentState = State.Parenting;
				return;
			}
		}

		if (parentingTimer >= ParentAge && !child) 
		{
			parentingTimer = 0f;
			// Extra randomization step to see if an egg is to be created.
			if (Random.Range(0f,1f) <= ParentDesire)
				LayEgg();
			currentState = State.Parenting;
		}
		else if ((((Vector2)(transform.position) - (Vector2)(Home.transform.position)).magnitude > LeashLength)
		    && ((((Vector2)(transform.position) - (Vector2)(Player.transform.position)).magnitude > GiveUpDistance) || playerInfo.IsUnderwater()))
		{
			currentState = State.HeadingHome;	
		} 
		else if (timeSinceWentHome > GoHomeTimeout) 
		{	
			// Target the player
			homeTargeter.Target = Player;
			Vector2? target = aStarTargeter.GetTarget();
			
			if (target != null) 
			{	
				// Check if we're gonna chase.
				if (((((Vector2)(Player.transform.position) - (Vector2)(Home.transform.position)).magnitude < LeashLength)
				    || (((Vector2)(transform.position) - (Vector2)(Player.transform.position)).magnitude < GiveUpDistance))
				    && !playerInfo.IsUnderwater()) 
				{
					currentState = State.Chasing;	
				} 
				else 
				{
					currentState = State.Wandering;
				}
			}
		}
	}


	private void UpdateAnimation()
	{
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


	private void CheckIfHitPlayer(Collider2D other) 
	{
		if (other.gameObject.tag.Equals ("Player")) {

			PlayerInfo playerInfo = other.gameObject.GetComponent<PlayerInfo>();

			if (!playerInfo.IsInvulnerable()) {

				playerInfo.DecrementHealth();
				playerInfo.MakeInvulnerable();

				// Knock the player
				GameObject player = GameObject.FindGameObjectWithTag("Player");
				Vector2 knockDirection = ((Vector2)(player.transform.position - transform.position)).normalized;
				player.rigidbody2D.AddForce(knockDirection * KnockForce);
			}
		}
	}


	private void LayEgg()
	{
		child = (GameObject)Instantiate(Egg, transform.position - Vector3.down + new Vector3(0.0f, 0.0f, 1.0f), Quaternion.identity);
	}


	public void OnTriggerEnter2D(Collider2D other) 
	{
		CheckIfHitPlayer(other);

		if (other.gameObject.tag == "Projectile") {
			rigidbody2D.velocity = Vector2.zero;
			bubbleTimeLeft = BubbleTime;
			currentState = State.Bubbled;
		}
	}


	public void OnTriggerStay2D(Collider2D other) 
	{
		CheckIfHitPlayer(other);
	}
}
