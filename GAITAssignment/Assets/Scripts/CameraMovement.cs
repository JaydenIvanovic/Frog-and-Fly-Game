using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Transform))]
public class CameraMovement : MonoBehaviour {

	public Transform _player;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		transform.position = new Vector3(_player.position.x, _player.position.y, transform.position.z);
	}
}
