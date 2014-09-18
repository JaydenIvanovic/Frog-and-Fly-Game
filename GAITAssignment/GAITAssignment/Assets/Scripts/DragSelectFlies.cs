﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DragSelectFlies : MonoBehaviour 
{
	private List<GameObject> flies;
	private bool drawing;
	private Vector2 startPos;

	// Use this for initialization
	void Start () 
	{
		drawing = false;
	}
	

	// Update is called once per frame
	void Update () 
	{
		if(Input.GetMouseButtonDown(0)) {
			if(!drawing) {
				drawing = true;
				startPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);			
			}
		}

		if(drawing) {
			DrawBox(startPos, Camera.main.ScreenToWorldPoint(Input.mousePosition));
		}

		if (Input.GetMouseButtonUp(0)) {
			// Calculate the boundaries that select flies will fall into.
			Vector2 endPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			float minx = Mathf.Min(startPos.x, endPos.x);
			float maxx = Mathf.Max(startPos.x, endPos.x);
			float miny = Mathf.Min(startPos.y, endPos.y);
			float maxy = Mathf.Max(startPos.y, endPos.y);
			// Update select flies using the calcaluate min and max boundaries.
			UpdateSelectedFlies(new Vector2(minx, miny), new Vector2(maxx, maxy));
			// Were done selecting so cleanup.
			drawing = false;
			GetComponent<MeshFilter>().mesh.Clear();
		}
	}


	// Draw the graphical representation of the selector.
	private void DrawBox(Vector2 start, Vector2 end)
	{
		//Debug.DrawLine(start, end, Color.white,0.0f, true);
		Mesh mesh = new Mesh();

		Vector3[] verts = new Vector3[4];
		int[] triangles = new int[6];
		Color32[] color = new Color32[4];

		verts[0] = new Vector3(start.x, start.y, -1f);
		verts[1] = new Vector3(end.x, end.y, -1f);
		verts[2] = new Vector3(start.x, end.y, -1f);
		verts[3] = new Vector3(end.x, start.y, -1f);

		triangles[0] = 0;
		triangles[1] = 1;
		triangles[2] = 2;
		triangles[3] = 1; // if 0 and 
		triangles[4] = 0; // if 1 then it doesn't work, which confuses me.
		triangles[5] = 3;

		color[0] = new Color(0f, 1f, 0.3f, 0.5f);
		color[1] = new Color(0f, 1f, 0.3f, 0.5f);
		color[2] = new Color(0f, 1f, 0.3f, 0.5f);
		color[3] = new Color(0f, 1f, 0.3f, 0.5f);

		mesh.vertices = verts;
		mesh.colors32 = color;
		mesh.triangles = triangles;

		GetComponent<MeshFilter>().mesh = mesh;
	}


	// See if the fly objects of this parent are within
	// the box boundaries. If so, set their mouse property to selected.
	private void UpdateSelectedFlies(Vector2 boundsStart, Vector2 boundsEnd)
	{
		foreach (Transform fly in GetComponentInChildren<Transform>()) {
			if (fly.name == "PlayerFly") {
				// Check if fly's current position is within the boundaries.
				if (fly.transform.position.x >= boundsStart.x && fly.transform.position.x <= boundsEnd.x &&
					fly.transform.position.y >= boundsStart.y && fly.transform.position.y <= boundsEnd.y) {
					fly.GetComponent<MouseTargeter>().selected = true;
				}
				else {
					fly.GetComponent<MouseTargeter>().selected = false;
				}
			}
		}
	}
}