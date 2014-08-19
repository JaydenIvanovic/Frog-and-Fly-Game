using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class HuntTargeter : Targeter {

	public GameObject Target;
	public float flankingDistance = 5.0f;

	public override Vector2? GetTarget ()
	{
		if (Target == null) {
			return null;
		}

		bool isClosest = true;
		float targetDistance = (Target.transform.position - transform.position).magnitude;

		GameObject[] snakes = GameObject.FindGameObjectsWithTag("Predator");

		foreach (GameObject snake in snakes) {
			float snakeDistance = (Target.transform.position - snake.transform.position).magnitude;
			if (snakeDistance < targetDistance) {
				isClosest = false;
			}
		}

		if (isClosest) {
			return (Vector2?)(Target.transform.position);
		} else {

			Vector2 normPlayerVelocity = Target.rigidbody2D.velocity;

			normPlayerVelocity.Normalize();
			Vector2 posDifference = (Vector2)(Target.transform.position - transform.position);

			Vector2 normPosDifference = posDifference.normalized;
			float cosTheta = Vector2.Dot(normPlayerVelocity, normPosDifference);

			float compAlongVelocity = Vector2.Dot(posDifference, normPlayerVelocity);

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
