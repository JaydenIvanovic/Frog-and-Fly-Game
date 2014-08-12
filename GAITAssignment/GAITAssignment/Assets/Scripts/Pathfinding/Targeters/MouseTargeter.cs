using UnityEngine;
using System.Collections;

public class MouseTargeter : Targeter {
	
	private Vector2? _target = null;
	
	// Update is called once per frame
	void Update () {

		// Use right-click to move
		if(Input.GetMouseButtonDown(1)) {
			Vector2 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			_target = clickPos;
		}
	}

	public override Vector2? GetTarget ()
	{
		return _target;
	}
}
