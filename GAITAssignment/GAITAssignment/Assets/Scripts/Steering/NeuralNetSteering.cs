using UnityEngine;
using System.Collections;

public class NeuralNetSteering : SteeringBehaviour {
	
	public float updateFrequency = 0.2f;
	public SpawnDumbFlies flyManager;
	public float minSteeringMag = 1.0f;

	private float updateTimer = 0.0f;
	private NeuralNet net;
	private float[] netInput;

	public void Start() {
		net = new NeuralNet(2, 2);
	}
	
	public void Update() {

		updateTimer += Time.deltaTime;

		if (updateTimer > updateFrequency) {

			Vector2 closestFlyPos = (Vector2)(transform.position);

			if (flyManager != null) {
				closestFlyPos = (Vector2)(flyManager.getClosestFly((Vector2)(transform.position)).transform.position);
			}

			Vector2 positionDifference = (Vector2)(transform.position) - closestFlyPos;

			float inputMag = Mathf.Clamp(10.0f - positionDifference.magnitude, 0.0f, 10.0f) / 10.0f;

			Vector2 inputVec = positionDifference.normalized * inputMag;

			netInput = new float[]{inputVec.x, inputVec.y};
			updateTimer = 0.0f;
		}
	}

	public override Vector2 GetSteering()
	{
		if (netInput != null) {
			float[] netOutput = net.CalculateOutput(netInput);
			Vector2 steering = new Vector2(netOutput[0], netOutput[1]);
			if (steering.magnitude < minSteeringMag) {
				steering = steering.normalized * minSteeringMag;
			}
			return steering;
		} else {
			return Vector2.zero;
		}
	}
}