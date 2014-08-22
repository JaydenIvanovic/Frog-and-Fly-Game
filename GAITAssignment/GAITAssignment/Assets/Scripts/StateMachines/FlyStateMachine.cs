﻿using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AppleTreeTargeter))]
[RequireComponent(typeof(GameObjectTargeter))]
[RequireComponent(typeof(Seek))]
[RequireComponent(typeof(Movement))]
public class FlyStateMachine : MonoBehaviour {

	private enum State {
		Eating,
		SeekFood, 
		Fleeing
	}

	private Targeter appleTreeTargeter;
	private GameObjectTargeter playerTargeter;
	private Seek seekComponent;
	private Movement movement;
	private State currentState;
	private float timeEating;
	private bool doneEating;

	public float fleeDistance;
	public float fleeSpeed;
	public float appleTreeSpeed;
	public float appleTreeDist;
	public float maxEatTime;

	// Use this for initialization
	void Start () {
		appleTreeTargeter = (AppleTreeTargeter)GetComponent<AppleTreeTargeter>();
		playerTargeter = (GameObjectTargeter)GetComponent<GameObjectTargeter>();
		seekComponent = (Seek)GetComponent<Seek>();
		movement = (Movement)GetComponent<Movement>();

		playerTargeter.Target = GameObject.Find("Player");

		ResetEatingVars();
	}
	
	// Update is called once per frame
	void Update () {
		UpdateState();

		switch (currentState)
		{
			case State.Fleeing:
				seekComponent.flee = true;
				seekComponent.SetTargeter(playerTargeter);
				movement.speed = fleeSpeed;
				break;
			case State.SeekFood:
				seekComponent.flee = false;
				seekComponent.SetTargeter(appleTreeTargeter);
				movement.speed = appleTreeSpeed;
				break;
			case State.Eating:
				timeEating += Time.deltaTime;
				if (timeEating >= maxEatTime)
				{
					doneEating = true;
					((AppleTreeTargeter)appleTreeTargeter).UpdateTree();
				}
				break;
		}
	}

	// Determine the flies current state.
	private void UpdateState() {
		float distanceFromPlayer;
		Vector2? playerPos = playerTargeter.GetTarget();
		
		if (playerPos == null) {
			distanceFromPlayer = float.MaxValue;
		} else {
			distanceFromPlayer = ((Vector2)(transform.position) - (Vector2)(playerTargeter.GetTarget())).magnitude;
		}

		float distanceFromAppleTree = Vector2.Distance((Vector2)appleTreeTargeter.GetTarget(), (Vector2)transform.position);

		// Only if the player is close and not underwater.
		if (distanceFromPlayer < fleeDistance && !GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInfo>().IsUnderwater()) {
			currentState = State.Fleeing;
			ResetEatingVars();
			//Random chance to change the tree the fly will visit to eat.
			if(Random.Range(0,2) == 0) 
				((AppleTreeTargeter)appleTreeTargeter).UpdateTree();
		} else if ( (distanceFromAppleTree < appleTreeDist) && !doneEating ){
			currentState = State.Eating;
		} else {
			currentState = State.SeekFood;
			ResetEatingVars();
		}
	}

	private void ResetEatingVars()
	{
		doneEating = false;
		timeEating = 0f;
	}
}
