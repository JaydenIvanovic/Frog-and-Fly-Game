using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// TO DO:
// I'm not sure if we really need constructors - Unity likes you to use Awake() / Start() instead

public class GAFrogController : GAController<GameObject> {

	public enum ParentSelectionMode
	{
		Proportional = 0,
		Exponential = 1
	};

	public ParentSelectionMode parentSelectionMode = ParentSelectionMode.Proportional;

	// These "accentuation" variables make it so that the best performers get a bigger slice of the pie
	// when it comes to parent selection. I didn't get this out of the book but it just seems like a 
	// natural thing to try.
	public float propSelectionAccentuation = 0.5f; // Should be between 0 and 1
	public float expSelectionAccentuation = 0.5f;

	public bool verbose = false;

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

			// If a frog did badly enough then just randomise its weights again
			float resetThreshold = 2.9f;
			int resetCount = 0;
			foreach (GameObject frog in population) {
				if (CalcFitness(frog) < resetThreshold) {
					frog.GetComponent<NeuralNetSteering>().neuralNet.RandomiseWeights();
					frog.GetComponent<PlayerInfo>().Reset();
					resetCount++;
				}
			}

			if (verbose) {
				Debug.Log("Reset " + resetCount + " frogs due to bad performance");
			}

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

		switch (parentSelectionMode) {
		case ParentSelectionMode.Proportional:
			return SelectParentProportional();
		case ParentSelectionMode.Exponential:
			return SelectParentExponential();
		default:
			return SelectParentProportional();
		}
	}
	
	private GameObject SelectParentProportional() {

		float sumFitness = 0.0f;
		float maxFitness = float.MinValue;

		// Find the maximum fitness
		for (int i = 0; i < fitness.Count; i++) {
			if (fitness[i] > maxFitness) {
				maxFitness = fitness[i];
			}
		}

		// Calculate the total population's fitness
		for (int i = 0; i < fitness.Count; i++) {
			sumFitness += Mathf.Max(fitness[i] - maxFitness * propSelectionAccentuation, 0.0f);
		}

		// Just return a random frog if there were no flies caught
		if (sumFitness == 0.0f) {
			return CopyChromosome(population[Random.Range(0, population.Count)]);
		}
		
		// Weight the change of a frog being chosen based on its fitness
		float cumuFitness = 0.0f;
		float threshold = Random.Range(0.0f, sumFitness);
		
		for (int i = 0; i < fitness.Count; i++) {

			cumuFitness += Mathf.Max(fitness[i] - maxFitness * propSelectionAccentuation, 0.0f);

			if (cumuFitness >= threshold) {
				if (verbose) {
					Debug.Log("Selected parent " + i + ", fitness = " + fitness[i]);
				}
				return CopyChromosome(population[i]);
			}
		}
		
		// Should never reach this point
		return null; 
	}

	private GameObject SelectParentExponential() {

		float sumFitness = 0.0f;

		for (int i = 0; i < fitness.Count; i++) {
			sumFitness += Mathf.Exp(fitness[i] * expSelectionAccentuation);
		}

		// Just return a random frog if there were no flies caught
		if (sumFitness == 0.0f) {
			return CopyChromosome(population[Random.Range(0, population.Count)]);
		}

		// Weight the change of a frog being chosen based on its fitness
		float cumuFitness = 0.0f;
		float threshold = Random.Range(0.0f, sumFitness);

		for (int i = 0; i < fitness.Count; i++) {
			cumuFitness += Mathf.Exp(fitness[i] * expSelectionAccentuation);
			if (cumuFitness >= threshold) {
				if (verbose) {
					Debug.Log("Selected parent with fitness = " + fitness[i]);
				}
				return CopyChromosome(population[i]);
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

		GameObject child1 = CopyChromosome(parent1);
		GameObject child2 = CopyChromosome(parent2);

		NeuralNet net1 = child1.GetComponent<NeuralNetSteering>().neuralNet;
		NeuralNet net2 = child2.GetComponent<NeuralNetSteering>().neuralNet;

		int crossOverPoint = net1.GetRandomCrossOverIndex();
		int counter = 0;
		float tempWeight;

		for (int i = 0; i < net1.weights.Length; i++) {

			for (int j = 0; j < net1.weights[i].Length; j++) {

				if (counter >= crossOverPoint) {
					tempWeight = net1.weights[i][j];
					net1.weights[i][j] = net2.weights[i][j];
					net2.weights[i][j] = tempWeight;
				}

				counter++;
			}
		}

		return new GameObject[]{child1, child2};
	}
	
	public override GameObject Mutate(GameObject chromosome) {

		float perturbationAmount = 0.5f;

		GameObject clonedFrog = CopyChromosome(chromosome);
		
		NeuralNet net = clonedFrog.GetComponent<NeuralNetSteering>().neuralNet;

		for (int i = 0; i < net.weights.Length; i++) {
			
			for (int j = 0; j < net.weights[i].Length; j++) {
				
				if (Random.Range(0.0f, 1.0f) <= mutationRate) {
					Debug.Log("Old " + i + ", " + j + " was " + net.weights[i][j]);
					net.weights[i][j] += Random.Range(-1.0f, 1.0f) * perturbationAmount;
					Debug.Log("Mutated " + i + ", " + j + " to " + net.weights[i][j]);
				}
			}
		}
		
		return clonedFrog;
	}

	public override GameObject CopyChromosome(GameObject chromosome) {

		GameObject clonedFrog = (GameObject)Instantiate(chromosome);
		clonedFrog.GetComponent<NeuralNetSteering>().neuralNet = (NeuralNet)(chromosome.GetComponent<NeuralNetSteering>().neuralNet.Clone());
		clonedFrog.GetComponent<PlayerInfo>().Reset();
		return clonedFrog;
	}
}
