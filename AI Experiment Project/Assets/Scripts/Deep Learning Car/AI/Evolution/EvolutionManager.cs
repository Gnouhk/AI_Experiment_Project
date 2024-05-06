using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Unity.VisualScripting;
using System.Globalization;
using System.Diagnostics;

public class EvolutionManager : MonoBehaviour
{
    #region Members

    private static System.Random randomizer = new System.Random();
    public static EvolutionManager Instance { get; private set; }

    // Write down results of each generation to file.
    [SerializeField]
    private bool SaveStatistics = false;
    private string statisticsFileName;

    // Write down the number of first to finished to file
    [SerializeField]
    private uint SaveFirstNGenotype = 0;
    private uint genotypesSaved = 0;
     
    // Population size
    [SerializeField]
    private int PopulationSize = 30;

    // The generic algo restart after a certain amount of generations
    [SerializeField]
    private int RestartAfter = 100;

    // Use elitist selection or remainder stochastic sampling
    [SerializeField]
    private bool ElitistSelection = false;

    // Topology of the agent's FNN
    [SerializeField]
    private uint[] FNNTopology;

    // The current population
    private List<Agent> agents = new List<Agent>();

    // The amount of agents that currently alive
    public int AgentsAliveCount { get; private set; }

    // Event when all agents died
    public event System.Action AllAgentsDied;

    private GenericAlgorithm geneticAlgorithm;

    // The age of the current generation
    public uint GenerationCount
    {
        get { return geneticAlgorithm.GenerationCount; }
    }

    #endregion

    #region Constructors

    private void Awake()
    {
        if (Instance != null)
        {
            UnityEngine.Debug.LogError("More than one EvolutionManager in the Scene");
            return;
        }
        Instance = this;
    }

    #endregion

    #region Methods

    public void StartEvolution()
    {
        // Create neural network to determine parameter count
        NeuralNetwork nn = new NeuralNetwork(FNNTopology);

        // Setup genetic algorithm 
        geneticAlgorithm = new GenericAlgorithm((uint) nn.WeightCount, (uint) PopulationSize);
        genotypesSaved = 0;

        UnityEngine.Debug.Log("Starting Evolution: Generation Count = " + GenerationCount);

        geneticAlgorithm.Evaluation = StartEvaluation;

        if(ElitistSelection)
        {
            // Second configuration
            geneticAlgorithm.Selection = GenericAlgorithm.DefaultSelectionOperator;
            geneticAlgorithm.Recombination = RandomRecombination;
            geneticAlgorithm.Mutation = MutateAllButBestTwo;
        }
        else
        {
            // First configuration
            geneticAlgorithm.Selection = RemainderStochasticSampling;
            geneticAlgorithm.Recombination = RandomRecombination;
            geneticAlgorithm.Mutation = MutateAllButBestTwo;
        }

        AllAgentsDied += geneticAlgorithm.EvaluationFinished;
    
        // Statistics
        if(SaveStatistics)
        {
            statisticsFileName = "Evaluation - " + GameStateManager.Instance.TrackName + " " + DateTime.Now.ToString("yyyy_MM_dd_HH-mm-ss");
            WriteStatisticsFileStart();
            geneticAlgorithm.FitnessCalculationFinished += WriteStatisticsToFile;
        }

        geneticAlgorithm.FitnessCalculationFinished += CheckForTrackFinished;

        // Restart logic
        if (RestartAfter > 0)
        {
            geneticAlgorithm.TerminationCriterion += CheckGenerationTermination;
            geneticAlgorithm.AlgorithmTerminated += OnGATermination;
        }

        geneticAlgorithm.Start();
        UnityEngine.Debug.Log("Evolution Started: Agents Count = " + agents.Count);
    }

    // Writes the starting line to the statistics file.
    private void WriteStatisticsFileStart()
    {
        File.WriteAllText(statisticsFileName + ".txt", "Evaluation of a Population with size " + PopulationSize +
                ", on Track \"" + GameStateManager.Instance.TrackName + "\", using the following GA operators: " + Environment.NewLine +
                "Selection: " + geneticAlgorithm.Selection.Method.Name + Environment.NewLine +
                "Recombination: " + geneticAlgorithm.Recombination.Method.Name + Environment.NewLine +
                "Mutation: " + geneticAlgorithm.Mutation.Method.Name + Environment.NewLine +
                "FitnessCalculation: " + geneticAlgorithm.FitnessCalculationMethod.Method.Name + Environment.NewLine + Environment.NewLine);
    }

    // Appends the current generation count and evaluation of the best geno in the file
    private void WriteStatisticsToFile(IEnumerable<Genotype> currentPopulation)
    {
        foreach (Genotype genotype in currentPopulation)
        {
            File.AppendAllText(statisticsFileName + ".txt", geneticAlgorithm.GenerationCount + "\t" + genotype.Evaluation + Environment.NewLine);
            break;
        }
    }

    // Check current population and saves geno to a file if their eva is > or = to 1
    private void CheckForTrackFinished(IEnumerable<Genotype> currentPopulation)
    {
        if (genotypesSaved >= SaveFirstNGenotype) return;

        string saveFolder = statisticsFileName + "/";

        foreach (Genotype genotype in currentPopulation)
        {
            if (genotype.Evaluation >= 1)
            {
                if (!Directory.Exists(saveFolder))
                {
                    Directory.CreateDirectory(saveFolder);
                }

                genotype.SaveToFile(saveFolder + "Genotype - Finished as " + (++genotypesSaved) + ".txt");

                if (genotypesSaved >= SaveFirstNGenotype) return;
            }
            else
            {
                return;
            }
        }
    }

