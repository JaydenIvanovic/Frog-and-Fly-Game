using UnityEngine;
using System.Collections;
using System.Collections.Generic;


// Tightly coupled to this class.
public struct Trees
{
	public List<Vector2> positions;

	public Trees Clone()
	{
		Trees t = new Trees();
		t.positions = new List<Vector2>();

		foreach (Vector2 position in positions) {
			t.positions.Add(new Vector2(position.x, position.y));
		}

		return t;
	}
}


public class GenerateFoodTrees : GAController<Trees>
{
	public int vectorMutate;
	public int runs;
	public int numTrees;
	public float minxBoundary, maxxBoundary;
	public float minyBoundary, maxyBoundary;
	public GameObject appleTreePrefab, flowerTreePrefab;
	public GameObject home;
	public float requiredDistanceFromHome, distanceFromNeighbours;
	private Trees fitTrees;

	public GenerateFoodTrees() : base(50, 0.001f, 0.7f) {}

	void Awake()
	{
		// Run the GA and get a layout of tree positions for us to use.
		InitPopulation();

		for (int i = 0; i < runs; ++i){
			RunEpoch();
		}

		fitTrees = ReturnFittest();

		// Instantiate the trees in the positions calculated by the GA.
		for(int i = 0; i < fitTrees.positions.Count; ++i) {
			if (Random.value < 0.5) {
				GameObject tree = (GameObject)Instantiate(appleTreePrefab, fitTrees.positions[i], Quaternion.identity);
				tree.transform.parent = transform;
			} else {
				GameObject tree = (GameObject)Instantiate(flowerTreePrefab, fitTrees.positions[i], Quaternion.identity);
				tree.transform.parent = transform;
			}
		}
	}


	// Randomly initalise the population by putting trees anywhere within 
	// the specified boundaries.
	public override void InitPopulation()
	{
		population = new List<Trees>();

		for (int i = 0; i < populationSize; ++i) {
			
			Trees t = new Trees();
			t.positions = new List<Vector2>();
			population.Insert(i, t);
			
			for (int j = 0; j < numTrees; ++j) {
				population[i].positions.Add(new Vector2(Random.Range(minxBoundary, maxxBoundary), Random.Range(minyBoundary, maxyBoundary))); 
			}

		}
	}
	

	// TO DO: Move the tournament selection to the GAController abstract interface 
	// so it can be shared between this and the frog.
	public override Trees SelectParent()
	{
		int tournamentSize = populationSize / 2;
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
	

	// Naive fitness calculation.
	public override float CalcFitness(Trees chromosome)
	{
		float sumDistFromGoal = 0f;
		float sumDistFromOthers = 0f;

		// We want to loop through each tree position and
		// compare its distance to every other tree position and goal tree.
		for (int i = 0; i < chromosome.positions.Count; ++i) {
			
			for (int j = 0; j < chromosome.positions.Count; ++j) {
				
				// Skip this iteration as we don't want to compare the tree
				// position to itself.
				if (i == j)
					continue;

				// If these two tree are really close, instead halve the fitness
				if (Vector2.Distance(chromosome.positions[j], chromosome.positions[i]) < 2)
					sumDistFromOthers *= 0.5f;
				else
					sumDistFromOthers += Vector2.Distance(chromosome.positions[j], chromosome.positions[i]);
			}

			// If outside boundary return 0 as this is a solution we never want.
			if (chromosome.positions[i].x > maxxBoundary || chromosome.positions[i].x < minxBoundary ||
				chromosome.positions[i].y > maxyBoundary || chromosome.positions[i].y < minyBoundary) {
				return 0;
			} else {
				sumDistFromGoal += Vector2.Distance(Vector2.zero, chromosome.positions[i]);
			}
		}

		return sumDistFromGoal + sumDistFromOthers;
	}
	

	// Do a simple crossover where child1 and child2 keep the
	// first half of their respective parents set of positions, 
	// but the second half is inherited from the other parent.
	// i.e. child2 = [half parent 2 | half parent 1]
	public override Trees[] CrossOver(Trees parent1, Trees parent2)
	{
		int midPoint = parent1.positions.Count / 2;
		
		Trees[] children = new Trees[2];
		children[0].positions = new List<Vector2>();
		children[1].positions = new List<Vector2>();

		for(int i = 0; i < midPoint; ++i) {
			children[0].positions.Add(parent1.positions[i]);
			children[1].positions.Add(parent2.positions[i]);
		}

		for (int i = midPoint; i < parent1.positions.Count; ++i) {
			children[0].positions.Add(parent2.positions[i]);
			children[1].positions.Add(parent1.positions[i]);
		}

		return children;
	}
	

	public override void Mutate(Trees chromosome)
	{
		// Loop through each position
		for (int i = 0; i < chromosome.positions.Count; ++i) {
			
			float x = chromosome.positions[i].x;
			float y = chromosome.positions[i].y;

			// Mutate x pos
			if(Random.value < mutationRate) {
				x = Random.Range(x - vectorMutate, x + vectorMutate);
			}

			// Mutate y pos
			if(Random.value < mutationRate) {
				y = Random.Range(y - vectorMutate, y + vectorMutate);
			}

			chromosome.positions[i] = new Vector2(x, y);
		}
	}


	public override Trees CopyChromosome(Trees chromosome)
	{
		return chromosome.Clone();
	}
}
