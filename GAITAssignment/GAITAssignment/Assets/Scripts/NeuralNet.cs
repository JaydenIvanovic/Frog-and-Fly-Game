using UnityEngine;
using System.Collections;

[System.Serializable]
public class NeuralNet {

	public float bias = 0.0f;
	public int hiddenLayerNeurons = 4;

	public int inputNeurons;
	public int outputNeurons;

	public float[][] neuronValues = new float[3][]; // For storing calculated values
	public float[][] weights = new float[2][];

	public NeuralNet(int inputNeurons, int outputNeurons) {

		this.inputNeurons = inputNeurons;
		this.outputNeurons = outputNeurons;

		neuronValues[0] = new float[inputNeurons];
		neuronValues[1] = new float[hiddenLayerNeurons];
		neuronValues[2] = new float[outputNeurons];

		weights[0] = new float[(inputNeurons + 1) * hiddenLayerNeurons]; // +1 for the bias
		for (int i = 0; i < weights[0].Length; i++) {
			weights[0][i] = Random.value - 0.5f;
		}

		weights[1] = new float[(hiddenLayerNeurons + 1) * outputNeurons]; // +1 for the bias
		for (int i = 0; i < weights[1].Length; i++) {
			weights[1][i] = Random.value - 0.5f;
		}
	}

	public float[] CalculateOutput(float[] inputValues) {

		float[] result = new float[outputNeurons];

		if (inputValues.Length != inputNeurons) {
			Debug.Log("ERROR: Wrong number of inputs provided to neural net!");
			return result;
		}

		for (int i = 0; i < inputValues.Length; i++) {
			neuronValues[0][i] = inputValues[i];
		}

		// Hidden layer
		for (int i = 0; i < hiddenLayerNeurons; i++) {

			neuronValues[1][i] = bias * weights[0][i * (inputNeurons + 1)];

			for (int j = 0; j < inputNeurons; j++) {
				neuronValues[1][i] += neuronValues[0][j] * weights[0][i * (inputNeurons + 1) + j + 1];
			}

			// Squash between -1 and 1
			neuronValues[1][i] = Squash(neuronValues[1][i]);
		}

		// Output
		for (int i = 0; i < outputNeurons; i++) {
			
			neuronValues[2][i] = bias * weights[1][i * (hiddenLayerNeurons + 1)];
			
			for (int j = 0; j < hiddenLayerNeurons; j++) {
				neuronValues[2][i] += neuronValues[1][j] * weights[1][i * (hiddenLayerNeurons + 1) + j + 1];
			}
			
			// Squash between -1 and 1
			neuronValues[2][i] = Squash(neuronValues[2][i]);
		}

		return neuronValues[2];
	}

	private float Squash(float unsquashedValue) {
		return 2.0f / (1.0f + Mathf.Exp(unsquashedValue)) - 1.0f;
	}
}
