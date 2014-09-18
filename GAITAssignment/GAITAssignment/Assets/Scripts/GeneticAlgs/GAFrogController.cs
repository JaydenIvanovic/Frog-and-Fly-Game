using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// TO DO:
// I'm not sure if we really need constructors - Unity likes you to use Awake() / Start() instead

public class GAFrogController : GAController<NeuralNet> {

	public enum ParentSelectionMode
	{
		Proportional = 0,
		Exponential = 1,
		Tournament = 2,
		RankRoulette = 3
	};

	[System.Serializable]
	public class GAParameters {
		public int NumberOfBatches = 1;
		public float batchTime = 10.0f;
		public float timeScale = 3.0f;
		public bool spawnFlies = true;
		public bool flyMovement = false;
		public bool snakesActive = false;
		public bool frogObstacleAvoidance = true;
		public float discardThreshold = 2.9f;
		public ParentSelectionMode parentSelectionMode = ParentSelectionMode.Proportional;

		// These "accentuation" variables make it so that the best performers get a bigger slice of the pie
		// when it comes to parent selection. I didn't get this out of the book but it just seems like a 
		// natural thing to try.
		public float propSelectionAccentuation = 0.5f; // Should be between 0 and 1
		public float expSelectionAccentuation = 0.5f;

		public bool verbose = false;

		public NeuralNetSettings neuralNetSettings;
	} 

	[System.Serializable]
	public class NeuralNetSettings {
		public int HiddenNeurons = 4;
		public bool useRotationSymmetry = true;
		public bool useReflectionSymmetry = true;
	}

	public int FrogsOnScreen = 8;
	public int CurrentEpoch = 0;
	public int CurrentBatch = 0;
	public bool letFrogShootBubble = false;
	public int parameterIndexToUse = 0;
	public GAParameters[] parameters;

	private GAParameters currentParams;
	private float updateTimer = 0.0f;
	private int currentPopIndex = 0;
	
	// Defaults for mutation and crossover rates are as recommended in 
	// the "AI Techniques for Game Programming" book.
	public GAFrogController(float mutationRate = 0.001f, float crossoverRate = 0.7f) : base(0, mutationRate, crossoverRate) {}

	public GAFrogController() : base(0, 0.001f, 0.7f) {}
	
	public override void InitPopulation()
	{
		// Not needed - handled by Awake()
	}

	public void Awake() {}

	// Michael: It's CRITICAL that this stuff goes in Start(), not Awake() since the frogs may not
	// all be initialised before this component. It was causing me nightmares!
	public void Start() {

		currentParams = parameters[parameterIndexToUse];

		Time.timeScale = currentParams.timeScale;

		population = new List<NeuralNet>();
		populationSize = currentParams.NumberOfBatches * FrogsOnScreen;

		for (int i = 0; i < populationSize; i++) {
			population.Add(new NeuralNet(6, currentParams.neuralNetSettings.HiddenNeurons, 2, currentParams.neuralNetSettings.useRotationSymmetry, currentParams.neuralNetSettings.useReflectionSymmetry));
		}

		GameObject[] frogs = GameObject.FindGameObjectsWithTag("Player");

		foreach (GameObject frog in frogs) {
			frog.GetComponent<NeuralNetSteering>().neuralNet = population[currentPopIndex];
			frog.GetComponent<NeuralNetSteering>().neuralNet.ParentFrog = frog;
			frog.GetComponentInChildren<Mouth>().BubbleEnabled = letFrogShootBubble;
			frog.GetComponent<SteeringController>().avoidObstacles = currentParams.frogObstacleAvoidance;
			IncrementPopulationIndex();
		}

		UpdatePens();
	}

	public void IncrementPopulationIndex() {
		currentPopIndex = (currentPopIndex + 1) % populationSize;
	}

	private void UpdatePens() {

		GameObject[] penManagers = GameObject.FindGameObjectsWithTag("PenManager");
		
		for (int i = 0; i < penManagers.Length; i++) {
			
			ManagePen manager = penManagers[i].GetComponent<ManagePen>();

			manager.spawnFlies = currentParams.spawnFlies;
			manager.snake.SetActive(currentParams.snakesActive);
		}

		GameObject[] flies = GameObject.FindGameObjectsWithTag("Fly");

		foreach (GameObject fly in flies) {
			if (!currentParams.spawnFlies) {
				Destroy(fly);
			} else {
				fly.GetComponent<Movement>().speed = (currentParams.flyMovement ? 3.0f : 0.0f);
			}
		} 
	}
	
