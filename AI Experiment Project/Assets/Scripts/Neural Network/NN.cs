using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class NN : MonoBehaviour
{
    public Layer[] layers;
    public int[] networkShape = {4, 32, 2};

    public void Awake()
    {
        layers = new Layer[networkShape.Length - 1];
        for (int i = 0; i < layers.Length; i++)
        {
            layers[i] = new Layer(networkShape[i], networkShape[i + 1]);
        }

        //ensure the random number that aren't the same pattern each time.
        UnityEngine.Random.InitState((int) System.DateTime.Now.Ticks);
    }

    //feed input and return the output.
    public float[] Brain(float[] inputs)
    {
        //UnityEngine.Debug.Log($"Running forward pass with inputs: {string.Join(", ", inputs)}");
        for (int i = 0; i < layers.Length; i++)
        {
            if (i == 0)
            {
                layers[i].Forward(inputs);
                layers[i].Activation();
            }
            else if (i == layers.Length - 1)
            {
                layers[i].Forward(layers[i - 1].nodeArray);
            }
            else
            {
                layers[i].Forward(layers[i - 1].nodeArray);
                layers[i].Activation();
            }
        }
        float[] output = layers[layers.Length - 1].nodeArray;

        //UnityEngine.Debug.Log($"Output from forward pass: {string.Join(", ", output)}");
        return output;
    }

    //placeholder function where implement backpropagation 
    public void Train(float[] inputs, float[] expectedOutputs, float learningRate)
    {
        UnityEngine.Debug.Log($"Starting training with inputs: {string.Join(", ", inputs)} and expected outputs: {string.Join(", ", expectedOutputs)}");

        // TODO: Implement the forward pass to get the predictions
        float[] predictions = Brain(inputs);

        // TODO: Implement the calculation of the loss gradient
        float[] lossGradients = CalculateLossGradient(predictions, expectedOutputs);

        // TODO: Implement the backpropagation to get the gradients for weights and biases
        Backpropagate(lossGradients);

        //update weights and biases in all layers.
        UpdateWeights(inputs, learningRate);
    }

    private void Backpropagate(float[] lossGradients)
    {
        UnityEngine.Debug.Log($"Running backpropagation with loss gradients: {string.Join(", ", lossGradients)}");
        //starting from the output layer and moving back towards the input layer
        for (int i = layers.Length - 1; i >= 0; i--)
        {
            //for each neuron in the layer
            for(int j = 0; j < layers[i].n_neurons; j++)
            {
                //if it's the output layer
                if(i == layers.Length - 1)
                {
                    //the gradient is the derivative of the loss functionhttps://127.0.0.1:50230/lol-game-data/assets/ASSETS/Regalia/BannerSkins/wa2023.png
                    layers[i].deltaArray[j] = lossGradients[j];
                }
                else
                {
                    //for hidden layers, sum up all the contributions from the neurons in the next layer
                    float sum = 0;

                    for(int k = 0; k < layers[i + 1].n_neurons; k++)
                    {
                        sum += layers[i + 1].weightsArray[k, j] * layers[i + 1].deltaArray[k];
                    }

                    //multiply by the derivative of the activation function
                    layers[i].deltaArray[j] = sum * DerivativeOfActivationFunction(layers[i].nodeArray[j]);
                }
            }
        }
    }

    private void UpdateWeights(float[] inputs, float learningRate)
    {
        UnityEngine.Debug.Log($"Updating weights and biases with learning rate: {learningRate}");
        //loop through each layer
        for (int i = 0; i < layers.Length;i++)
        {
            //UnityEngine.Debug.Log($"Layer {i} weights after update: {string.Join(", ", layers[i].weightsArray.Cast<float>())}");
            //UnityEngine.Debug.Log($"Layer {i} biases after update: {string.Join(", ", layers[i].biasesArray)}");

            float[] layerInputs = i == 0 ? inputs : layers[i - 1].nodeArray;

            for(int neuronIndex = 0; neuronIndex < layers[i].n_neurons; neuronIndex++)
            {
                for(int weightIndex = 0; weightIndex < layers[i].n_inputs; weightIndex++)
                {
                    //calculate the gradient for the current weight
                    float gradient = CalculateGradient(layerInputs, i, neuronIndex, weightIndex);

                    //update the weight with the gradient
                    //the learning rate is negative since, this is gradient descend
                    layers[i].weightsArray[neuronIndex,weightIndex] -= learningRate * gradient;
                }

                //update biases, using delta values
                layers[i].biasesArray[neuronIndex] -= learningRate * layers[i].deltaArray[neuronIndex];
            }
        }
    }

    private float CalculateGradient(float[] inputs, int layerIndex, int neuronIndex, int weightIndex)
    {
        float delta = layers[layerIndex].deltaArray[neuronIndex];

        float input;
        if(layerIndex == 0)
        {
            //for the first layer, the inputs are the network's inputs
            input = inputs[weightIndex];
        }
        else
        {
            //for subsequent layers. the inputs are the outputs of the previous layer's neuros
            input = layers[layerIndex - 1].nodeArray[weightIndex];
        }

        return delta * input;
    }

    private float[] CalculateLossGradient(float[] predictions, float[] expectedOutputs)
    {
        UnityEngine.Debug.Log($"Calculating loss gradients. Predictions: {string.Join(", ", predictions)}, Expected: {string.Join(", ", expectedOutputs)}");
        if (predictions.Length != expectedOutputs.Length)
        {
            throw new ArgumentException("The predictions and expectedOutputs arrays must be of the same length.");
        }

        float[] lossGradients = new float[predictions.Length];
        
        for(int i = 0; i < predictions.Length; i++)
        {
            lossGradients[i] = 2 * (predictions[i] - expectedOutputs[i]);
        }

        UnityEngine.Debug.Log($"Loss Gradients: {string.Join(", ", lossGradients)}");
        return lossGradients;
    }

    public (float[,], float[]) [] GetWeightsAndBiases()
    {
        var weightsAndBiaese = new (float[,], float [])[layers.Length];
        for(int i = 0; i < layers.Length; i++)
        {
            weightsAndBiaese[i] = (layers[i].weightsArray, layers[i].biasesArray);
        }
        return weightsAndBiaese;
    }

    public void SetWeightsAndBiases((float[,], float[])[] weightsAndBiases)
    {
        for (int i = 0; i < layers.Length; i++)
        {
            layers[i].weightsArray = weightsAndBiases[i].Item1;
            layers[i].biasesArray = weightsAndBiases[i].Item2;
        }
    }

    private float DerivativeOfActivationFunction(float output)
    {
        return output > 0 ? 1f : 0f;
    }

    /* copy the weights and biases from one network to another
    public Layer[] copyLayers()
    {
        Layer[] copiedLayer = new Layer[networkShape.Length];

        for (int i = 0; i < layers.Length; i++)
        {
            copiedLayer[i] = new Layer(networkShape[i], networkShape[i + 1]);
            System.Array.Copy (layers[i].weightsArray, copiedLayer[i].weightsArray, layers[i].weightsArray.GetLength(0) * layers[i].weightsArray.GetLength(1));
            System.Array.Copy (layers[i].biasesArray, copiedLayer[i].biasesArray, layers[i].biasesArray.GetLength(0));
        }
        return (copiedLayer);
    }
    */

    public class Layer
    {
        public float[,] weightsArray;
        public float[] biasesArray;
        public float[] nodeArray;
        public float[] deltaArray;

        public int n_neurons;
        public int n_inputs;

        public Layer(int n_inputs, int n_neurons)
        {
            this.n_neurons = n_neurons;
            this.n_inputs = n_inputs;

            weightsArray = new float[n_neurons, n_inputs];
            biasesArray = new float[n_neurons];

            // Random initialization of weights and biases
            for (int i = 0; i < n_neurons; i++)
            {
                for (int j = 0; j < n_inputs; j++)
                {
                    weightsArray[i, j] = UnityEngine.Random.Range(-1f, 1f); // Replace with your desired range
                }
                biasesArray[i] = UnityEngine.Random.Range(-1f, 1f); // Replace with your desired range
            }
        }

        // take in an array of inputs and returns an array of output.
        public void Forward (float [] inputsArray)
        {
            nodeArray = new float[n_neurons];

            for (int i = 0; i < n_neurons; i++)
            {
                nodeArray[i] = 0;
                //sum of the weights times inputs
                for(int j = 0; j < n_inputs; j++)
                {
                    nodeArray[i] += weightsArray[i,j] * inputsArray[j];
                }

                //add the bias
                nodeArray[i] += biasesArray[i];
            }
        }

        // use ReLU method
        public void Activation()
        {
            for (int i = 0;i < n_neurons; i++)
            {
                nodeArray[i] = Mathf.Max(0, nodeArray[i]);
            }
        }
    }
}