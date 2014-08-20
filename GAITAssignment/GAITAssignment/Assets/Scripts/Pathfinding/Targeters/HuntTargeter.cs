using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
[RequireComponent(typeof(Flocking))]
public class HuntTargeter : Targeter {
	
	public GameObject Target;
	public float flankingDistance = 5.0f;
	public float attackDistance = 3.0f;
	public int maxAttackerCount = 2;
	public bool dumbAttack = false;

	private Flocking flocker;

	void Awake() {
		flocker = GetComponent<Flocking>();
	}

	public override Vector2? GetTarget ()
	{
		// Default
		flocker.seperationWeight = 0.2f;

		if (Target == null) {
			return null;
		}
		
		// Player velocity
		Vector2 normPlayerVelocity = Target.rigidbody2D.velocity;
		
		// All attack if the player is still
		if (normPlayerVelocity == Vector2.zero) {
			//
			return (Vector2?)(Target.transform.position);
		}
		
		normPlayerVelocity.Normalize();
		
		// Vector to player
		Vector2 posDifference = (Vector2)(Target.transform.position - transform.position);
		Vector2 normPosDifference = posDifference.normalized;
		float cosTheta = Vector2.Dot(normPlayerVelocity, normPosDifference);
		
		// Component of vector to player that's in the direction of the player's velocity.
		// If it's positive then we're chasing, otherwise we're coming head-on
		float compAlongVelocity = Vector2.Dot(posDifference, normPlayerVelocity);
		
		int rank = 0;
		GameObject[] snakes = GameObject.FindGameObjectsWithTag("Predator");
		
		foreach (GameObject snake in snakes) {
			
			// Vector from other snake to player
			Vector2 posDifferenceOther = (Vector2)(Target.transform.position - snake.transform.position);
			Vector2 normPosDifferenceOther = posDifferenceOther.normalized;
			float cosThetaOther = Vector2.Dot(normPlayerVelocity, normPosDifferenceOther);
			
			// Make closest snake from both sides attack
			if ((posDifferenceOther.magnitude < posDifference.magnitude) && (Mathf.Sign(cosTheta) == Mathf.Sign(cosThetaOther))) {
				rank++;
			}
		}

		/* TO DO: I really need to tidy up this code - for now I just got it working! */

		if (dumbAttack || (rank == 0) || ((posDifference.magnitude < attackDistance) && (rank < maxAttackerCount))) {
			// Turn on for visual debugging
			//this.transform.localScale = new Vector3(3.0f, 3.0f, 3.0f);
			if (dumbAttack || (rank == 0)) {
				flocker.seperationWeight = 0.0f;
			}
			return (Vector2?)(Target.transform.position);
		} else {
			// Turn on for visual debugging
			//this.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
			// If we're approaching head-on at less than 45 degrees, just target the player
			if ((cosTheta <= (-1.0f / Mathf.Sqrt(2.0f))) && (cosTheta >= -1.0f)) {
				return (Vector2?)(Target.transform.position);
			} else if (cosTheta < 0.0f) {
				if (posDifference.magnitude < flankingDistance) {
					// We're in a good flanking position so turn and move with the player
					compAlongVelocity *= -1.0f;
				} else {
					// Try to get closer before flanking
					return (Vector2?)(Target.transform.position);
				}
			}
			
			Vector2 vecAlongVelocity = normPlayerVelocity * compAlongVelocity;
			
			return (Vector2?)(transform.position) + (Vector2?)vecAlongVelocity;
		}
	}
}
