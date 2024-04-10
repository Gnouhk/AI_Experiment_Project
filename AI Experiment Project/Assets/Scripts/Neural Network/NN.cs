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
        Random.InitState((int) System.DateTime.Now.Ticks);
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

        return (layers[layers.Length - 1].nodeArray);
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
        }

        // take in an array of inputs and returns an array of output.
        public void Forward (float [] inputsArray)
        {
            nodeArray = new float[n_neurons];

            for(int i = 0; i < n_neurons; i++)
            {
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
            for(int i = 0;i < n_neurons; i++)
            {
                if(nodeArray[i] < 0)
                {
                    nodeArray[i] = 0;
                }
            }
        }
    }
}
