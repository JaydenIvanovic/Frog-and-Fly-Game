using UnityEngine;
using System.Collections;

public class TunnelTransport : MonoBehaviour 
{
	public GameObject outTunnel; // The tunnel that this one leads to
	
	void Start () 
	{
	
	}
	

	void Update () 
	{
	
	}

	void OnCollisionEnter2D(Collision2D coll)
	{
		if (coll.gameObject.tag == "Player")
			coll.gameObject.transform.position = new Vector3(outTunnel.transform.position.x, outTunnel.transform.position.y - 1f, 0f);
	}
}
