using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class NeuralNet : System.ICloneable {

	[HideInInspector]
	[System.NonSerialized]
	public GameObject ParentFrog;

	[HideInInspector]
	public float fitness = 0.0f;

	[HideInInspector]
	public float snakeDistScore = 0.0f;
	
	public int inputNeurons;
	public int hiddenLayerNeurons;
	public int outputNeurons;
	public bool useRotationSymmetry;
	public bool useReflectionSymmetry;
	
	// Input settings
	public int NumFlyPositions = 2;
	public int NumSnakePositions = 1;
	public bool FeedObstacleInfo = true;
	public bool FeedOwnVelocity = true;
	
	public float defaultInputExponent = 1.0f;
	public float defaultOutputExponent = 1.0f;
	
	public float[][] neuronValues = new float[3][]; // For storing calculated values
	public float[][] weights = new float[2][];
	
	public List<float> weightsAsVector; // For viewing in the inspector
	
	public System.Object Clone() {
		
		NeuralNet clone = new NeuralNet(NumFlyPositions, NumSnakePositions, FeedObstacleInfo, FeedOwnVelocity, hiddenLayerNeurons, useRotationSymmetry, useReflectionSymmetry);

		clone.weights = new float[][]{(float[])(weights[0].Clone()), (float[])(weights[1].Clone())}; // Deep copy!
		clone.defaultInputExponent = defaultInputExponent;
		clone.defaultOutputExponent = defaultOutputExponent;
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
		
		weights[0] = new float[(inputNeurons + 1) * hiddenLayerNeurons]; // +1 for the exponent
		weights[1] = new float[(hiddenLayerNeurons + 1) * outputNeurons]; // +1 for the exponent
		
		// Hidden layer
		for (int i = 0; i < hiddenLayerNeurons; i++) {
			
			weights[0][i * (inputNeurons + 1)] = defaultInputExponent; // Exponent
			
			for (int j = 0; j < inputNeurons; j++) {
				weights[0][i * (inputNeurons + 1) + j + 1] = Random.value - 0.5f;
			}
		}
		
		// Output
		for (int i = 0; i < outputNeurons; i++) {
			
			weights[1][i * (hiddenLayerNeurons + 1)] = defaultOutputExponent; // Exponent
			
			for (int j = 0; j < hiddenLayerNeurons; j++) {
				weights[1][i * (hiddenLayerNeurons + 1) + j + 1] = Random.value - 0.5f;
			}
		}
		
		UpdateDisplayWeights();
	}
	
	public NeuralNet(int NumFlyPositions, int NumSnakePositions, bool FeedObstacleInfo, bool FeedOwnVelocity, int hiddenLayerNeurons, bool useRotationSymmetry, bool useReflectionSymmetry) {

		this.NumFlyPositions = NumFlyPositions;
		this.NumSnakePositions = NumSnakePositions;
		this.FeedObstacleInfo = FeedObstacleInfo;
		this.FeedOwnVelocity = FeedOwnVelocity;
		this.inputNeurons = (NumFlyPositions + NumSnakePositions) * 2 + (FeedObstacleInfo ? 2 : 0) + (FeedOwnVelocity ? 2 : 0);
		this.hiddenLayerNeurons = hiddenLayerNeurons;
		this.outputNeurons = 2;
		this.useRotationSymmetry = useRotationSymmetry;
		this.useReflectionSymmetry = useReflectionSymmetry;
		
		neuronValues[0] = new float[inputNeurons];
		neuronValues[1] = new float[hiddenLayerNeurons];
		neuronValues[2] = new float[outputNeurons];
		
		RandomiseWeights();
	}

	public NeuralNet(NeuralNet existingNet) {

		this.inputNeurons = existingNet.inputNeurons;
		this.hiddenLayerNeurons = existingNet.hiddenLayerNeurons;
		this.outputNeurons = existingNet.outputNeurons;
		this.useRotationSymmetry = existingNet.useRotationSymmetry;
		this.useReflectionSymmetry = existingNet.useReflectionSymmetry;
		
		neuronValues[0] = new float[inputNeurons];
		neuronValues[1] = new float[hiddenLayerNeurons];
		neuronValues[2] = new float[outputNeurons];
		
		RandomiseWeights();
	}

	public float[] CalculateOutputNoSymmetry(float[] inputValues) {
		
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
			
			float exponent = weights[0][i * (inputNeurons + 1)];
			
			for (int j = 0; j < inputNeurons; j++) {
				neuronValues[1][i] += neuronValues[0][j] * weights[0][i * (inputNeurons + 1) + j + 1];
			}
			
			// Squash between -1 and 1
			neuronValues[1][i] = Squash(neuronValues[1][i], exponent);
		}
		
		// Output
		for (int i = 0; i < outputNeurons; i++) {
			
			float exponent = weights[1][i * (hiddenLayerNeurons + 1)];
			
			for (int j = 0; j < hiddenLayerNeurons; j++) {
				neuronValues[2][i] += neuronValues[1][j] * weights[1][i * (hiddenLayerNeurons + 1) + j + 1];
			}
			
			// Squash between -1 and 1
			neuronValues[2][i] = Squash(neuronValues[2][i], exponent);
		}
		
		return neuronValues[2];
	}
	
	public float[] CalculateOutput(float[] inputValues) {

		if (!useRotationSymmetry && !useReflectionSymmetry) {

			return CalculateOutputNoSymmetry(inputValues);

		// Since reflections and rotations by 90 degrees shouldn't make any difference to the frog's behaviour, it's
	    // probably a good idea to enforce this, which is what the following code does. It loops through all 8 symmetries
		// of a square, applying the symmetry to the input, calculating the output, then reversing the symmetry on the
		// output so that we get back to the original co-ordinate system. By summing all 8 outputs, it ensures that the
		// frog is indifferent to symmetry transforms on the input.
		// We assume that the input is in the form of a list of 2d vectors and that the output is a single 2d vector.
		} else {

			Vector2 output = Vector2.zero;

			for (int coordSys = 1; coordSys >= (useReflectionSymmetry ? -1 : 1); coordSys -= 2) {

				for (int quadrant = 0; quadrant <= (useRotationSymmetry ? 3 : 0); quadrant++) {

					float[] rotatedInput = new float[inputValues.Length];

					for (int i = 0; i < inputValues.Length; i += 2) {

						Vector2 vec = new Vector2((float)coordSys * inputValues[i], inputValues[i + 1]);
						vec = MathsHelper.rotateVector(vec, (float)quadrant * 90.0f);
						rotatedInput[i] = vec.x;
						rotatedInput[i + 1] = vec.y;
					}

					float[] rotatedOutput = CalculateOutputNoSymmetry(rotatedInput);
					Vector2 rotatedOutputVec = new Vector2(rotatedOutput[0], rotatedOutput[1]);
					Vector2 restoredOutputVec = MathsHelper.rotateVector(rotatedOutputVec, (float)quadrant * -90.0f);
					restoredOutputVec.x *= (float)coordSys;
					output += restoredOutputVec;
				}
			}

			return new float[] {output.x, output.y};
		}
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
	
	private float Squash(float unsquashedValue, float exponent) {
		return 2.0f / (1.0f + Mathf.Exp(-unsquashedValue * exponent)) - 1.0f;
	}
}
