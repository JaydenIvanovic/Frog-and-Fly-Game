using UnityEngine;
using System.Collections;

public class AppleTreeTargeter : Targeter {

	private Vector2 treePosition;

	public void Start() {
		GameObject[] trees = GameObject.FindGameObjectsWithTag("AppleTree");
		treePosition = trees[Random.Range(0, 4)].transform.position;
	}

	public override Vector2? GetTarget ()
	{
		return (Vector2?)treePosition;
	}
}
