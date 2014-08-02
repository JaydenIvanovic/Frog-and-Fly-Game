using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Movement))]
public class KeyboardMovement : MonoBehaviour 
{
	private Movement movement;

	// Use this for initialization
	void Start () 
	{
		movement = gameObject.GetComponent<Movement>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0f);
		movement.Move(targetVelocity);
	}
}
