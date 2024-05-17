using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;

public class Genotype : IComparable<Genotype>, IEnumerable<float>
{
    #region Members

    private static Random randomizer = new Random();

    // Current evaluation of this genotype
    public float Evaluation { get; set; }

    // Current fitness
    public float Fitness { get; set; }

    // The vector of parameters of this genotype
    private float[] parameters;
    public int ParameterCount
    {
        get
        {
            if(parameters == null)
            {
                return 0;
            }
            return parameters.Length;
        }
    }

    // Overriden indexer for convenient parameter access
    public float this [int index]
    {
        get { return parameters [index]; }
        set { parameters [index] = value; }
    }

    #endregion

    #region Constructors

    // Instance of a new genotype with given parameter vecor and initial fitness of 0
    public Genotype(float[] parameters)
    {
        this.parameters = parameters;
        Fitness = 0;
    }

    #endregion

    #region Methods

    // Compare this genotype with another genotype depending on their fitness values
    public int CompareTo(Genotype other)
    {
        return other.Fitness.CompareTo(this.Fitness);
    }

    // Get an Enumerator to iterate over all parameters of this genotype
    public IEnumerator<float> GetEnumerator()
    {
        for(int i = 0; i <parameters.Length; i++)
        {
            yield return parameters [i];
        }
    }

    // Get an Enumerator to iterate over all parameters of this genotype
    IEnumerator IEnumerable.GetEnumerator()
    {
        for (int i = 0; i < parameters.Length;i++)
        {
            yield return parameters[i];
        }
    }

    // Set the parameters of this genotype to random values in the given range
    public void SetRandomParameters(float minValue, float maxValue)
    {
        if(minValue > maxValue)
        {
            throw new ArgumentException("Minimum value may not exceed maximum value");
        }

        float range = maxValue - minValue;
        for (int i = 0; i < parameters.Length;i++)
        {
            //Create a random float between minValue and maxValue
            parameters [i] = (float)((randomizer.NextDouble() * range) + minValue);
        }
    }

    // Return a copy of the parameter vector
    public float[] GetParameterCopy()
    {
        float[] copy = new float[ParameterCount];

        for(int i = 0; i < ParameterCount; i++)
        {
            copy[i] = parameters [i];   
        }

        return copy;
    }

    // Save the parameters of this genotype to a file at given file path
    public void SaveToFile(string filePath)
    {
        StringBuilder builder = new StringBuilder();

        foreach(float panam in parameters)
        {
            builder.Append(panam.ToString()).Append(";");
        }

        builder.Remove(builder.Length - 1, 1);

        File.WriteAllText(filePath, builder.ToString());
    }

    // Generates a random genotype with parameters in given range
    public static Genotype GenerateRandom(uint parameterCount, float minValue, float maxValue)
    {
        if (parameterCount == 0)
        {
            return new Genotype(new float[0]);
        }

        Genotype randomGenotype = new Genotype(new float[parameterCount]);
        randomGenotype.SetRandomParameters(minValue, maxValue);

        return randomGenotype;
    }

    // Loads a genotype from a file with given file path
    public static Genotype LoadFromFile(string filePath)
    {
        string data = File.ReadAllText(filePath);

        List<float> parameters = new List<float>();
        string[] paramStrings = data.Split(';');

        foreach (string parameter in paramStrings)
        {
            float parsed;

            if (!float.TryParse(parameter, out parsed)) 
            {
                throw new ArgumentException("The file at given file path does not contain a valid genotype serialisation.");
            }
            parameters.Add(parsed);

        }
        return new Genotype(parameters.ToArray());
    }

    #endregion
}
