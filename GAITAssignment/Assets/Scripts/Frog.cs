using UnityEngine;
using System.Collections;

public class Frog : MonoBehaviour 
{
	public float maxHealth = 100f;
	public float currentHealth = 100f;
	public float tongueLength = 2f;
	private uint fliesEaten;
	private float timeAlive;
	
	void Start () 
	{
		fliesEaten = 0;
		timeAlive = 0;
	}

	void Update () 
	{
		timeAlive += Time.deltaTime;
	}

	// Display frog properties to the player.
	void OnGUI()
	{
		GUI.Box(new Rect(5f, 5f, 110f, 90f), "Player Stats");
		GUI.Label(new Rect(10f, 30f, 80f, 50f), "Health: " + currentHealth);
		GUI.Label(new Rect(10f, 50f, 90f, 50f), "Flys Eaten: " + fliesEaten);
		GUI.Label(new Rect(10f, 70f, 90f, 50f), "Time Alive: " + (int)timeAlive);
	}

	void OnCollisionEnter2D(Collision2D coll)
	{
		if (coll.gameObject.tag == "Food")
		{
			fliesEaten++;
			Flocking.killFly(coll.gameObject);
		}
	}
}
