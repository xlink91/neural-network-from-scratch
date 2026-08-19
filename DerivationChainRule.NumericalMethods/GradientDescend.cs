using DerivationChainRule;
using Serilog;

namespace NumericalMethods;

public class GradientDescend
{
    private Function _function;
    private double _learningRate = 0.00001;
    public int Epochs { get; private set; }
    public const double Threshold = 0.0001;
    private const double _frictionVelocity = 0.9;

    private readonly ILogger? _logger;
    public GradientDescend(Function function, int epochs = 1_000, ILogger? logger = null)
    {
        _function = function;
        Epochs = epochs;
        _logger = logger;
    }

    // Fits the function's free parameters to the training data by gradient descent on the
    // mean squared error. The target value is a placeholder ("__target", reserved), so the
    // loss tree and its gradient tape are built ONCE for the whole run; substituting a
    // training entry is just assigning placeholder values — no tree building or
    // re-differentiation ever happens inside the loops.
    public Placeholder[] Train(TrainingData data)
    {
        Placeholder target = Placeholder.Create("__target");
        Function error = Function.Create(target) - _function;
        Function loss = error * error;
        GradientTape tape = new GradientTape(loss);

        Placeholder[] parameters = _function.Params;
        List<Placeholder> trainable = new List<Placeholder>();
        foreach (Placeholder placeholder in parameters)
        {
            if (!data.Contains(placeholder))
            {
                placeholder.Scalar = Scalar.Create(Random.Shared.NextDouble() + 1e-10);
                trainable.Add(placeholder);
            }
        }
        int entryCount = data.Entries.Count;
        double[] accumulatedGradients = new double[trainable.Count];
        for (int epoch = 0; epoch < Epochs; epoch++)
        {
            Array.Clear(accumulatedGradients);
            foreach (TrainingEntry entry in data.Entries)
            {
                foreach ((Placeholder input, Scalar value) in entry.IndependentVariables)
                {
                    input.Scalar = value;
                }
                target.Scalar = Scalar.Create(entry.DependentVariable);
                tape.Compute();
                for (int p = 0; p < trainable.Count; p++)
                {
                    accumulatedGradients[p] += tape.Gradient(trainable[p]);
                }
            }
            double maxGradient = 0;
            for (int p = 0; p < trainable.Count; p++)
            {
                double gradient = accumulatedGradients[p] / entryCount;
                Placeholder parameter = trainable[p];
                parameter.Scalar = Scalar.Create(parameter.Scalar.Value.Value - gradient * _learningRate);
                _logger?.Debug("Epoch {Epoch}: gradient={Gradient}, identifier={Identifier}, value={Value}", epoch, gradient, parameter.Identifier, parameter.Scalar.Value.Value);
                maxGradient = Math.Max(maxGradient, Math.Abs(gradient));
            }
            if (maxGradient < Threshold)
            {
                break;
            }
        }
        return parameters;
    }

    public Placeholder[] GetMinimun(GradienDescentAlgorithm algorithm = GradienDescentAlgorithm.GradientDescent)
    {
        if(algorithm == GradienDescentAlgorithm.GradientDescent)
            return GradientDescentRaw();
        if(algorithm == GradienDescentAlgorithm.GradientDescentMomentum)
            return GradientDescentMomentum();
        throw new NotImplementedException($"Algorithm {algorithm} not implemented");
    }

    // Placeholders that already carry a value are treated as fixed inputs; the rest are
    // randomly initialized and optimized. One tape pass per epoch yields every partial
    // derivative at once.
    private Placeholder[] GradientDescentRaw()
    {
        GradientTape tape = new GradientTape(_function);
        Placeholder[] parameters = _function.Params;
        HashSet<string> identifierAsConstant = new HashSet<string>();
        foreach (Placeholder placeholder in parameters)
        {
            if (placeholder.Scalar != null)
            {
                identifierAsConstant.Add(placeholder.Identifier);
            }
            else
            {
                placeholder.Scalar = Scalar.Create(Random.Shared.NextDouble() + 1e-10);
            }
        }
        for (int i = 0; i < Epochs; i++)
        {
            tape.Compute();
            double maxDerivative = 0;
            foreach (Placeholder arg in parameters)
            {
                if (identifierAsConstant.Contains(arg.Identifier))
                {
                    continue;
                }
                double gradientArg = tape.Gradient(arg);
                arg.Scalar = Scalar.Create(arg.Scalar.Value.Value - gradientArg * _learningRate);
                _logger?.Debug("Epoch {Epoch}: gradient={Gradient}, identifier={Identifier}, value={Value}", i, gradientArg, arg.Identifier, arg.Scalar.Value.Value);
                maxDerivative = Math.Max(maxDerivative, Math.Abs(gradientArg));
            }
            if (maxDerivative < Threshold)
            {
                break;
            }
        }
        return parameters;
    }

    private Placeholder[] GradientDescentMomentum()
    {
        GradientTape tape = new GradientTape(_function);
        Placeholder[] parameters = _function.Params;
        HashSet<string> identifierAsConstant = new HashSet<string>();
        foreach (Placeholder placeholder in parameters)
        {
            if (placeholder.Scalar != null)
            {
                identifierAsConstant.Add(placeholder.Identifier);
            }
            else
            {
                placeholder.Scalar = Scalar.Create(Random.Shared.NextDouble() + 1e-10);
            }
        }
        Dictionary<string, double> velocities = parameters.ToDictionary(x => x.Identifier, _ => Threshold);
        for (int i = 0; i < Epochs; i++)
        {
            tape.Compute();
            double maxDerivative = 0;
            foreach (Placeholder arg in parameters)
            {
                if (identifierAsConstant.Contains(arg.Identifier))
                {
                    continue;
                }
                double gradientArg = tape.Gradient(arg);
                _logger?.Debug("Epoch {Epoch}: gradient={Gradient}, identifier={Identifier}, value={Value}", i, gradientArg, arg.Identifier, arg.Scalar.Value.Value);
                maxDerivative = Math.Max(maxDerivative, Math.Abs(gradientArg));
                if (Math.Abs(gradientArg) < Threshold)
                {
                    break;
                }
                if (Math.Abs(velocities[arg.Identifier]) < Threshold * 1e-5)
                {
                    break;
                }
                velocities[arg.Identifier] = _frictionVelocity * velocities[arg.Identifier] + gradientArg * _learningRate;
                arg.Scalar = Scalar.Create(arg.Scalar.Value.Value - velocities[arg.Identifier]);
            }
            if (maxDerivative < Threshold)
            {
                break;
            }
        }
        return parameters;
    }

    public enum GradienDescentAlgorithm
    {
        GradientDescent,
        GradientDescentMomentum
    }
}

public class TrainingEntry
{
    public double DependentVariable { get; set; }
    public (Placeholder, Scalar)[] IndependentVariables { get; set; }

    public static TrainingEntry Create(double dependentVariable, (Placeholder, Scalar)[] dependentVariables)
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
    // Reference equality on purpose: Placeholder is a record whose value-based hash includes
    // the mutable Scalar, so a value-hashed set would corrupt as training mutates values.
    private readonly HashSet<Placeholder> _placeholders = new HashSet<Placeholder>(ReferenceEqualityComparer.Instance);
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
