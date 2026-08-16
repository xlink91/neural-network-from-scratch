using DerivationChainRule;
using Serilog;

namespace NumericalMethods;

public class GradientDescend
{
    private Function _function;
    private Placeholder _x;
    private decimal _learningRate = 0.00001m;
    public int Epochs { get; private set; }
    public const decimal Threshold = 0.0001m;
    private const decimal _frictionVelocity = 0.9m;

    private readonly ILogger? _logger;
    public GradientDescend(Function function, int epochs = 1_000, ILogger? logger = null)
    {
        _function = function;
        _x = function.Params.First(p => p.Identifier == "x");
        Epochs = epochs;
        _logger = logger;
    }
    
    public Scalar GetMinimun(GradienDescentAlgorithm algorithm = GradienDescentAlgorithm.GradientDescent)
    {
        if(algorithm == GradienDescentAlgorithm.GradientDescent)
            return GradientDescentRaw();
        if(algorithm == GradienDescentAlgorithm.GradientDescentMomentum)
            return GradientDescentMomentum();
        throw new NotImplementedException($"Algorithm {algorithm} not implemented");
    }


    private Scalar GradientDescentRaw()
    {
        var derivative = new Derivative(_function);
        Function df = derivative.Derive(_x);
        decimal argument = 0;
        for (int i = 0; i < Epochs; i++)
        {
            _x.Scalar = Scalar.Create(argument);
            var gradient = df.Evaluate().Value;
            _logger?.Debug("Epoch {Epoch}: gradient={Gradient}, argument={Argument}", i, gradient, argument);
            if (Math.Abs(gradient) < Threshold)
            {
                break;
            }
            argument -= gradient * _learningRate;
        }
        return argument;
    }

    private Scalar GradientDescentMomentum()
    {
        var derivative = new Derivative(_function);
        Function df = derivative.Derive(_x);
        decimal argument = 0;
        decimal velocity = Threshold;
        for (int i = 0; i < Epochs; i++)
        {
            _x.Scalar = Scalar.Create(argument);
            var gradient = df.Evaluate().Value;
            _logger?.Debug("Epoch {Epoch}: gradient={Gradient}, argument={Argument}", i, gradient, argument);
            if (Math.Abs(gradient) < Threshold)
            {
                break;
            }
            if (Math.Abs(velocity) < Threshold * 1e-5m)
            {
                break;
            }
            velocity = _frictionVelocity * velocity + gradient * _learningRate;
            argument -= velocity;
        }
        return argument;
    }
    
    public enum GradienDescentAlgorithm
    {
        GradientDescent,
        GradientDescentMomentum
    }
}