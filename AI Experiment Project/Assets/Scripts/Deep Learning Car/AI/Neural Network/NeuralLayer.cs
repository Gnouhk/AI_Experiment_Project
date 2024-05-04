using System;

// Representing a single layer of a fully connected feedforward neural network
public class NeuralLayer
{
    #region Members

    private static Random randomizer = new Random();

    // Delegate representing the activation function of an artificial neuron.
    public delegate double ActivationFunction(double xValue); // The input value of the function, return the calculated output value of the function

    // The activation function used by the neurons of this layer
    public ActivationFunction NeuronActivationFunction = Mathematic.SigmoidFunction;

    public uint NeuronCount { get; private set; }
    public uint OutputCount { get; private set; }
    public double[,] Weights { get; private set; }

    #endregion

    #region Constructors

    // Initialises a new neural layer
    public NeuralLayer(uint nodeCount, uint outputCount)
    {
        this.NeuronCount = nodeCount;
        this.OutputCount = outputCount;

        Weights = new double[nodeCount + 1, outputCount]; // +1 for bias node
    }

    #endregion

    #region Method

    // Set the weights of this layer to the given values
    public void SetWeights(double[] weights)
    {
        if(weights.Length != this.Weights.Length)
        {
            throw new ArgumentException("Input weights do not match the layer weight count");
        }

        // Copy weight from given value array
        int k = 0;
        for (int i = 0; i < this.Weights.GetLength(0); i++)
        {
            for (int j = 0; j < this.Weights.GetLength(1); j++) 
            {
                this.Weights[i, j] = weights[k++];
            }
        }
    }

    // Processes the given inputs using the current weights to the next layer
    public double[] ProcessInputs(double[] inputs)
    {
        if(inputs.Length != NeuronCount)
        {
            throw new ArgumentException("Given xValues do not match layer input count");
        }

        // Calculate sum for each neuron from the weighted inputs and biases
        double[] sums = new double[OutputCount];

        // Add bias neuron to inputs
        double[] biasedInputs = new double[NeuronCount + 1];
        inputs.CopyTo(biasedInputs, 0);
        biasedInputs[inputs.Length] = 1.0;

        for (int j = 0; j < this.Weights.GetLength(1); j++)
        {
            for (int i = 0; i < this.Weights.GetLength(0); i++)
            {
                sums[j] += biasedInputs[i] * Weights[i, j];
            }
        }

        // Apply activation function to sum, if set
        if (NeuronActivationFunction != null)
        {
            for (int i = 0; i < sums.Length; i++)
            {
                sums[i] = NeuronActivationFunction(sums[i]);
            }
        }

        return sums;
    }

    // Copies this NeuralLayer including its weights
    public NeuralLayer DeepCopy()
    {
        // Copy weights
        double[,] copiedWeights = new double[this.Weights.GetLength(0), this.Weights.GetLength(1)];

        for (int x = 0; x < this.Weights.GetLength(0); x++)
        {
            for (int y = 0; y < this.Weights.GetLength(1); y++)
            {
                copiedWeights[x, y] = this.Weights[x, y];
            }
        }

        // Create copy
        NeuralLayer newLayer = new NeuralLayer(this.NeuronCount, this.OutputCount);
        newLayer.Weights = copiedWeights;
        newLayer.NeuronActivationFunction = this.NeuronActivationFunction;

        return newLayer;
    }

    // Sets the weights of the connection from this layer to the next random values in given range.
    public void SetRandomWeights(double minValue, double maxValue)
    {
        double range = Math.Abs(minValue - maxValue);

        for (int i = 0; i < Weights.GetLength(0); i++)
        {
            for (int j = 0; j < Weights.GetLength(1); j++)
            {
                Weights[i, j] = minValue + (randomizer.NextDouble() * range); // Random double between minValue and maxValue
            }
        }
    }

    // Return a string representing this layer's connection weights
    public override string ToString()
    {
        string output = "";

        for (int x = 0; x < Weights.GetLength(0); x++)
        {
            for (int y = 0; y < Weights.GetLength(1); y++)
            {
                output += "[" + x + "," + y + "]: " + Weights[x, y];
            }

            output += "\n";
        }

        return output;
    }

    #endregion
}