	public void Update() {

		currentParams = parameters[parameterIndexToUse];

		UpdatePens();

		updateTimer += Time.deltaTime;

		GameObject[] penManagers = GameObject.FindGameObjectsWithTag("PenManager");
		GameObject[] frogs = GameObject.FindGameObjectsWithTag("Player");

		for (int i = 0; i < penManagers.Length; i++) {
			NeuralNet net = penManagers[i].GetComponent<ManagePen>().frog.GetComponent<NeuralNetSteering>().neuralNet;
			net.snakeDistScore += Time.deltaTime * Mathf.Min(1.75f, ((Vector2)(penManagers[i].GetComponent<ManagePen>().snake.transform.position) - (Vector2)(penManagers[i].GetComponent<ManagePen>().frog.transform.position)).magnitude);
		}
		
		if (updateTimer > currentParams.batchTime) {

			CurrentBatch++;

			for (int i = 0; i < penManagers.Length; i++) {
				penManagers[i].GetComponent<ManagePen>().currentSpawnPosition = 0;
			}

			// Reset the flies each time the frogs are reset so that we don't just end up
			// with the hard-to-reach flies
			GameObject[] flies = GameObject.FindGameObjectsWithTag("Fly");
			foreach (GameObject fly in flies) {
				Destroy(fly);
			}

			// Loop over the last neural nets used and store their fitnesses on the nets themselves.
			// This is necessary because the frogs are about to have their scores reset.

			int resetCount = 0; // If a neural net performed badly enough then just randomise its weights again

			for (int i = 1; i <= frogs.Length; i++) {

				int popIndex = (currentPopIndex - i + populationSize) % populationSize; // We have to count backwards from currentPopIndex using modular arithmetic to find the neural nets just used
				NeuralNet net = population[popIndex];
				PlayerInfo frogInfo = net.ParentFrog.GetComponent<PlayerInfo>();

				net.fitness = 1.0f * (float)(frogInfo.score) + (currentParams.batchTime - 7.5f * (float)(frogInfo.DamageTaken)) + 0.25f * net.snakeDistScore;
				net.snakeDistScore = 0.0f;

				if (net.fitness <= currentParams.discardThreshold) {
					net.RandomiseWeights();
					resetCount++;
				}
			}

			if (currentParams.verbose) {
				Debug.Log("Reset " + resetCount + " neural nets due to bad performance");
			}

			// If we've completed a batch then run the genetic algorithm thingy
			if (currentPopIndex == 0) {
				RunEpoch();
				CurrentEpoch++;
				CurrentBatch = 0;
				penManagers[0].GetComponent<ManagePen>().ResetSpawnPositions(100); // TO DO: Remove magic number
			}

			for (int i = 0; i < penManagers.Length; i++) {

				// Move the frog back to its start position
				frogs[i].transform.position = penManagers[i].transform.position;

				frogs[i].GetComponent<NeuralNetSteering>().neuralNet = population[currentPopIndex];
				frogs[i].GetComponent<NeuralNetSteering>().neuralNet.UpdateDisplayWeights();
				frogs[i].GetComponent<NeuralNetSteering>().manager = penManagers[i].GetComponent<ManagePen>();
				population[currentPopIndex].ParentFrog = frogs[i];

				// Ensure that the snakes are targeting the right frogs
				GameObject snake = penManagers[i].GetComponent<ManagePen>().snake;
				snake.GetComponent<GameObjectTargeter>().Target = frogs[i];
				snake.GetComponent<HuntTargeter>().Target = frogs[i];
				snake.GetComponent<PredatorStateMachine>().Player = frogs[i];
				snake.transform.position = new Vector3(snake.GetComponent<PredatorStateMachine>().Home.transform.position.x,
				                                       snake.GetComponent<PredatorStateMachine>().Home.transform.position.y,
				                                       snake.transform.position.z);

				// Find a new fly to select on the next update
				frogs[i].GetComponent<NeuralNetSteering>().selectedFly = null;

				// Reset the frog's score
				frogs[i].GetComponent<PlayerInfo>().Reset();

				IncrementPopulationIndex();
			}

			updateTimer = 0.0f;
		}
	}

	public override NeuralNet SelectParent() {

		switch (currentParams.parentSelectionMode) {
		case ParentSelectionMode.Proportional:
			Debug.Log("Proportional Selected.");
			return SelectParentProportional();
		case ParentSelectionMode.Exponential:
			Debug.Log("Exponential Selected.");
			return SelectParentExponential();
		case ParentSelectionMode.Tournament:
			Debug.Log("Tournament Selected.");
			return SelectParentTournament(2); // Binary tournament
		case ParentSelectionMode.RankRoulette:
			Debug.Log("RankRoulette Selected.");
			return SelectParentRankRoulette(1.8f); // 2 >= sp >= 1
		default:
			Debug.Log("Proportional Selected.");
			return SelectParentProportional();
		}
	}
	
