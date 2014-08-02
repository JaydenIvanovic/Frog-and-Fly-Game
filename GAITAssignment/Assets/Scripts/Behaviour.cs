using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Movement))]
public class Behaviour : MonoBehaviour 
{
	private Steering steeringSeek;
	public GameObject seekObject, fleeObject;
	// Use this for initialization
	void Start () 
	{
		steeringSeek = new Steering(transform.position, seekObject.transform.position);
		//steeringFlee = new Steering(fleeObject.transform.position, transform.position);
		rigidbody.velocity = Vector3.left *2f;
	}
	
	// Update is called once per frame
	void Update () 
	{
		steeringSeek.updatePlayerPosition(transform.position);
		SteeringOutput so = steeringSeek.getSteering();

		transform.position += rigidbody.velocity * Time.deltaTime;
		//rigidbody.rotation += rigidbody.angularVelocity;

		// Update the rigidbody velocities.
		rigidbody.velocity += so.linearVel * Time.deltaTime;
		// Must convert from radians to a vector.
		//rigidbody.angularVelocity += new Vector3(-Mathf.Sin(so.angularVel), Mathf.Cos(so.angularVel), 0f) * Time.deltaTime;
	}
}
