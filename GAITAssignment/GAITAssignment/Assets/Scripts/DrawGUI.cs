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
	public GUISkin skin;
	private Texture2D heartTex, eggTex, waterTex; 

	void Start () 
	{
		heartTex = SpriteToTexture(Heart);
		eggTex = SpriteToTexture(Egg);
		waterTex = SpriteToTexture(Water);
	}


	void OnGUI () 
	{
		GUI.skin = skin;

		GUI.Box (new Rect (10, 10, 100, 120), "");

		PlayerInfo info = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInfo>();

		int health = info.GetHealth();

		for (int i = 0; i < health; i++) {
			GUI.DrawTexture(new Rect(20 + (heartSize + heartSeparation) * i, 20, heartSize, heartSize), heartTex, ScaleMode.ScaleToFit, true, 0.0f);
		}

		// This could probably be made better by using GUI groups. 
		GUI.DrawTexture(new Rect(20, 45, heartSize, heartSize), Fly, ScaleMode.ScaleToFit, true, 0.0f);
		GUI.DrawTexture(new Rect(20, 70, heartSize, heartSize), eggTex, ScaleMode.ScaleToFit, true, 0.0f);
		GUI.DrawTexture(new Rect(20, 95, heartSize, heartSize), waterTex, ScaleMode.ScaleToFit, true, 0.0f);

		GUI.Label (new Rect (40, 45, 120, 20), ": " + info.GetScore());
		GUI.Label (new Rect (40, 70, 120, 20), ": " + info.GetEggsDestroyed());
		GUI.Label (new Rect (40, 95, 120, 20), ": " + (int)info.GetWaterLevel());
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
}