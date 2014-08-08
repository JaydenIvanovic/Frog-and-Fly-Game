using UnityEngine;
using System.Collections;

public struct SteeringOutput
{
	public Vector3 linearVel;
	public float angularVel;
	public bool ignore;
}

// Implementation of the seek and flee behaviours.
public class Seek : SteeringBehaviour
{
	private Movement move;
	private Transform target;
	public bool flee; // provide a switch in inspector window
	public float weight = 1f;

	protected void Awake()
	{
		move = GetComponent<Movement>();
		target = GetRandomAppleTree(); // our flies like fruit!
	}


	public override Vector2 GetSteering()
	{
		Vector2 targetDir;

		// Are we seeking or fleeing?
		if(!flee)
			targetDir = (target.position - transform.position).normalized;
		else
			targetDir = (transform.position - target.position).normalized;

		var targetVelocity = targetDir * move.acceleration * weight;

		return targetVelocity;
	}


	public Transform GetRandomAppleTree()
	{
		GameObject[] trees = GameObject.FindGameObjectsWithTag("AppleTree");

		return trees[Random.Range(0, 4)].transform;
	}
}
