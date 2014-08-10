using UnityEngine;
using System.Collections;

public class DrawGUI : MonoBehaviour {

	private int heartSize = 20;
	private int heartSeparation = 10;

	public Texture Heart;

	void OnGUI () {

		GUI.Box (new Rect (10, 10, 100, 70), "");

		int health = PlayerInfo.GetHealth();

		for (int i = 0; i < health; i++) {
			GUI.DrawTexture(new Rect(20 + (heartSize + heartSeparation) * i, 20, heartSize, heartSize), Heart, ScaleMode.ScaleToFit, true, 0.0f);
		}

		GUI.Label (new Rect (20, 50, 80, 20), "Flies eaten: " + PlayerInfo.GetScore());
	}
}