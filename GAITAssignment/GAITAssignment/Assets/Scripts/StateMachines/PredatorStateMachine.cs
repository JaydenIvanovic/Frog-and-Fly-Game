﻿using UnityEngine;
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
public class PredatorStateMachine : MonoBehaviour 
{
	private GameObjectTargeter underlyingTargeter;
	private AStarTargeter targeter;
	private Wander wanderer;
	private Movement movement;
	private Animator animator;
	private float timeSinceWentHome;
	private bool wasChasing = false;
	private GameObject child = null;
	private float parentingTimer;
	private State currentState;

	public GameObject Home;
	public GameObject Player;
	public GameObject Egg;
	public float ParentAge = 30f; // age in seconds
	public float ParentDesire = 0.3f;
	public float LeashLength = 6.0f;
	public float GiveUpDistance = 4.0f;
	public float GoHomeTimeout = 1.5f;
	public float KnockForce = 250.0f;

	private enum State
	{
		Chasing,
		Wandering,
		HeadingHome,
		Parenting
	};


	// Use this for initialization
	void Start ()
	{
		underlyingTargeter = GetComponent<GameObjectTargeter>();
		targeter = GetComponent<AStarTargeter>();
		wanderer = GetComponent<Wander>();
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
	}


	// Update is called once per frame
	void Update () 
	{
		UpdateState();

		switch(currentState)
		{
			case State.Chasing:
				child = null;
				wanderer.weight = 0.0f;
				movement.acceleration = 5.0f;
				movement.speed = 3.0f;
				wasChasing = true;
				break;
			case State.HeadingHome:
				child = null;
				targeter.enabled = true;
				underlyingTargeter.Target = Home;
				wanderer.weight = 0.0f;
				movement.acceleration = 1.0f;
				movement.speed = 2.0f;
				
				if (wasChasing) {
					timeSinceWentHome = 0.0f;
					wasChasing = false;
				}
				break;
			case State.Parenting:
				targeter.enabled = true;
				underlyingTargeter.Target = child;
				wanderer.weight = 0.25f;
				break;
			case State.Wandering:
				child = null;
				targeter.enabled = false;
				wanderer.weight = 1.0f;
				movement.acceleration = 1.0f;
				movement.speed = 2.0f;
				wasChasing = false;
				break;
		}

		UpdateAnimation();
	}


	private void UpdateState()
	{
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
		    && (((Vector2)(transform.position) - (Vector2)(Player.transform.position)).magnitude > GiveUpDistance))
		{
			currentState = State.HeadingHome;	
		} 
		else if (timeSinceWentHome > GoHomeTimeout) 
		{	
			// Target the player
			targeter.enabled = true;
			underlyingTargeter.Target = Player;
			Vector2? target = targeter.GetTarget();
			
			if (target != null) 
			{	
				// Check if we're gonna chase.
				if (((((Vector2)(Player.transform.position) - (Vector2)(Home.transform.position)).magnitude < LeashLength)
				    || (((Vector2)(transform.position) - (Vector2)(Player.transform.position)).magnitude < GiveUpDistance))
					&& !PlayerInfo.IsUnderwater()) 
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
		if (other.gameObject.tag.Equals ("Player") && !PlayerInfo.IsInvulnerable()) {

			PlayerInfo.DecrementHealth();
			PlayerInfo.MakeInvulnerable();

			// Knock the player
			GameObject player = GameObject.FindGameObjectWithTag("Player");
			Vector2 knockDirection = ((Vector2)(player.transform.position - transform.position)).normalized;
			player.rigidbody2D.AddForce(knockDirection * KnockForce);
		}
	}


	private void LayEgg()
	{
		child = (GameObject)Instantiate(Egg, transform.position - Vector3.down, Quaternion.identity);
	}


	public void OnTriggerEnter2D(Collider2D other) 
	{
		CheckIfHitPlayer(other);
	}


	public void OnTriggerStay2D(Collider2D other) 
	{
		CheckIfHitPlayer(other);
	}
}
