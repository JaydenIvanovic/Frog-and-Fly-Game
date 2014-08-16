using UnityEngine;
using System.Collections;

[System.Serializable]
public class GameObjectTargeter : Targeter {

	public GameObject Target;

	public GameObjectTargeter(GameObject obj)
	{
		Target = obj;
	}

	public override Vector2? GetTarget ()
	{
		if (Target == null) {
			return null;
		} else {
			return Target.transform.position;
		}
	}

	public void SetTarget(GameObject newTarget)
	{
		Target = newTarget;
	}
}
