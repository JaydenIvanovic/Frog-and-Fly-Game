using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NeuralNetSteering : SteeringBehaviour {
	
	public float updateFrequency = 0.2f;
	public ManagePen manager;
	public float minSteeringMag = 1.0f;
	public NeuralNet neuralNet;
	public GameObject selectedFly;
	public float InputFlickerPreventionFactor = 0.2f; // 0.0f means it will always choose the closest fly
	                                                  // A value of 0.2f means that the chosen fly won't change unless another fly is 20% closer
	private float updateTimer = 0.0f;
	private float[] netInput;
	
	public void Update() {

		updateTimer += Time.deltaTime;

		Vector2 inputVec = Vector2.zero;

		if (updateTimer > updateFrequency) {

			GameObject tempFly = manager.getClosestFly((Vector2)(transform.FindChild("Mouth").position));

			// Couldn't find any flies, possibly because we just started a new epoch and they haven't spawned yet
			if (tempFly != null) {

				if (selectedFly == null) {
					selectedFly = tempFly;
				}

				Vector2 posDiffTemp = (Vector2)(transform.FindChild("Mouth").position) - (Vector2)(tempFly.transform.position);
				Vector2 posDiffSelected = (Vector2)(transform.FindChild("Mouth").position) - (Vector2)(selectedFly.transform.position);

				if ((posDiffTemp.magnitude * (1.0f + InputFlickerPreventionFactor)) < posDiffSelected.magnitude) {
					selectedFly = tempFly;
					posDiffSelected = posDiffTemp;
				}

				// Make it so that closer flies send a stronger signal, but the signal is always between 0 and 1
				float inputMag = Mathf.Exp(-posDiffSelected.magnitude / 10.0f);

				inputVec = posDiffSelected.normalized * inputMag;
			}

			Vector2 posDiffSnake = (Vector2)(transform.position) - (Vector2)(manager.snake.transform.position);

			float inputMagSnake = Mathf.Exp(-posDiffSnake.magnitude / 5.0f);

			Vector2 inputVecSnake = posDiffSnake.normalized * inputMagSnake;

			if (!manager.snake.activeSelf) {
				inputVecSnake = Vector2.zero;
			}

			netInput = new float[]{inputVec.x, inputVec.y, inputVecSnake.x, inputVecSnake.y};

			updateTimer = 0.0f;
		}
	}

	public override Vector2 GetSteering()
	{
		if (netInput != null) {
			float[] netOutput = neuralNet.CalculateOutput(netInput);
			Vector2 steering = new Vector2(netOutput[0], netOutput[1]);
			//if (steering.magnitude < minSteeringMag) {
				//steering = steering.normalized * minSteeringMag;
			//}
			//return steering * 4.0f;
			return steering * 30.0f;
		} else {
			return Vector2.zero;
		}
	}
}