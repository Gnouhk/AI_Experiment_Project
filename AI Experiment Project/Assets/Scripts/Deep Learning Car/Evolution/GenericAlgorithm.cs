using System;
using System.Collections.Generic;

public class GenericAlgorithm
{
    #region Members

    #region Default Parameters

    // Default min, max value of initial population parameters
    public const float DefInitParamMin = -1.0f;
    public const float DefInitParamMax = 1.0f;

    // Default probability of a parameter being swapped during crossover.
    public const float DefCrossSwapProp = 0.6f;

    // Default probability of a parameter being mutated.
    public const float DefMutationProp = 0.3f;

    // Default amount by which parameters may be mutated.
    public const float DefMutationAmount = 2.0f;

    // Default percent of genotypes in a new population that are mutated.
    public const float DefMutationPerc = 1.0f;

    #endregion

    #region Operator Delegate

    // Methods used to initialise the initial population.
    public delegate void InitialisationOperator(IEnumerable<Genotype> initialPopulation);

    // Methods used to evaluate (or start the evluation process of) the current population.
    public delegate void EvaluationOperator (IEnumerable<Genotype> currentPopulation);

    // Methods used to calculate the fitness value of each genotype of the current population.
    public delegate void FitnessCalculation (IEnumerable<Genotype> currentPopulation);

    // Methods used to select genotypes of the current population and create the intermediate population.
    public delegate List<Genotype> SelectionOperator(List<Genotype> currentPopulation);

    // Methods used to recombine the intermediate population to generate a new popualation
    public delegate List<Genotype> RecombinationOperator(List<Genotype> intermediatePopulation, uint newPopulationSize);

    // Methods used to mutate new population
    public delegate void MutationOperator(List<Genotype> newPopulation);

    // Methods used to check whether any termination criterion has been met
    public delegate bool CheckTerminationCriterion(IEnumerable<Genotype> currentPopulation);

    #endregion

    #region Operator Methods

    // Method used to initalise the initial population.
    public InitialisationOperator InitialisePopulation = DafaultPopulationInitialisation;

    // Method used to evaluate the current population
    public EvaluationOperator Evaluation = AsyncEvaluation;

    // Method used to calculate the fitness value of each genotype of the current population
    public FitnessCalculation FitnessCalculationMethod = DefaultFitnessCalculation;

    // Method used to select genotypes of the current population and crease the intermediate population
    public SelectionOperator Selection = DefaultFitnessCalculation;

    // Method used to recombine the intermediate population to generate a new population.
    public RecombinationOperator Recombination = DefaultRecombinationOperator;

    // Method used to mutate the new population
    public MutationOperator Mutation = DefaultMutationOperator;

    // Method used to check whether any termination criteria has been met.
    public CheckTerminationCriterion TerminationCriterion = null;

    #endregion



    #endregion
}
