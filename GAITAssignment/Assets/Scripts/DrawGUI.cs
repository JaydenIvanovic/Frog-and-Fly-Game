using UnityEngine;
using System.Collections;

public class DrawGUI : MonoBehaviour {
	
	void OnGUI () {
		GUI.Box (new Rect (10, 10, 160, 40), "");
		GUI.Label (new Rect (20, 20, 140, 20), "Flies eaten: " + PlayerInfo.GetScore());
	}
}