	private NeuralNet SelectParentProportional() {

		float sumFitness = 0.0f;
		float maxFitness = float.MinValue;
		float minFitness = float.MaxValue;

		// Find the maximum and minimum fitnesses
		for (int i = 0; i < fitness.Count; i++) {
			if (fitness[i] > maxFitness) {
				maxFitness = fitness[i];
			}
			if (fitness[i] < minFitness) {
				minFitness = fitness[i];
			}
		}

		// Ensure that all fitnesses are positive
		if (minFitness < 0.0f) {

			maxFitness -= minFitness;

			for (int i = 0; i < fitness.Count; i++) {
				fitness[i] -= minFitness;
			}
		}

		// Calculate the total population's fitness
		for (int i = 0; i < fitness.Count; i++) {
			sumFitness += Mathf.Max(fitness[i] - maxFitness * currentParams.propSelectionAccentuation, 0.0f);
		}

		// Just return a random frog if there were no flies caught
		if (sumFitness == 0.0f) {
			return CopyChromosome(population[Random.Range(0, population.Count)]);
		}
		
		// Weight the change of a frog being chosen based on its fitness
		float cumuFitness = 0.0f;
		float threshold = Random.Range(0.0f, sumFitness);
		
		for (int i = 0; i < fitness.Count; i++) {

			cumuFitness += Mathf.Max(fitness[i] - maxFitness * currentParams.propSelectionAccentuation, 0.0f);

			if (cumuFitness >= threshold) {
				if (currentParams.verbose) {
					Debug.Log("Selected parent " + i + ", fitness = " + fitness[i]);
				}
				return CopyChromosome(population[i]);
			}
		}
		
		// Should never reach this point
		return null; 
	}

	private NeuralNet SelectParentExponential() {

		float sumFitness = 0.0f;
		float minFitness = float.MaxValue;
		
		// Find the minimum fitness
		for (int i = 0; i < fitness.Count; i++) {
			if (fitness[i] < minFitness) {
				minFitness = fitness[i];
			}
		}

		// Ensure that all fitnesses are positive
		if (minFitness < 0.0f) {
			for (int i = 0; i < fitness.Count; i++) {
				fitness[i] -= minFitness;
			}
		}

		for (int i = 0; i < fitness.Count; i++) {
			sumFitness += Mathf.Exp(fitness[i] * currentParams.expSelectionAccentuation);
		}

		// Just return a random frog if there were no flies caught
		if (sumFitness == 0.0f) {
			return CopyChromosome(population[Random.Range(0, population.Count)]);
		}

		// Weight the change of a frog being chosen based on its fitness
		float cumuFitness = 0.0f;
		float threshold = Random.Range(0.0f, sumFitness);

		for (int i = 0; i < fitness.Count; i++) {
			cumuFitness += Mathf.Exp(fitness[i] * currentParams.expSelectionAccentuation);
			if (cumuFitness >= threshold) {
				if (currentParams.verbose) {
					Debug.Log("Selected parent with fitness = " + fitness[i]);
				}
				return CopyChromosome(population[i]);
			}
		}

		// Should never reach this point
		return null; 
	}
	
	// Jayden: If I understood this selection method it's quite straightforward.
	// Pick x individuals from population N and the one with the best fitness
	// goes into the new population. Rinse and repeat. 
	private NeuralNet SelectParentTournament(int tournamentSize) {

		// We can just store the index of the competitors.
		int[] tournamentPopulation = new int[tournamentSize];

		// Create the tournament pool of competitors.
		for (int i = 0; i < tournamentSize; ++i) {
			int randIndex = (int)(Random.value * tournamentSize);
			tournamentPopulation[i] = randIndex;
		}

		int bestIndex = tournamentPopulation[0];
		// Find the competitor with the best fitness, they win!
		foreach (int i in tournamentPopulation) {
			if (fitness[i] > fitness[bestIndex]) {
				bestIndex = i;
			}
		}
		
		return CopyChromosome(population[bestIndex]);
	}

