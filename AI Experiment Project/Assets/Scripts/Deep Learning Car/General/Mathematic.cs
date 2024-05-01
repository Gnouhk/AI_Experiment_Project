using System;

public static class Mathematic
{
    #region Methods

    // Standard sigmoid function.
    public static double SigmoidFunction (double xValue)
    {
        if (xValue > 10)
        {
            return 1.0;
        }
        else if (xValue < -10)
        {
            return 0.0;
        }
        else
        {
            return 1.0 / (1.0 + Math.Exp(-xValue));
        }
    }

    // SoftSign function
    public static double SoftSignFunction(double xValue)
    {
        return xValue / (1 + Math.Abs(xValue));
    }
    #endregion
}
