using System.Runtime.CompilerServices;
using DerivationChainRule;
using Serilog;

namespace NumericalMethods;

public class GradientDescend
{
    private Function _function;
    private decimal _learningRate = 0.00001m;
    public int Epochs { get; private set; }
    public const decimal Threshold = 0.0001m;
    private const decimal _frictionVelocity = 0.9m;

    private readonly ILogger? _logger;
    public GradientDescend(Function function, int epochs = 1_000, ILogger? logger = null)
    {
        _function = function;
        Epochs = epochs;
        _logger = logger;
    }

    public Placeholder[] Train(TrainingData data)
    {
        foreach (var placeholder in _function.Params)
        {
            if (!data.Contains(placeholder))
            {
                placeholder.Scalar = (decimal)Random.Shared.NextDouble() + 1e-10m;
            }
        }
        for (int i = 0; i < Epochs; i++)
        {
            decimal maxDerivative = decimal.MinValue;
            foreach (Placeholder arg in _function.Params)
            {
                if(data.Contains(arg))
                {
                    continue;
                }
                decimal gradientArg = 0m;
                foreach (TrainingEntry entry in data.Entries)
                {
                    Function smm = (Function.Create(entry.DependentVariable) - _function) * (Function.Create(entry.DependentVariable) - _function) * Function.Create(1.0m/data.Entries.Count);
                    entry.IndependentVariables.ToList().ForEach(x => x.Item1.Scalar = x.Item2);
                    var derivative = new Derivative(smm);
                    var df = derivative.Derive(arg);
                    gradientArg += df.Evaluate().Value;
                }
                arg.Scalar -= gradientArg * _learningRate;
                _logger?.Debug("Epoch {Epoch}: gradient={Gradient}, identifier={Identifier}, value={Value}", i, gradientArg, arg.Identifier, arg.Scalar.Value);
                maxDerivative = Math.Max(maxDerivative, Math.Abs(gradientArg));
            }
            if (Math.Abs(maxDerivative) < Threshold)
            {
                break;
            }
        }
        return _function.Params.ToArray();
    }
    
    public Placeholder[] GetMinimun(GradienDescentAlgorithm algorithm = GradienDescentAlgorithm.GradientDescent)
    {
        if(algorithm == GradienDescentAlgorithm.GradientDescent)
            return GradientDescentRaw();
        if(algorithm == GradienDescentAlgorithm.GradientDescentMomentum)
            return GradientDescentMomentum();
        throw new NotImplementedException($"Algorithm {algorithm} not implemented");
    }
    
    private Placeholder[] GradientDescentRaw()
    {
        var derivative = new Derivative(_function);
        HashSet<string> identifierAsConstant = new HashSet<string>();
        Dictionary<string, Placeholder> arguments = _function
            .Params
            .ToDictionary(x => x.Identifier, x =>
            {
                if (x.Scalar != null)
                {
                    identifierAsConstant.Add(x.Identifier);
                }
                else
                {
                    x.Scalar = (decimal)Random.Shared.NextDouble() + 1e-10m;
                }
                return x;
            });
        for (int i = 0; i < Epochs; i++)
        {
            decimal maxDerivative = decimal.MinValue;
            foreach (Placeholder arg in _function.Params)
            {
                if (identifierAsConstant.Contains(arg.Identifier))
                {
                    continue;
                }
                var df = derivative.Derive(arg);
                var gradientArg = df.Evaluate().Value;
                arguments[arg.Identifier].Scalar -= gradientArg * _learningRate;
                _logger?.Debug("Epoch {Epoch}: gradient={Gradient}, identifier={Identifier}, value={Value}", i, gradientArg, arg.Identifier, arg.Scalar.Value);
                maxDerivative = Math.Max(maxDerivative, Math.Abs(gradientArg));
            }
            if (Math.Abs(maxDerivative) < Threshold)
            {
                break;
            }
        }
        return arguments.Select(x => x.Value).ToArray();
    }

    private Placeholder[] GradientDescentMomentum()
    {
        var derivative = new Derivative(_function);
        HashSet<string> identifierAsConstant = new HashSet<string>();
        Dictionary<string, Placeholder> arguments = _function
            .Params
            .ToDictionary(x => x.Identifier, x =>
            {
                if (x.Scalar != null)
                {
                    identifierAsConstant.Add(x.Identifier);
                }
                else
                {
                    x.Scalar = (decimal)Random.Shared.NextDouble() + 1e-10m;
                }
                return x;
            });
        Dictionary<string, decimal> velocities = arguments.ToDictionary(x => x.Key, x => Threshold);
        for (int i = 0; i < Epochs; i++)
        {
            foreach (Placeholder arg in _function.Params)
            {
                if (identifierAsConstant.Contains(arg.Identifier))
                {
                    continue;
                }
                Function df = derivative.Derive(arg);
                var gradientArg = df.Evaluate().Value;
                _logger?.Debug("Epoch {Epoch}: gradient={Gradient}, argument={Argument}", i, gradientArg, arguments[arg.Identifier]);
                if (Math.Abs(gradientArg) < Threshold)
                {
                    break;
                }
                if (Math.Abs(velocities[arg.Identifier]) < Threshold * 1e-5m)
                {
                    break;
                }
                velocities[arg.Identifier] = _frictionVelocity * velocities[arg.Identifier] + gradientArg * _learningRate;
                arguments[arg.Identifier].Scalar -= velocities[arg.Identifier];
            }
        }
        return arguments.Select(x => x.Value).ToArray();
    }
    
    public enum GradienDescentAlgorithm
    {
        GradientDescent,
        GradientDescentMomentum
    }
}

public class TrainingEntry
{
    public decimal DependentVariable { get; set; }
    public (Placeholder, Scalar)[] IndependentVariables { get; set; }

    public static TrainingEntry Create(decimal dependentVariable, (Placeholder, Scalar)[] dependentVariables)
    {
        return new TrainingEntry
        {
            DependentVariable = dependentVariable,
            IndependentVariables = dependentVariables
        };
    }
}

public class TrainingData
{
    public List<TrainingEntry> Entries { get; private set; } = new List<TrainingEntry>();
    HashSet<Placeholder> _placeholders = new HashSet<Placeholder>();
    public static TrainingData Create()
    {
        return new();
    }
    public TrainingData Add(TrainingEntry entry)
    {
        Entries.Add(entry);
        foreach (var (placeholder, _) in entry.IndependentVariables)
        {
            _placeholders.Add(placeholder);
        }
        return this;
    }
    public bool Contains(Placeholder placeholder)
    {
        return _placeholders.Contains(placeholder);
    }
}