	// Jayden: And here is the rank-based roulette wheel method.
	// This one is supposed to find a higher quality solution
	// but takes longer to converge.
	private NeuralNet SelectParentRankRoulette(float selectivePressure) {
		
		// We want to store a sorted list of indexes based on rank.
		int[] sortedPop = new int[populationSize];
		// Scaled rank values for the sorted population.
		float[] scaledRank = new float[populationSize];
		//int bestIndex = 0;

		// O(n^2) sort, if it's too slow I'll change it.
		List<float> copyFitness = new List<float>(fitness);

		for (int i = 0; i < populationSize; i++) {
			sortedPop[i] = i;
		}

		float tempValue;
		int tempIndex;

		// Insertion sort
		// Rank the LOWEST fitness first because it should get the worst scaled rank
		for (int i = 0; i < (populationSize - 1); i++) {

			for (int j = i + 1; j < populationSize; j++) {

				if (copyFitness[j] < copyFitness[i]) {

					// Swap values
					tempValue = copyFitness[i];
					copyFitness[i] = copyFitness[j];
					copyFitness[j] = tempValue;

					// Swap indices
					tempIndex = sortedPop[i];
					sortedPop[i] = sortedPop[j];
					sortedPop[j] = tempIndex;
				}
			}
		}

		/*
		for (int p = 0; p < populationSize; ++p) {
			bestIndex = 0;
			for (int i = 1; i < copyFitness.Count; ++i) {
				if (copyFitness[bestIndex] < copyFitness[i]) {
					bestIndex = i;
				}
			}
			sortedPop[p] = bestIndex;
			Debug.Log(p + ", " + fitness[sortedPop[p]]);
			copyFitness.RemoveAt(bestIndex);
		}
		*/

		// Scale the rank according to the selective pressure parameter (2 >= SP >= 1).
		for (int i = 0; i < populationSize; ++i) {
			// From the paper: Genetic Algorithm Performance with Different Selection Strategies in Solving TSP
			// Link: http://www.iaeng.org/publication/WCE2011/WCE2011_pp1134-1139.pdf
			scaledRank[i] = 2 - selectivePressure + ( 2 * (selectivePressure - 1) * ( (i - 1) / (populationSize - 1f) ) );
			//Debug.Log(i + ", " + scaledRank[i] + " fitness: " + fitness[sortedPop[i]]);
		}

		// Get the sum of the ranks i.e. sumFitness.
		float sumFitness = 0f;
		for (int i = 0; i <populationSize; ++i)
			sumFitness += scaledRank[i];

		// Now use a standard roulette wheel selection method using the scaled ranks.
		float cumuFitness = 0f;
		float threshold = Random.Range(0f, sumFitness);

		for (int i = 0; i < populationSize; ++i) {
			cumuFitness += scaledRank[i];
			if(cumuFitness >= threshold) {
				//Debug.Log("Rank selected was " + sortedPop[i]);
				return CopyChromosome(population[sortedPop[i]]);
			}
		}

		return null;
	}

	public override float CalcFitness(NeuralNet chromosome) {

		return chromosome.fitness;
	}

	// Now follows the advice from the bottom of page 258 in the "AI Techniques for Game Programming" book.
	public override NeuralNet[] CrossOver(NeuralNet parent1, NeuralNet parent2) {

		NeuralNet child1 = (NeuralNet)(parent1.Clone());
		NeuralNet child2 = (NeuralNet)(parent2.Clone());

		int crossOverPoint = child1.GetRandomCrossOverIndex();
		int counter = 0;
		float tempWeight;

		for (int i = 0; i < child1.weights.Length; i++) {

			for (int j = 0; j < child1.weights[i].Length; j++) {

				if (counter >= crossOverPoint) {
					tempWeight = child1.weights[i][j];
					child1.weights[i][j] = child2.weights[i][j];
					child2.weights[i][j] = tempWeight;
				}

				counter++;
			}
		}

		return new NeuralNet[]{child1, child2};
	}
	
	public override void Mutate(NeuralNet chromosome) {

		float perturbationAmount = 0.2f;

		for (int i = 0; i < chromosome.weights.Length; i++) {
			
			for (int j = 0; j < chromosome.weights[i].Length; j++) {
				
				if (Random.Range(0.0f, 1.0f) <= mutationRate) {

					chromosome.weights[i][j] += Random.Range(-1.0f, 1.0f) * perturbationAmount;

					if (currentParams.verbose) {
						Debug.Log("Mutated " + i + ", " + j + " to " + chromosome.weights[i][j]);
					}
				}
			}
		}
	}

	public override NeuralNet CopyChromosome(NeuralNet chromosome) {
		return (NeuralNet)(chromosome.Clone());
	}
}
