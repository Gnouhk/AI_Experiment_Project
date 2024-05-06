using System;
using System.Collections.Generic;

public class Agent : IComparable<Agent>
{
    #region Members

    // The underlying genotype of this agent
    public Genotype Genotype { get; private set; }

    // The feedforward neural network=
    public NeuralNetwork FNN { get; private set; }

    private bool isAlive = false;

    public bool IsAlive
    {
        get { return isAlive; }
        private set
        {
            if (isAlive != value)
            {
                isAlive = value;

                if (!isAlive && AgentDied != null)
                {
                    AgentDied(this);
                }
            }
        }
    }

    public event Action<Agent> AgentDied;

    #endregion

    #region Constructors

    // Initialises a new agent from given genotype, constructing FNN from the parameters of the genotype
    public Agent(Genotype genotype, NeuralLayer.ActivationFunction defaultActivation, params uint[] topology)
    {
        IsAlive = false;
        this.Genotype = genotype;

        FNN = new NeuralNetwork(topology);

        foreach (NeuralLayer layer in FNN.Layers)
        {
            layer.NeuronActivationFunction = defaultActivation;
        }

        // Check if topology is valid
        if (FNN.WeightCount != genotype.ParameterCount)
        {
            throw new ArgumentException("The given genotype's parameter count must match the neural network topology's weight count");
        }

        // Construct FNN from genotype
        IEnumerator<float> parameters = genotype.GetEnumerator();
        foreach (NeuralLayer layer in FNN.Layers) // Loop all layer
        {
            for (int i = 0; i < layer.Weights.GetLength(0); i++) // Loop all nodes of the current layer
            {
                for (int j = 0; j < layer.Weights.GetLength(1); j++) // Loop all nodes of the next layer
                {
                    layer.Weights[i, j] = parameters.Current;
                    parameters.MoveNext();
                }
            }
        }
    }

    #endregion

    #region Methods

    // Resets this agents to be alive
    public void Reset()
    {
        Genotype.Evaluation = 0;
        Genotype.Fitness = 0;
        isAlive = true;
    }

    // Kill this agent
    public void Kill()
    {
        isAlive = false;
    }

    // Compares this agent to another agent, by comparing their underlying genotype
    public int CompareTo(Agent other)
    {
        return this.Genotype.CompareTo(other.Genotype);
    }

    #endregion

}
