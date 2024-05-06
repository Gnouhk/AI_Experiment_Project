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
    public const float DefCrossSwapProb = 0.6f;

    // Default probability of a parameter being mutated.
    public const float DefMutationProb = 0.3f;

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
    public InitialisationOperator InitialisePopulation = DefaultPopulationInitialisation;

    // Method used to evaluate the current population
    public EvaluationOperator Evaluation = AsyncEvaluation;

    // Method used to calculate the fitness value of each genotype of the current population
    public FitnessCalculation FitnessCalculationMethod = DefaultFitnessCalculation;

    // Method used to select genotypes of the current population and crease the intermediate population
    public SelectionOperator Selection = DefaultSelectionOperator;

    // Method used to recombine the intermediate population to generate a new population.
    public RecombinationOperator Recombination = DefaultRecombinationOperator;

    // Method used to mutate the new population
    public MutationOperator Mutation = DefaultMutationOperator;

    // Method used to check whether any termination criteria has been met.
    public CheckTerminationCriterion TerminationCriterion = null;

    #endregion

    private static Random randomizer = new Random();

    private List<Genotype> currentPopulation;

    public uint PopulationSize { get; private set; }
    public uint GenerationCount { get; private set; }
    
    // Whether the current population be sorted before calling the termination criterion operator
    public bool SortPopulation { get; private set; }
    public bool Running { get; private set; }

    // Event for when the algo is eventually terminated.
    public event System.Action<GenericAlgorithm> AlgorithmTerminated;

    // Event for when the algorithm has finished fitness calculation
    public event System.Action<IEnumerable<Genotype>> FitnessCalculationFinished;

    #endregion

    #region Contructors

    // Init a new generic algo instance, creating a initial population of given size.
    public GenericAlgorithm(uint genotypeParamCount, uint populationSize)
    {
        this.PopulationSize = populationSize;

        // Init empty population
        currentPopulation = new List<Genotype>((int) populationSize); 

        for (int i = 0; i <populationSize; i++)
        {
            currentPopulation.Add(new Genotype(new float[genotypeParamCount]));
        }

        GenerationCount = 1;
        SortPopulation = true;
        Running = false;
    }

    #endregion

    #region Methods

    public void Start()
    {
        Running = true;

        InitialisePopulation(currentPopulation);

        Evaluation(currentPopulation);
    }

    public void EvaluationFinished()
    {
        // Calculate fitness from evaluation
        FitnessCalculationMethod(currentPopulation);

        // Sort population if flag was set
        if (SortPopulation)
        {
            currentPopulation.Sort();
        }

        // Fire fitness calcualtion finished event
        if(FitnessCalculationFinished != null)
        {
            FitnessCalculationFinished(currentPopulation);
        }

        // Check termination criterion
        if (TerminationCriterion != null && TerminationCriterion(currentPopulation))
        {
            Terminate();
            return;
        }

        // Apply selection 
        List<Genotype> intermediatePopulation = Selection(currentPopulation);

        // Apply Recombination
        List<Genotype> newPopulation = Recombination(intermediatePopulation, PopulationSize);

        // Apply Mutation
        Mutation(newPopulation);

        // Set current population to newly generated one and start evaluation again
        currentPopulation = newPopulation;
        GenerationCount++;

        Evaluation(currentPopulation);
    }

    private void Terminate()
    {
        Running = false;
        if (AlgorithmTerminated != null)
        {
            AlgorithmTerminated(this);
        }
    }

    // Init the population by setting each parameter to a random value in the default range
    public static void DefaultPopulationInitialisation(IEnumerable<Genotype> population)
    {
        // Set parameters to random values in set range
        foreach (Genotype genotype in population)
        {
            genotype.SetRandomParameters(DefInitParamMin, DefInitParamMax);
        }
    }

    public static void AsyncEvaluation (IEnumerable<Genotype> currentPopulation)
    {

    }

    // Calculates the fitness of each genotype
    public static void DefaultFitnessCalculation(IEnumerable<Genotype> currentPopulation)
    {
        // Calcualate average evaluation of whole population
        uint populationSize = 0;
        float overallEvaluation = 0;

        foreach (Genotype genotype in currentPopulation)
        {
            overallEvaluation += genotype.Evaluation;
            populationSize++;
        }

        float averageEvaluation = overallEvaluation / populationSize;

        // Assign fitness
        foreach (Genotype genotype in currentPopulation)
        {
            genotype.Fitness = genotype.Evaluation / averageEvaluation;
        }
    }

    // Select the best three genotypes of the current population and copies them to intermediate population
    public static List<Genotype> DefaultSelectionOperator(List<Genotype> currentPopulation)
    {
        List<Genotype> intermediatePopulation = new List<Genotype>();
        intermediatePopulation.Add(currentPopulation[0]);
        intermediatePopulation.Add(currentPopulation[1]);
        intermediatePopulation.Add(currentPopulation[2]);

        return intermediatePopulation;
    }

    // Crosses the first with the second genotype if the intermediate population
    public static List<Genotype> DefaultRecombinationOperator(List<Genotype> intermediatePopulation, uint newPopulationSize)
    {
        if(intermediatePopulation.Count < 2)
        {
            throw new ArgumentException("Intermediate population size must be greater than 2 for this operator.");
        }

        List<Genotype> newPopulation = new List<Genotype>();
        while (newPopulation.Count < newPopulationSize)
        {
            Genotype offspring1, offspring2;
            CompleteCrossover(intermediatePopulation[0], intermediatePopulation[1], DefCrossSwapProb, out offspring1, out offspring2);

            newPopulation.Add(offspring1);
            if (newPopulation.Count < newPopulationSize)
            {
                newPopulation.Add(offspring2);
            }
        }

        return newPopulation;
    }

    // Mutate each genotype with the default mutation probability and amount
    public static void DefaultMutationOperator(List<Genotype> newPopulation)
    {
        foreach (Genotype genotype in newPopulation)
        {
            if (randomizer.NextDouble() < DefMutationPerc)
            {
                MutateGenotype(genotype, DefMutationProb, DefMutationAmount);
            }
        }
    }

    public static void CompleteCrossover(Genotype parent1, Genotype parent2, float swapChance, out Genotype offspring1, out Genotype offspring2)
    {
        // Init new parameter vector
        int parameterCount = parent1.ParameterCount;
        float[] off1Parameters = new float[parameterCount], off2Parameters = new float[parameterCount];

        // Iterate over all parameters randomly swapping
        for (int i = 0; i < parameterCount; i++)
        {
            if (randomizer.Next() < swapChance)
            {
                // Swap parameters
                off1Parameters[i] = parent2[i];
                off2Parameters[i] = parent1[i];
            }
            else
            {
                // Don't swap parameters
                off1Parameters[i] = parent1[i];
                off2Parameters[i] = parent2[i];
            }
        }

        offspring1 = new Genotype(off1Parameters);
        offspring2 = new Genotype(off2Parameters);
    }

    public static void MutateGenotype(Genotype genotype, float mutationProb, float mutationAmount)
    {
        for (int i = 0; i < genotype.ParameterCount; i++)
        {
            if (randomizer.NextDouble() < mutationProb)
            {
                // Mutate by random amount in range [-mutationAmount, mutationAmount]
                genotype[i] += (float)(randomizer.NextDouble() * (mutationAmount * 2) - mutationAmount);
            }
        }
    }

    #endregion
}
