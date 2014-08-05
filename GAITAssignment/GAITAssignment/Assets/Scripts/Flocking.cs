using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// TODO When the flock reaches a border they should move away from it 
// to stay within the world boundaries.
[RequireComponent(typeof(Rigidbody))]
public class Flocking : MonoBehaviour 
{
	private const string BOUNDARIES = "boundaries";
	public float alignmentWeight = 0.1f;
	public float cohesionWeight = 0.1f;
	public float seperationWeight = 0.1f;
	public float neighbourDist = 30;
	public float defaultSpeed = 4f;
	public float maxVelocity = 7f;
	private static List<GameObject> boundaries = new List<GameObject>();
	private static List<GameObject> agents = new List<GameObject>();

	private delegate Vector3 ReturnVector(GameObject agent);
	private delegate Vector3 Finalization(Vector3 velocity, uint neighbourCount);

	// Use this for initialization
	void Start () 
	{
		// Get boundaries: TODO when the flock is near the boundary, alter trajectory.
		//boundaries.AddRange(GameObject.FindGameObjectsWithTag(BOUNDARIES));

		agents.Add(gameObject);

		InvokeRepeating("randomizeDirection", 0f, 3f);
	}


	// Update is called once per frame
	void Update () 
	{
		// Compute the new velocity, taking into consideration the weights of each behaviour.
		Vector3 vel = (computeAlignment() * alignmentWeight) + (computeCohesion() * cohesionWeight) + (computeSeperation() * seperationWeight);
		vel.Normalize();
		vel *= defaultSpeed;

		rigidbody.velocity += vel;

		// Cap rigidbody velocity.
		if(rigidbody.velocity.magnitude > maxVelocity)
			rigidbody.velocity = rigidbody.velocity.normalized * maxVelocity;
	}


	// Computation to add to the velocity for the "Alignment" behaviour.
	private Vector3 alignmentVector(GameObject agent)
	{
		return agent.rigidbody.velocity;
	}

	
	// Final steps for the "Alignment" behaviour.
	private Vector3 alignmentFinalize(Vector3 velocity, uint neighbourCount)
	{
		velocity /= (float)neighbourCount;
		velocity.Normalize();
		
		return velocity;
	}


	// Computation to add to the velocity for the "Cohesion" behaviour.
	private Vector3 cohesionVector(GameObject agent)
	{
		return agent.rigidbody.position;
	}


	// Final steps for the "Cohesion" behaviour.
	private Vector3 cohesionFinalize(Vector3 velocity, uint neighbourCount)
	{
		velocity /= (float)neighbourCount;
		Vector3 vectorToMass = velocity - transform.position;
		vectorToMass.Normalize();
		
		return vectorToMass;
	}


	// Computation to add to the velocity for the "Seperation" behaviour.
	private Vector3 seperationVector(GameObject agent)
	{
		return agent.rigidbody.position - transform.position;
	}


	// Final steps for the "Seperation" behaviour.
	private Vector3 seperationFinalize(Vector3 velocity, uint neighbourCount)
	{
		velocity /= (float)neighbourCount;
		velocity *= -1;
		velocity.Normalize();
		
		return velocity;
	}


	// Main algorithmic 'formula' of alignment, cohesion and seperation.
	// The passed functions handle the minute differences.
	private Vector3 runAlgorithm(ReturnVector vecFunc, Finalization finalizeFunc)
	{
		Vector3 velocity = Vector3.zero;
		uint neighbourCount = 0;
		
		foreach (GameObject agent in agents)
		{
			if (agent == gameObject)
				continue;

			// Find neighbours of our agent to include in the calculation. 
			if ( Vector3.Distance(agent.transform.position, transform.position) < neighbourDist )
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
	private Vector3 computeAlignment()
	{
		ReturnVector vecFunc = alignmentVector;
		Finalization finalizeFunc = alignmentFinalize;

		return runAlgorithm(vecFunc, finalizeFunc);
	}


	// Run the main algorithm performing the operations
	// required of the "Cohesion" behaviour.
	private Vector3 computeCohesion()
	{
		ReturnVector vecFunc = cohesionVector;
		Finalization finalizeFunc = cohesionFinalize;
		
		return runAlgorithm(vecFunc, finalizeFunc);
	}


	// Run the main algorithm performing the operations
	// required of the "Seperation" behaviour.
	private Vector3 computeSeperation()
	{
		ReturnVector vecFunc = seperationVector;
		Finalization finalizeFunc = seperationFinalize;
		
		return runAlgorithm(vecFunc, finalizeFunc);
	}


	// Get the group to move in a new direction. Add a bit of randomization
	// so they don't continually go in the same direction.
	public void randomizeDirection()
	{
		int i = Random.Range(0, 4);

		Debug.Log("New direction: " + i);

		switch (i)
		{
			case 0:
				rigidbody.velocity = Vector3.left * defaultSpeed;
				break;
			case 1:
				rigidbody.velocity = Vector3.right * defaultSpeed;
				break;
			case 2:
				rigidbody.velocity = Vector3.up * defaultSpeed;
				break;
			case 3: 
				rigidbody.velocity = Vector3.down * defaultSpeed;
				break;
		}
	}
}
