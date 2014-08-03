using UnityEngine;
using System.Collections;

public struct SteeringOutput
{
	public Vector3 linearVel;
	public float angularVel;
	public bool ignore;
}

public class Steering 
{
	private Vector3 _character;
	private Vector3 _target;
	public float maxAcceleration = 1.2f;

	public Vector3 character
	{
		get{return _character;}
		set{_character = value;}
	}

	public Vector3 target
	{
		get{return _target;}
		set{_target = value;}
	}

	public Steering(Vector3 character, Vector3 target)
	{
		this._character = character;
		this._target = target;
	}

	public virtual SteeringOutput getSteering()
	{
		var steering = new SteeringOutput();

		// Get direction to target and make it a unit length
		// vector before heading there at maximum speed.
		steering.linearVel = _target - _character;
		steering.linearVel.Normalize();
		steering.linearVel *= maxAcceleration;

		// As we are working in 2D, ensure z is 0
		steering.linearVel.z = 0f;

		// The angular velocity will be handled by other
		// behaviours such as 'align'.
		steering.angularVel = 0f;

		steering.ignore = false;

		return steering;
	}

	public void updatePlayerPosition(Vector3 position)
	{
		_character = position;
	}
}
