using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// TODO When the flock reaches a border they should move away from it 
// to stay within the world boundaries.
[RequireComponent(typeof(Rigidbody2D))]
public class Flocking : SteeringBehaviour 
{
	private const string BOUNDARIES = "boundaries";
	public float alignmentWeight = 0.1f;
	public float cohesionWeight = 0.1f;
	public float seperationWeight = 0.1f;
	public float neighbourDist = 30;
	private Movement movement;
	private static List<GameObject> agents = new List<GameObject>();

	private delegate Vector2 ReturnVector(GameObject agent);
	private delegate Vector2 Finalization(Vector2 velocity, uint neighbourCount);

	public static void DestroyFly(GameObject fly) 
	{
		agents.Remove (fly);
	}

	// Use this for initialization
	void Start () 
	{
		agents.Add(gameObject);
		movement = GetComponent<Movement>();
	}


	public override Vector2 GetSteering()
	{
		// Compute the new velocity, taking into consideration the weights of each behaviour.
		Vector2 vel = (computeAlignment() * alignmentWeight) + (computeCohesion() * cohesionWeight) + (computeSeperation() * seperationWeight);
		vel.Normalize();
		vel *= movement.speed;
		
		return vel;
	}


	// Computation to add to the velocity for the "Alignment" behaviour.
	private Vector2 alignmentVector(GameObject agent)
	{
		return agent.rigidbody2D.velocity;
	}

	
	// Final steps for the "Alignment" behaviour.
	private Vector2 alignmentFinalize(Vector2 velocity, uint neighbourCount)
	{
		velocity /= (float)neighbourCount;
		velocity.Normalize();
		
		return velocity;
	}


	// Computation to add to the velocity for the "Cohesion" behaviour.
	private Vector2 cohesionVector(GameObject agent)
	{
		return agent.rigidbody2D.position;
	}


	// Final steps for the "Cohesion" behaviour.
	private Vector2 cohesionFinalize(Vector2 velocity, uint neighbourCount)
	{
		velocity /= (float)neighbourCount;
		Vector2 vectorToMass = velocity - new Vector2(transform.position.x, transform.position.y);
		vectorToMass.Normalize();
		
		return vectorToMass;
	}


	// Computation to add to the velocity for the "Seperation" behaviour.
	private Vector2 seperationVector(GameObject agent)
	{
		return agent.rigidbody2D.position - new Vector2(transform.position.x, transform.position.y);
	}


	// Final steps for the "Seperation" behaviour.
	private Vector2 seperationFinalize(Vector2 velocity, uint neighbourCount)
	{
		velocity /= (float)neighbourCount;
		velocity *= -1;
		velocity.Normalize();
		
		return velocity;
	}


	// Main algorithmic 'formula' of alignment, cohesion and seperation.
	// The passed functions handle the minute differences.
	private Vector2 runAlgorithm(ReturnVector vecFunc, Finalization finalizeFunc)
	{
		Vector2 velocity = Vector2.zero;
		uint neighbourCount = 0;
		
		foreach (GameObject agent in agents)
		{
			if (agent == gameObject)
				continue;

			// Find neighbours of our agent to include in the calculation. 
			if ( Vector2.Distance(agent.transform.position, transform.position) < neighbourDist )
			{
				//Debug.Log("Found neighbour");
				velocity += vecFunc(agent);
				neighbourCount += 1;
			}
		}
		
		if (neighbourCount == 0)
			return velocity;
		
		return finalizeFunc(velocity, neighbourCount);
	}


	// Run the main algorithm performing the operations
	// required of the "Alignment" behaviour.
	private Vector2 computeAlignment()
	{
		ReturnVector vecFunc = alignmentVector;
		Finalization finalizeFunc = alignmentFinalize;

		return runAlgorithm(vecFunc, finalizeFunc);
	}


	// Run the main algorithm performing the operations
	// required of the "Cohesion" behaviour.
	private Vector2 computeCohesion()
	{
		ReturnVector vecFunc = cohesionVector;
		Finalization finalizeFunc = cohesionFinalize;
		
		return runAlgorithm(vecFunc, finalizeFunc);
	}


	// Run the main algorithm performing the operations
	// required of the "Seperation" behaviour.
	private Vector2 computeSeperation()
	{
		ReturnVector vecFunc = seperationVector;
		Finalization finalizeFunc = seperationFinalize;
		
		return runAlgorithm(vecFunc, finalizeFunc);
	}
}
