using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class NeuralNet : System.ICloneable {

	public GameObject ParentFrog;
	public float fitness = 0.0f;

	public float bias = 0.0f;
	
	public int inputNeurons;
	public int hiddenLayerNeurons;
	public int outputNeurons;

	public float[][] neuronValues = new float[3][]; // For storing calculated values
	public float[][] weights = new float[2][];

	public List<float> weightsAsVector; // For viewing in the inspector
	
	public System.Object Clone() {

		NeuralNet clone = new NeuralNet(inputNeurons, hiddenLayerNeurons, outputNeurons);
		clone.bias = bias;
		clone.weights = new float[][]{(float[])(weights[0].Clone()), (float[])(weights[1].Clone())}; // Deep copy!
		return (System.Object)clone;
	}

	public void UpdateDisplayWeights() {

		weightsAsVector = new List<float>();
		
		for (int i = 0; i < weights[0].Length; i++) {
			weightsAsVector.Add(weights[0][i]);
		}
		for (int i = 0; i < weights[1].Length; i++) {
			weightsAsVector.Add(weights[1][i]);
		}
	}

	public void RandomiseWeights() {

		weights[0] = new float[(inputNeurons + 1) * hiddenLayerNeurons]; // +1 for the bias
		for (int i = 0; i < weights[0].Length; i++) {
			weights[0][i] = Random.value - 0.5f;
		}
		
		weights[1] = new float[(hiddenLayerNeurons + 1) * outputNeurons]; // +1 for the bias
		for (int i = 0; i < weights[1].Length; i++) {
			weights[1][i] = Random.value - 0.5f;
		}

		UpdateDisplayWeights();
	}

	public NeuralNet(int inputNeurons, int hiddenLayerNeurons, int outputNeurons) {

		this.inputNeurons = inputNeurons;
		this.hiddenLayerNeurons = hiddenLayerNeurons;
		this.outputNeurons = outputNeurons;

		neuronValues[0] = new float[inputNeurons];
		neuronValues[1] = new float[hiddenLayerNeurons];
		neuronValues[2] = new float[outputNeurons];
	
		RandomiseWeights();
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

	public int GetRandomCrossOverIndex() {

		List<int> crossOverPoints = new List<int>();
		int counter = 0;

		// Hidden layer
		for (int i = 0; i < hiddenLayerNeurons; i++) {
			crossOverPoints.Add(counter);
			counter += (inputNeurons + 1);
		}
		
		// Output
		for (int i = 0; i < outputNeurons; i++) {
			crossOverPoints.Add(counter);
			counter += (hiddenLayerNeurons + 1);
		}

		// Don't crossover right at the start because then it's not really a crossover
		crossOverPoints.Remove(0);

		return crossOverPoints[Random.Range(0, crossOverPoints.Count)];
	}

	private float Squash(float unsquashedValue) {
		return 2.0f / (1.0f + Mathf.Exp(unsquashedValue)) - 1.0f;
	}
}
