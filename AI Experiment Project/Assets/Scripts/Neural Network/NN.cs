using System;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

public class NN : MonoBehaviour
{
    public Layer[] layers;
    public int[] networkShape = { 2, 4, 3, 2 };

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
        UnityEngine.Debug.Log("NN Input: " + string.Join(", ", inputs.Select(i => i.ToString()).ToArray()));

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

        UnityEngine.Debug.Log("NN Output: " + string.Join(", ", output.Select(o => o.ToString()).ToArray()));

        return output;
    }

    //copy the weights and biases from one network to another
    public Layer[] copyLayers()
    {
        Layer[] copiedLayer = new Layer[networkShape.Length];

        for (int i = 0; i < layers.Length; i++)
        {
            copiedLayer[i] = new Layer(networkShape[i], networkShape[i + 1]);
            System.Array.Copy (layers[i].weightsArray, copiedLayer[i].weightsArray, layers[i].weightsArray.GetLength(0) * layers[i].weightsArray.GetLength(1));
            System.Array.Copy(layers[i].biasesArray, copiedLayer[i].biasesArray, layers[i].biasesArray.GetLength(0));
        }

        return (copiedLayer);
    }

    public class Layer
    {
        public float[,] weightsArray;
        public float[] biasesArray;
        public float[] nodeArray;

        private int n_neurons;
        private int n_inputs;

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
            UnityEngine.Debug.Log("Forward function received inputs: " + string.Join(", ", inputsArray));

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

            UnityEngine.Debug.Log("Output before activation: " + string.Join(", ", nodeArray));
        }

        // use ReLU method
        public void Activation()
        {
            UnityEngine.Debug.Log("Activation function received inputs: " + string.Join(", ", nodeArray));

            for (int i = 0;i < n_neurons; i++)
            {
                nodeArray[i] = Mathf.Max(0, nodeArray[i]);
            }

            UnityEngine.Debug.Log("Output after activation: " + string.Join(", ", nodeArray));
        }
    }
}
