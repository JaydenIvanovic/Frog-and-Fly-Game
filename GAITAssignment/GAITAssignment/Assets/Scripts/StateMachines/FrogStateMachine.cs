using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AStarTargeter))]
[RequireComponent(typeof(SpriteRenderer))]
public class FrogStateMachine : MonoBehaviour {

	private Animator animator;
	private SpriteRenderer spriteRenderer;
	private Animator tongueAnimator;
	private SpriteRenderer tongueSprite;
	private AStarTargeter targeter;

	public float InvulnerableFlickerFrequency = 8.0f;

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		tongueAnimator = GameObject.Find("Tongue").GetComponent<Animator>();
		tongueSprite = GameObject.Find("Tongue").GetComponent<SpriteRenderer>();
		targeter = GetComponent<AStarTargeter>();
	}
	
	// Update is called once per frame
	void Update () {

		if (PlayerInfo.IsUnderwater()) {
			animator.SetBool("Underwater", true);
		} else {
			animator.SetBool("Underwater", false);
		}

		// Defaults
		animator.enabled = true;
		spriteRenderer.enabled = true;
		tongueAnimator.enabled = true;
		tongueSprite.enabled = true;
		
		// Flicker when invulnerable
		if (PlayerInfo.IsInvulnerable()) {
			if (((int)(Time.unscaledTime * InvulnerableFlickerFrequency * 2.0f)) % 2 == 0) {
				animator.enabled = false;
				spriteRenderer.enabled = false;
				tongueAnimator.enabled = false;
				tongueSprite.enabled = false;
			}
		}

		Vector2? target = targeter.GetTarget();
	
		if (target != null) {
			animator.SetBool("Sitting", false);
		} else {
			animator.SetBool("Sitting", true);
		}
	}
}
