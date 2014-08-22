using UnityEngine;
using System.Collections;

public class DrawGUI : MonoBehaviour {

	private int heartSize = 20;
	private int heartSeparation = 10;

	public Texture Heart;

	void OnGUI () {

		GUI.Box (new Rect (10, 10, 130, 100), "");

		PlayerInfo info = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInfo>();

		int health = info.GetHealth();

		for (int i = 0; i < health; i++) {
			GUI.DrawTexture(new Rect(20 + (heartSize + heartSeparation) * i, 20, heartSize, heartSize), Heart, ScaleMode.ScaleToFit, true, 0.0f);
		}

		GUI.Label (new Rect (20, 50, 80, 20), "Flies eaten: " + info.GetScore());
		GUI.Label (new Rect (20, 65, 120, 20), "Eggs destroyed: " + info.GetEggsDestroyed());
		GUI.Label (new Rect (20, 80, 120, 20), "Water Level: " + (int)info.GetWaterLevel());
	}
}