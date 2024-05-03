using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


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

    private GenericAlgorithm genericAlgorithm;

    // The age of the current generation
    public uint GenerationCount
    {
        get { return genericAlgorithm.GenerationCount; }
    }

    #endregion

    #region Constructors

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("More than one EvolutionManager in the Scene");
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
        genericAlgorithm = new GenericAlgorithm((uint)nn.WeightCount, (uint)PopulationSize);
        genotypesSaved = 0;

        genericAlgorithm.Evaluation = StartEvaluation;

        if(ElitistSelection)
        {
            // Second configuration
            genericAlgorithm.Selection = GenericAlgorithm.DefaultSelectionOperator;
            genericAlgorithm.Recombination = RandomRecombination;
            genericAlgorithm.Mutation = MutateAllButBestTwo;
        }
        else
        {
            // First configuration
            genericAlgorithm.Selection = RemainerStochasticSampling;
            genericAlgorithm.Recombination = RandomRecombination;
            genericAlgorithm.Mutation = MutateAllButBestTwo;
        }

        AllAgentsDied += genericAlgorithm.EvaluationFinished;
    
        // Statistics
        if(SaveStatistics)
        {
            statisticsFileName = "Evaluation - " + GameStateManager.Instance.TrackName + " " + DateTime.Now.ToString("yyyy_MM_dd_HH-mm-ss");
        }    
    
    
    }

    

    #endregion
}
