using UnityEngine;
using System.Collections;

/* Simple implementation of a finite state machine for 
 * the fly game character. 
 */
public class FlyFSM : MonoBehaviour
{
	private State _currentState;
	private SteeringBehaviour _sb;

	public enum State
	{
		Searching,
		Eating,
		Fleeing,
		Baited
	}
	

	protected void Start()
	{
		_currentState = State.Searching;
		_sb = GetComponent<SteeringBehaviour>();
		rigidbody2D.velocity = Vector2.right;
		//_sb.selectedBehaviour = BType.Wander;
	}


	public State currentState
	{
		get {return _currentState;}
		set {_currentState = value;}
	}


	protected void Update()
	{
		// Switch to handle the different behaviours to be performed
		// dependent on the current state.
		switch (_currentState)
		{
			case State.Searching:
				_sb.selectedBehaviour = BType.Wander;
				break;
			case State.Eating:
				break;
			case State.Fleeing:
				break;
			case State.Baited:
				break;
		}
	}
}
