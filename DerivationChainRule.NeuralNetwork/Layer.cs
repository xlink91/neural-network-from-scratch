namespace DerivationChainRule.NeuralNetwork;

public class Layer
{
    public Neuron[] Inputs { get; private set; }
    public Function[] Weights { get; private set; }
    public Function Bias { get; private set; } = Function.B;
    public Function F { get; private set; }
    
    public Layer(Neuron[] inputs)
    {
        Inputs = inputs;
        Weights = new Function[inputs.Length];
        for (int i = 0; i < inputs.Length; i++)
        {
            Weights[i] = Function.W;
        }
        F = Compute();
    }
    
    public static Layer Create(Neuron[] inputs)
    {
        return new Layer(inputs);
    }

    private Function Compute()
    {
        if (Weights.Length != Inputs.Length)
            throw new ArgumentException("Weights length must match inputs length.");
        Function sum = Bias;
        for (int i = 0; i < Inputs.Length; i++)
        {
            sum += Inputs[i].F * Weights[i];
        }
        return Function.Tanh(sum);
    }
}