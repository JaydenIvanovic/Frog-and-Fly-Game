using UnityEngine;
using System.Collections;

public interface GeneticAlgorithm_I<T>
{
	void InitPopulation();
	T SelectParent();
	float CalcFitness(T chromosome);
	T[] CrossOver(T parent1, T parent2);
	T Mutate(T chromosome);
	void RunEpoch();
}
