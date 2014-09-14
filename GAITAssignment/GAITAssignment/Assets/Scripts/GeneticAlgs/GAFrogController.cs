using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// TO DO:
// I'm not sure if we really need constructors - Unity likes you to use Awake() / Start() instead

public class GAFrogController : GAController<GameObject> {

	public float updateFrequency = 2.0f;
	private float updateTimer = 0.0f;
	
	// Defaults for mutation and crossover rates are as recommended in 
	// the "AI Techniques for Game Programming" book.
	public GAFrogController(float mutationRate = 0.001f, float crossoverRate = 0.7f) : base(0, mutationRate, crossoverRate) {}

	public GAFrogController() : base(0, 0.001f, 0.7f) {}
	
	public override void InitPopulation()
	{
		// Not needed - handled by Awake()
	}

	public void Awake() {

		population = new List<GameObject>();
		GameObject[] frogs = GameObject.FindGameObjectsWithTag("Player");
		population.AddRange(frogs);

		populationSize = population.Count;
	}

	public void Update() {

		updateTimer += Time.deltaTime;
		
		if (updateTimer > updateFrequency) {

			// Get the old positions and fly managers so we can restore them for the new population
			Queue<Vector3> oldFrogPositions = new Queue<Vector3>();
			Queue<SpawnDumbFlies> flySpawners = new Queue<SpawnDumbFlies>();

			GameObject[] frogs = GameObject.FindGameObjectsWithTag("Player");
			foreach (GameObject frog in frogs) {
				oldFrogPositions.Enqueue(frog.GetComponent<NeuralNetSteering>().flyManager.transform.position);
				flySpawners.Enqueue(frog.GetComponent<NeuralNetSteering>().flyManager);
			}

			// Reset the flies so we don't end up with a bunch of hard-to-reach ones
			GameObject[] flies = GameObject.FindGameObjectsWithTag("Fly");
			foreach (GameObject fly in flies) {
				Destroy(fly);
			}

			fitness = new List<float>(populationSize);
			RunEpoch();

			// Remove old population
			frogs = GameObject.FindGameObjectsWithTag("Player");
			foreach (GameObject frog in frogs) {
				if (!population.Contains(frog)) {
					Destroy(frog);
				}
			}

			frogs = GameObject.FindGameObjectsWithTag("Player");
			foreach (GameObject frog in frogs) {
				if (population.Contains(frog)) {
					frog.transform.position = oldFrogPositions.Dequeue();
					frog.GetComponent<NeuralNetSteering>().flyManager = flySpawners.Dequeue();
					frog.GetComponent<NeuralNetSteering>().flyManager.frog = frog;
					frog.GetComponent<NeuralNetSteering>().selectedFly = null;
				}
			}

			updateTimer = 0.0f;
		}
	}
	
	public override GameObject SelectParent() {

		GameObject result = null;
		float sumFitness = 0.0f;

		// Temp
		//float maxFitness = float.MinValue;
		//int bestIndex = -1;

		for (int i = 0; i < fitness.Count; i++) {

			// Temp
			//if (fitness[i] > maxFitness) {
				//bestIndex = i;
				//maxFitness = fitness[i];
			//}

			sumFitness += fitness[i];
		}

		//return population[bestIndex];

		// Just return a random frog if there were no flies caught
		if (sumFitness == 0.0f) {
			result = population[Random.Range(0, population.Count)];
			//Debug.Log("Parent fitness was " + CalcFitness(result));
			return result;
		}

		// Weight the change of a frog being chosen based on its fitness
		float cumuFitness = 0.0f;
		float threshold = Random.Range(0.0f, sumFitness);

		for (int i = 0; i < fitness.Count; i++) {
			cumuFitness += fitness[i];
			if (cumuFitness >= threshold) {
				result = population[i];
				return result;
			}
		}

		// Should never reach this point
		return null; 
	}
	
	public override float CalcFitness(GameObject chromosome) {

		// Prevent divide by zero when selecting a parent
		float minFitness = 0.0001f;

		PlayerInfo frogInfo = chromosome.GetComponent<PlayerInfo>();
		return frogInfo.score + minFitness;
	}

	// Now follows the advice from the bottom of page 258 in the "AI Techniques for Game Programming" book.
	public override GameObject[] CrossOver(GameObject parent1, GameObject parent2) {

		GameObject child1 = (GameObject)Instantiate(parent1);
		GameObject child2 = (GameObject)Instantiate(parent2);

		NeuralNet net1 = parent1.GetComponent<NeuralNetSteering>().neuralNet;
		NeuralNet net2 = parent2.GetComponent<NeuralNetSteering>().neuralNet;

		float[][] newWeights1 = (float[][])(net1.weights.Clone());
		float[][] newWeights2 = (float[][])(net2.weights.Clone());

		int crossOverPoint = net1.GetRandomCrossOverIndex();
		int counter = 0;
		float tempWeight;

		for (int i = 0; i < newWeights1.Length; i++) {

			for (int j = 0; j < newWeights1[i].Length; j++) {

				if (counter >= crossOverPoint) {
					tempWeight = newWeights1[i][j];
					newWeights1[i][j] = newWeights2[i][j];
					newWeights2[i][j] = tempWeight;
				}

				counter++;
			}
		}

		child1.GetComponent<NeuralNetSteering>().neuralNet.weights = newWeights1;
		child2.GetComponent<NeuralNetSteering>().neuralNet.weights = newWeights2;

		child1.GetComponent<PlayerInfo>().score = 0;
		child2.GetComponent<PlayerInfo>().score = 0;

		return new GameObject[]{child1, child2};
	}
	
	public override GameObject Mutate(GameObject chromosome) {

		float perturbationAmount = 0.05f;

		GameObject clonedFrog = (GameObject)Instantiate(chromosome);
		
		NeuralNet net = chromosome.GetComponent<NeuralNetSteering>().neuralNet;
		
		float[][] newWeights = (float[][])(net.weights.Clone());
		
		for (int i = 0; i < newWeights.Length; i++) {
			
			for (int j = 0; j < newWeights[i].Length; j++) {
				
				if (Random.Range(0.0f, 1.0f) >= mutationRate) {
					newWeights[i][j] += Random.Range(-1.0f, 1.0f) * perturbationAmount;
				}
			}
		}
		
		clonedFrog.GetComponent<NeuralNetSteering>().neuralNet.weights = newWeights;
		clonedFrog.GetComponent<PlayerInfo>().score = 0;
		
		return clonedFrog;
	}

	public override GameObject Clone(GameObject chromosome) {

		GameObject clonedFrog = (GameObject)Instantiate(chromosome);
		clonedFrog.GetComponent<NeuralNetSteering>().neuralNet.weights = (float[][])(chromosome.GetComponent<NeuralNetSteering>().neuralNet.weights.Clone());
		clonedFrog.GetComponent<PlayerInfo>().score = 0;
		return clonedFrog;
	}
}
