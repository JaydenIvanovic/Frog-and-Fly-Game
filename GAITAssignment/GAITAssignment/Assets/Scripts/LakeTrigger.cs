using UnityEngine;
using System.Collections;

public class LakeTrigger : MonoBehaviour {
	
	public void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.tag == "Player")
			GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInfo>().SetUnderwater(true);
	}
	
	public void OnTriggerStay2D(Collider2D other) {
		if (other.gameObject.tag == "Player")
			GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInfo>().SetUnderwater(true);
	}
	
	public void OnTriggerExit2D(Collider2D other) {
		if (other.gameObject.tag == "Player")
			GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInfo>().SetUnderwater(false);
	}
}
