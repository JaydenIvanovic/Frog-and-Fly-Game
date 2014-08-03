using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class Flocking : MonoBehaviour 
{
	public float alignmentWeight = 0.1f;
	public float cohesionWeight = 0.1f;
	public float seperationWeight = 0.1f;
	public float neighbourDist = 30;
	public float defaultSpeed = 4f;
	private static List<GameObject> agents = new List<GameObject>();

	// Use this for initialization
	void Start () 
	{
		int i = Random.Range(0, 4);

		// Just randomize an initial velocity.
		switch (i)
		{
			case 0:
				gameObject.rigidbody.velocity = Vector3.left;
				break;
			case 1:
				gameObject.rigidbody.velocity = Vector3.right;
				break;
			case 2:
				gameObject.rigidbody.velocity = Vector3.up;
				break;
			case 3: 
				gameObject.rigidbody.velocity = Vector3.down;
				break;
		}

		agents.Add(gameObject);

		InvokeRepeating("randomizeDirection", 4f, 4f);
	}
	
	// Update is called once per frame
	void Update () 
	{
		Vector3 vel = computeAlignment() * alignmentWeight + computeCohesion() * cohesionWeight + computeSeperation() * seperationWeight;
		vel.Normalize();
		vel *= defaultSpeed;

		Debug.Log("RV: " + rigidbody.velocity);
		Debug.Log(vel.ToString());

		rigidbody.velocity = vel;
	}

	private Vector3 computeAlignment()
	{
		Vector3 velocity = Vector3.zero;
		uint neighbourCount = 0;

		foreach (GameObject agent in agents)
		{
			if (agent == gameObject)
				continue;

			if ( Vector3.Distance(agent.transform.position, transform.position) < neighbourDist )
			{
				Debug.Log("Found neighbour");
				velocity += agent.rigidbody.velocity;
				neighbourCount += 1;
			}
		}

		if (neighbourCount == 0)
			return velocity;

		velocity /= neighbourCount;
		velocity.Normalize();

		return velocity;
	}

	private Vector3 computeCohesion()
	{
		Vector3 velocity = Vector3.zero;
		uint neighbourCount = 0;
		
		foreach (GameObject agent in agents)
		{
			if (agent == gameObject)
				continue;
			
			if ( Vector3.Distance(agent.transform.position, transform.position) < neighbourDist )
			{
				Debug.Log("Found neighbour");
				velocity += agent.rigidbody.position;
				neighbourCount += 1;
			}
		}
		
		if (neighbourCount == 0)
			return velocity;
		
		velocity /= neighbourCount;
		Vector3 vectorToMass = velocity - transform.position;
		vectorToMass.Normalize();
		
		return vectorToMass;
	}

	private Vector3 computeSeperation()
	{
		Vector3 velocity = Vector3.zero;
		uint neighbourCount = 0;
		
		foreach (GameObject agent in agents)
		{
			if (agent == gameObject)
				continue;
			
			if ( Vector3.Distance(agent.transform.position, transform.position) < neighbourDist )
			{
				Debug.Log("Found neighbour");
				velocity += agent.rigidbody.position - transform.position;
				neighbourCount += 1;
			}
		}
		
		if (neighbourCount == 0)
			return velocity;
		
		velocity /= neighbourCount;
		velocity *= -1;
		velocity.Normalize();
		
		return velocity;
	}

	private void randomizeDirection()
	{
		Debug.Log("Randomizing Directions");
		foreach (GameObject agent in agents)
		{
			int i = Random.Range(0, 4);

			switch (i)
			{
				case 0:
					gameObject.rigidbody.velocity = Vector3.left;
					break;
				case 1:
					gameObject.rigidbody.velocity = Vector3.right;
					break;
				case 2:
					gameObject.rigidbody.velocity = Vector3.up;
					break;
				case 3: 
					gameObject.rigidbody.velocity = Vector3.down;
					break;
			}
		}
	}
}
