using UnityEngine;
using System.Collections;

public class FlyObstacleAvoid : MonoBehaviour 
{
	public float distToCol = 3f;

	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		RaycastHit2D hit = Physics2D.Raycast(transform.position, rigidbody2D.velocity.magnitude, distToCol); 
	}
}
