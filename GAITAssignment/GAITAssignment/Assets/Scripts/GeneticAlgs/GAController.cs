﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class GAController<T> : GeneticAlgorithm_I<T>
{
	private List<T> population;
	private List<float> fitness;
	private float mutationRate, crossoverRate;
	private int populationSize;


	// Defaults for mutation and crossover rates are as recommended in 
	// the "AI Techniques for Game Programming" book.
	public GAController(int popSize, float mutationRate = 0.001f, float crossoverRate = 0.7f)
	{
		this.mutationRate = mutationRate;
		this.crossoverRate = crossoverRate;
		this.fitness = new List<float>(popSize);
		this.populationSize = popSize;
	}


	// Getters and Setters
	public List<T> Population 
	{
		get{return population;}
		set{this.population = value;}
	}


	public void SetIndividual(int index, T value)
	{
		population.Insert(index, value);
	}


	public List<float> Fitness 
	{
		get{return fitness;}
		set{this.fitness = value;}
	}


	public int PopulationSize
	{
		get{return populationSize;}
	}


	public float MutationRate 
	{
		get{return mutationRate;}
	}


	// Methods to be overidden by the subclass
	public abstract void InitPopulation();
	
	public abstract T SelectParent();
	
	public abstract float CalcFitness(T chromosome);
	
	public abstract T[] CrossOver(T parent1, T parent2);
	
	public abstract void Mutate(T chromosome);


	// The core Genetic Algorithm
	// Run one generation of evolution.
	public void RunEpoch()
	{
		// Update the fitness values
		for (int i = 0; i < populationSize; ++i) {
			fitness.Insert(i, CalcFitness(population[i]));
		}
		
		// For storing the children as they are created.
		List<T> children = new List<T>(populationSize);
		// Keep track of the number of individuals in the new population.
		int numChildren = 0;
		
		while (numChildren < populationSize) {		
			// Selection
			T parent1 = SelectParent();
			T parent2 = SelectParent();
			
			T[] child;
			// CrossOver
			if (Random.value < crossoverRate) {
				child = CrossOver(parent1, parent2);
			} else {
				child = new T[2];
				child[0] = parent1;
				child[1] = parent2;
			}
			
			// Mutation step.
			Mutate(child[0]);
			Mutate(child[1]);
			
			children.Add(child[0]);
			children.Add(child[1]);
			
			numChildren += 2;
		}
		
		// Replace the population with the newly calculated one. 
		population = children;
	}
}
