﻿using UnityEngine;
using System.Collections;

// Ensure that the screen fits a certain width and height (for the menu and death screens)
public class CameraMovementMinSize : MonoBehaviour {
	
	public float RequiredWidth = 10.0f;
	public float RequiredHeight = 10.0f;
	
	void Update () {

		Camera.main.orthographicSize = Mathf.Max(RequiredWidth / Camera.main.aspect, RequiredHeight);
	}
}