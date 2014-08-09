﻿using UnityEngine;
using System.Collections;

[System.Serializable]
public class GameObjectTargeter : Targeter {

	public GameObject Target;

	public override Vector2? GetTarget ()
	{
		if (Target == null) {
			return null;
		} else {
			return Target.transform.position;
		}
	}
}
