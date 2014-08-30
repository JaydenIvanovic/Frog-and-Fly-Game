using UnityEngine;
using System.Collections;

public class DrawGUIFlyDemo : MonoBehaviour {
	
	public GUISkin skin;
	public GUIStyle pauseText;

	private bool isPaused;
	private bool avoidObstacles = true;

	void Start () 
	{
		isPaused = false;

		if (!PlayerPrefs.HasKey("AvoidObstacles")) {
			PlayerPrefs.SetInt("AvoidObstacles", avoidObstacles? 1 : -1);
		}

		UpdateFlies();
	}
	
	
	void Update ()
	{
		CheckForPause();
	}

	void UpdateFlies() {

		GameObject[] flys = GameObject.FindGameObjectsWithTag("Fly");
		foreach (GameObject fly in flys) {
			SteeringController sc = fly.GetComponent<SteeringController>();
			if (sc != null) {
				sc.avoidObstacles = (PlayerPrefs.GetInt("AvoidObstacles") == 1)? true : false;
			}
		}
	}
	
	void OnGUI () 
	{
		GUI.skin = skin;

		GUI.Box (new Rect (10, 10, 180, 90), "");

		if(GUI.Button(new Rect (20, 20, 160, 30), "Avoid obstacles: " + ((PlayerPrefs.GetInt("AvoidObstacles") == 1)? "On" : "Off"))) {
			PlayerPrefs.SetInt("AvoidObstacles", PlayerPrefs.GetInt("AvoidObstacles") * -1);
			UpdateFlies();
		}
		if(GUI.Button(new Rect (20, 60, 160, 30), "Restart demo")) {
			Application.LoadLevel (Application.loadedLevel);
		}

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