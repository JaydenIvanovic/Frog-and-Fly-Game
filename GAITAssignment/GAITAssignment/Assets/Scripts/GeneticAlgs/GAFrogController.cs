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
				}
			}

			updateTimer = 0.0f;
		}
	}
	
	public override GameObject SelectParent() {

		float sumFitness = 0.0f;

		for (int i = 0; i < fitness.Count; i++) {
			sumFitness += fitness[i];
		}

		// Just return a random frog if there were no flies caught
		if (sumFitness == 0.0f) {
			return population[Random.Range(0, population.Count)];
		}

		// Weight the change of a frog being chosen based on its fitness
		float cumuFitness = 0.0f;
		float threshold = Random.Range(0.0f, sumFitness);

		for (int i = 0; i < fitness.Count; i++) {
			cumuFitness += fitness[i];
			if (cumuFitness >= threshold) {
				return population[i];
			}
		}

		// Should never reach this point
		return null; 
	}
	
	public override float CalcFitness(GameObject chromosome) {

		// Ensure that all frogs have a chance of being selected
		float minFitness = 1.0f;

		PlayerInfo frogInfo = chromosome.GetComponent<PlayerInfo>();
		return frogInfo.score + minFitness;
	}
	
	public override GameObject[] CrossOver(GameObject parent1, GameObject parent2) {

		GameObject child1 = (GameObject)Instantiate(parent1);
		GameObject child2 = (GameObject)Instantiate(parent2);

		float[][] newWeights1 = parent1.GetComponent<NeuralNetSteering>().neuralNet.weights;
		float[][] newWeights2 = parent2.GetComponent<NeuralNetSteering>().neuralNet.weights;
		float tempWeight;

		// Randomly swap weights
		float crossOverChance = 0.5f;

		for (int i = 0; i < newWeights1.Length; i++) {
			for (int j = 0; j < newWeights1[i].Length; j++) {
				if (Random.Range(0.0f, 1.0f) < crossOverChance) {
					tempWeight = newWeights1[i][j];
					newWeights1[i][j] = newWeights2[i][j];
					newWeights2[i][j] = tempWeight;
				}
			}
		}

		child1.GetComponent<NeuralNetSteering>().neuralNet.weights = newWeights1;
		child2.GetComponent<NeuralNetSteering>().neuralNet.weights = newWeights2;

		return new GameObject[]{child1, child2};
	}

	// TO DO: Fix this so it doesn't just clone
	public override GameObject Mutate(GameObject chromosome) {

		GameObject clonedFrog = (GameObject)Instantiate(chromosome);
		clonedFrog.GetComponent<NeuralNetSteering>().neuralNet.weights = chromosome.GetComponent<NeuralNetSteering>().neuralNet.weights;
		return clonedFrog;
	}

	public override GameObject Clone(GameObject chromosome) {

		GameObject clonedFrog = (GameObject)Instantiate(chromosome);
		clonedFrog.GetComponent<NeuralNetSteering>().neuralNet.weights = chromosome.GetComponent<NeuralNetSteering>().neuralNet.weights;
		return clonedFrog;
	}
}
