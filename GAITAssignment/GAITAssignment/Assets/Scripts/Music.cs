﻿using UnityEngine;
using System.Collections;

public class Music : MonoBehaviour {

	private AudioSource SoundSource;
	private int currentClip = 0;

	public AudioClip[] SoundClips;
	public float[] VolumeAdjustments;

	void Awake()
	{
		DontDestroyOnLoad(gameObject);
		SoundSource = gameObject.AddComponent<AudioSource>();
		SoundSource.loop = true;
	}
	
	void Start()
	{
		if (currentClip < SoundClips.Length) {
			SoundSource.clip = SoundClips[currentClip];
			SoundSource.volume = VolumeAdjustments[currentClip];
			SoundSource.Play();
		}
	}

	public void changeTrack() {

		SoundSource.Stop();
		currentClip = (currentClip + 1) % (SoundClips.Length + 1);

		if (currentClip < SoundClips.Length) {
			SoundSource.clip = SoundClips[currentClip];
			SoundSource.volume = VolumeAdjustments[currentClip];
			SoundSource.Play();
		}
	}

	public string getCurrentTrackName() {

		if (currentClip == SoundClips.Length) {
			return "Off";
		} else {
			return SoundClips[currentClip].name;
		}
	}
}
