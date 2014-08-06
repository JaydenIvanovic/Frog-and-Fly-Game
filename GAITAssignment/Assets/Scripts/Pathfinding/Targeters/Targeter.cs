using UnityEngine;
using System.Collections;

public enum TargeterType
{
	Mouse = 0,
	AStar = 1
};

public abstract class Targeter : MonoBehaviour {
	public abstract Vector2? GetTarget();
}
