using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Invisibility : MonoBehaviour 
{
	public float visibleDistance;
	public GameObject flyPlayer;
	public GameObject smokePrefab;
	private List<Transform> flies;
	private SpriteRenderer frogRenderer;
	private SpriteRenderer tongueRenderer;
	private GameObject smokeInst;
	private bool visible;

	// Use this for initialization
	void Start () 
	{
		flies = new List<Transform>();
		foreach (Transform t in flyPlayer.GetComponentInChildren<Transform>()) {
			flies.Add(t);
		}

		frogRenderer = GetComponent<SpriteRenderer>();

		foreach (Transform t in GetComponentInChildren<Transform>()) {
			if (t.name == "Tongue") {
				tongueRenderer = t.gameObject.GetComponent<SpriteRenderer>();
			}
		}
	}
	
	// Update is called once per frame
	void Update () 
	{
		foreach (Transform fly in flies) {
			if (Vector2.Distance(fly.position, transform.position) <= visibleDistance) {
				frogRenderer.enabled = true;
				tongueRenderer.enabled = true;

				if(smokeInst == null)
					smokeInst = (GameObject)Instantiate(smokePrefab, new Vector3(transform.position.x, transform.position.y, transform.position.z - 1), Quaternion.identity);

				return;
			}
		}

		if(smokeInst)
			Destroy(smokeInst);

		frogRenderer.enabled = false;
		tongueRenderer.enabled = false;
	}
}
