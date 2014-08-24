using UnityEngine;
using System.Collections;

public class DrawGUI : MonoBehaviour 
{
	private int heartSize = 20;
	private int heartSeparation = 10;

	public Sprite Heart;
	public Texture Fly;
	public Sprite Egg;
	public Sprite Water;
	public Sprite WaterMeter;
	public GUISkin skin;
	public GUIStyle pauseText;

	private Texture2D heartTex, eggTex, waterTex, waterMeterTex;
	private bool isPaused;

	void Start () 
	{
		isPaused = false;
		heartTex = SpriteToTexture(Heart);
		eggTex = SpriteToTexture(Egg);
		waterTex = SpriteToTexture(Water);
		waterMeterTex = SpriteToTexture(WaterMeter);
	}


	void Update ()
	{
		CheckForPause();
	}


	void OnGUI () 
	{
		GUI.skin = skin;

		GUI.Box (new Rect (10, 10, 100, 120), "");

		int health = PlayerInfo.GetHealth();

		for (int i = 0; i < health; i++) {
			GUI.DrawTexture(new Rect(20 + (heartSize + heartSeparation) * i, 20, heartSize, heartSize), heartTex, ScaleMode.ScaleToFit, true, 0.0f);
		}

		// This could probably be made better by using GUI groups. 
		GUI.DrawTexture(new Rect(20, 45, heartSize, heartSize), waterTex, ScaleMode.ScaleToFit, true, 0.0f);
		GUI.DrawTexture(new Rect(20, 70, heartSize, heartSize), Fly, ScaleMode.ScaleToFit, true, 0.0f);
		GUI.DrawTexture(new Rect(20, 90, heartSize, heartSize), eggTex, ScaleMode.ScaleToFit, true, 0.0f);

		GUI.DrawTexture(new Rect(50 - 2, 45 + 3, PlayerInfo.GetWaterLevel() / 2.0f, 14), waterMeterTex, ScaleMode.StretchToFill, true, 0.0f);
		GUI.Label (new Rect (40, 70, 120, 20), ": " + PlayerInfo.GetScore());
		GUI.Label (new Rect (40, 95, 120, 20), ": " + PlayerInfo.GetEggsDestroyed());

		// Draw the pause menu
		if (isPaused) {
			int menuWidth = 300;
			int menuHeight = 220;

			// Center the menu on the screen.
			GUI.BeginGroup(new Rect (Screen.width / 2 - menuWidth / 2, Screen.height / 2 - menuHeight / 2, menuWidth, menuHeight));
			GUI.Box (new Rect (0, 0, menuWidth, menuHeight), "");
			GUI.Label(new Rect (55, 30, 100, 30), "Game Paused", pauseText);
			// Draw the button which will take the player back to the main menu.
			// And handle the situation in which it is pressed.
			if(GUI.Button(new Rect (100, 70, 100, 30), "Main Menu")) {
				isPaused = false;
				Time.timeScale = 1;
				AStarTargeter.ClearGrids();
				Application.LoadLevel("Menu");
			}
			GUI.EndGroup();
		}
	}


	// Helper function to convert sprites to textures.
	// Follows the code from http://answers.unity3d.com/questions/651984/convert-sprite-image-to-texture.html
	private Texture2D SpriteToTexture(Sprite sprite)
	{
		// Create a new empty texture with the dimensions of the sprite image.
		Texture2D texture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
		// Get the pixels corresponding to this sprite from the sprite sheet.
		Color[] pixels = sprite.texture.GetPixels((int)sprite.textureRect.x, (int)sprite.textureRect.y, (int)sprite.textureRect.width, (int)sprite.textureRect.height);
		// Fill the new texture. 
		texture.SetPixels(pixels);
		// Must be called to set changes made via SetPixels.
		texture.Apply();

		return texture;
	}


	private void CheckForPause() 
	{
		if (Input.GetKeyDown(KeyCode.Escape)) {
			if (isPaused) {
				Time.timeScale = 1;
				isPaused = false;
			}
			else {
				Time.timeScale = 0;
				isPaused = true;
			}
		}
	}
}