    // Check whether the termination criterion of generation count was met
    private bool CheckGenerationTermination(IEnumerable<Genotype> currentPopulation)
    {
        return geneticAlgorithm.GenerationCount >= RestartAfter;
    }

    // Called when the genetic algo was terminated
    private void OnGATermination(GenericAlgorithm ga)
    {
        AllAgentsDied -= ga.EvaluationFinished;

        RestartAlgorithm(5.0f);
    }

    // Restart the algo after a time.
    private void RestartAlgorithm(float wait)
    {
        Invoke("StartEvolution", wait);
    }

    // Start evaluation by creating new agents from the current population and restarting track manager.
    private void StartEvaluation(IEnumerable<Genotype> currentPopulation)
    {
        // Create new agents
        agents.Clear();
        AgentsAliveCount = 0;

        foreach (Genotype genotype in currentPopulation)
        {
            agents.Add(new Agent(genotype, Mathematic.SoftSignFunction, FNNTopology));
        }

        TrackManager.Instance.SetCarAmount(agents.Count);
        IEnumerator<CarController> carsEnum = TrackManager.Instance.GetCarEnumerator();

        for(int i = 0; i < agents.Count; i++)
        {
            if(!carsEnum.MoveNext())
            {
                UnityEngine.Debug.LogError("Cars enum ended before agents");
                break;
            }

            carsEnum.Current.Agent = agents[i];
            AgentsAliveCount++;
            agents[i].AgentDied += OnAgentDied;
        }

        TrackManager.Instance.Restart();
    }

    // When an agent died
    private void OnAgentDied(Agent agent)
    {
        AgentsAliveCount--;
        if (AgentsAliveCount == 0 && AllAgentsDied != null) 
        {
            AllAgentsDied();
        }
    }

    // Selection operator for the genetic algo, using a method called remainder stochastic sampling.
    private List<Genotype> RemainderStochasticSampling(List<Genotype> currentPopulation)
    {
        List<Genotype> intermediatePopulation = new List<Genotype>();

        // Put integer portion of genotypes into intermediatePopulation

        foreach (Genotype genotype in currentPopulation)
        {
            if (genotype.Fitness < 1)
            {
                break;
            }
            else
            {
                for (int i = 0; i < (int) genotype.Fitness; i++)
                {
                    intermediatePopulation.Add(new Genotype(genotype.GetParameterCopy()));
                }
            }
        }

        // Put remainder portion of genotypes into intermediatePopulation
        foreach (Genotype genotype in currentPopulation)
        {
            float remainder = genotype.Fitness - (int)genotype.Fitness;
            
            if(randomizer.NextDouble() < remainder)
            {
                intermediatePopulation.Add(new Genotype(genotype.GetParameterCopy()));
            }
        } 

        return intermediatePopulation;
    }

    // Recombination operator for the genetic algo, recombining random genotypes.
    private List<Genotype> RandomRecombination(List<Genotype> intermediatePopulation, uint newPopulationSize)
    {
        if(intermediatePopulation.Count < 2)
        {
            throw new ArgumentException("The intermediate population has to be at least of size 2 for this operator.");
        }

        List<Genotype> newPopulation = new List<Genotype>();

        // Always add best two
        newPopulation.Add(intermediatePopulation[0]);
        newPopulation.Add(intermediatePopulation[1]);

        while (newPopulation.Count < newPopulationSize)
        {
            // Get two random that are not the same
            int randomIndex1 = randomizer.Next(0, intermediatePopulation.Count), randomIndex2;
            do
            {
                randomIndex2 = randomizer.Next(0, intermediatePopulation.Count);
            } while (randomIndex2 == randomIndex1);

            Genotype offspring1, offspring2;
            GenericAlgorithm.CompleteCrossover(intermediatePopulation[randomIndex1], intermediatePopulation[randomIndex2],
                GenericAlgorithm.DefCrossSwapProp, out offspring1, out offspring2);

            newPopulation.Add(offspring1);
            if (newPopulation.Count < newPopulationSize)
            {
                newPopulation.Add(offspring2);
            }
        }

        return newPopulation;
    }

    // Mutates all members of the new population
    private void MutateAllButBestTwo(List<Genotype> newPopulation)
    {
        for (int i = 2; i < newPopulation.Count; i++) 
        {
            if (randomizer.NextDouble() < GenericAlgorithm.DefMutationPerc)
            {
                GenericAlgorithm.MutateGenotype(newPopulation[i], GenericAlgorithm.DefMutationProp, GenericAlgorithm.DefMutationAmount); ;
            }
        }
    }

    private void MutateAll(List<Genotype> newPopulation)
    {
        foreach (Genotype genotype in newPopulation)
        {
            if(randomizer.NextDouble() < GenericAlgorithm.DefMutationPerc)
            {
                GenericAlgorithm.MutateGenotype(genotype, GenericAlgorithm.DefMutationProp, GenericAlgorithm.DefMutationAmount);
            }
        }
    }

    #endregion
}
