using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AStarTargeter))]
public class FrogStateMachine : MonoBehaviour {

	private Animator animator;
	private AStarTargeter targeter;

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator>();
		targeter = GetComponent<AStarTargeter>();
	}
	
	// Update is called once per frame
	void Update () {

		Vector2? target = targeter.GetTarget();
	
		if (target != null) {
			animator.SetBool("Sitting", false);
		} else {
			animator.SetBool("Sitting", true);
		}
	}
}
