using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NeuralNetSteering : SteeringBehaviour {
	
	public float updateFrequency = 0.2f;
	public SpawnDumbFlies flyManager;
	public float minSteeringMag = 1.0f;
	public NeuralNet neuralNet;
	public GameObject selectedFly;

	public List<float> weightsAsVector; // For checking in the inspector

	private float updateTimer = 0.0f;
	private float[] netInput;
	
	public void Awake() {
		neuralNet = new NeuralNet(2, 2);
	}
	
	public void Update() {

		weightsAsVector = new List<float>();

		for (int i = 0; i < neuralNet.weights[0].Length; i++) {
			weightsAsVector.Add(neuralNet.weights[0][i]);
		}
		for (int i = 0; i < neuralNet.weights[1].Length; i++) {
			weightsAsVector.Add(neuralNet.weights[1][i]);
		}

		updateTimer += Time.deltaTime;

		if (updateTimer > updateFrequency) {

			// Don't constantly change the selected fly cos it'll tend to make the neural net input flicker heaps
			if (selectedFly == null) {
				selectedFly = flyManager.getClosestFly((Vector2)(transform.FindChild("Mouth").position));
			}

			Vector2 selectedFlyPos = (Vector2)(selectedFly.transform.position);

			Vector2 positionDifference = (Vector2)(transform.FindChild("Mouth").position) - selectedFlyPos;

			float inputMag = Mathf.Clamp(10.0f - positionDifference.magnitude, 0.0f, 10.0f) / 10.0f;

			Vector2 inputVec = positionDifference.normalized * inputMag;

			netInput = new float[]{inputVec.x, inputVec.y};
			updateTimer = 0.0f;
		}
	}

	public override Vector2 GetSteering()
	{
		if (netInput != null) {
			float[] netOutput = neuralNet.CalculateOutput(netInput);
			Vector2 steering = new Vector2(netOutput[0], netOutput[1]);
			if (steering.magnitude < minSteeringMag) {
				steering = steering.normalized * minSteeringMag;
			}
			return steering * 4.0f;
		} else {
			return Vector2.zero;
		}
	}
}