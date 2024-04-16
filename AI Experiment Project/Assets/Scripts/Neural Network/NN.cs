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

        return output;
    }

    //placeholder function where implement backpropagation 
    public void Train(float[] inputs, float[] expectedOutputs, float learningRate)
    {
        // TODO: Implement the forward pass to get the predictions
        float[] predictions = Brain(inputs);

        // TODO: Implement the calculation of the loss gradient
        float[] lossGradients = CalculateLossGradient(predictions, expectedOutputs);

        // TODO: Implement the backpropagation to get the gradients for weights and biases
        Backpropagate(lossGradients);

        //update weights and biases in all layers.
        UpdateWeights(learningRate);
    }

    private void Backpropagate(float[] lossGradients)
    {
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

    private void UpdateWeights(float learningRate)
    {
        //loop through each layer
        for(int i = 0; i < layers.Length;i++)
        {
            //update weight and bias in the layer based on the stored gradients
            //
        }
    }

    private float CalculateGradient(int layerIndex, int neuronIndex, int weightIndex)
    {
        float delta = layers[layerIndex].deltaArray[neuronIndex];

        float input = layerIndex == 0 ? inputs[weightIndex] : layers[layerIndex - 1].nodeArray[weightIndex];

        return input * delta;
    }

    private float[] CalculateLossGradient(float[] predictions, float[] expectedOutputs)
    {


        return new float[predictions.Length];
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