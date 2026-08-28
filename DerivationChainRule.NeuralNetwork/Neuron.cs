namespace DerivationChainRule.NeuralNetwork;

public class Neuron
{
    public Placeholder[] Inputs { get; private set; }
    public Placeholder[] Weights { get; private set; } 
    public Placeholder Biase { get; private set; }
    public Function F { get; private set; }
    private Neuron(int input_size)
    {
        Inputs = [.. Enumerable.Range(0, input_size).Select(i => Placeholder.X)];
        Weights = [.. Enumerable.Range(0, input_size).Select(i => Placeholder.W)];
        Biase = Placeholder.B;
        F = Compute();
    }
    private Neuron(Placeholder[] placeholders)
    {
        Inputs = placeholders;
        Weights = [.. Enumerable.Range(0, placeholders.Length).Select(i => Placeholder.W)];
        Biase = Placeholder.B;
        F = Compute();
    }
    public static Neuron Create(int input_size)
    {
        return new Neuron(input_size);
    }
    public static Neuron Create(Placeholder[] placeholders)
    {
        return new Neuron(placeholders);
    }
    private Function Compute()
    {
        if (Weights.Length != Inputs.Length)
            throw new ArgumentException("Weights length must match inputs length.");
        Function sum = Function.Create(Biase);
        for (int i = 0; i < Inputs.Length; i++)
        {
            sum += Function.Create(Inputs[i]) * Function.Create(Weights[i]);
        }
        return Function.Tanh(sum);
    }
}