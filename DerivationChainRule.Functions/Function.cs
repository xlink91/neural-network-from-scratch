using System.Globalization;

namespace DerivationChainRule;

public record Function
{
    public Function? Left { get; private set; }
    public Function? Right { get; private set; }
    public Op Operator { get; private set; }
    public Placeholder Placeholder { get; private set; }
    private Placeholder[] _parameters;
    private string? _duplicateParameterIdentifier;
    public Placeholder[] Params
    {
        get
        {
            return _parameters = _parameters ?? GetParams();
        }
    }
    public static Function W => Create(Placeholder.W);
    public static Function B => Create(Placeholder.B);
    public static Function X => Create(Placeholder.X);
    public Scalar Scalar { get; private set; }
    protected Function(Op op, Function left, Function right)
    {
        this.Operator = op;
        this.Left = left;
        this.Right = right;
    }
    protected Function(Placeholder placeholder)
    {
        Operator = Op.PlaceHolder;
        Placeholder = placeholder;
    }
    protected Function(Scalar scalar)
    {
        Operator = Op.Scalar;
        Scalar = scalar;
    }
    protected Function(Op op, Function inner)
    {
        Operator = op;
        Left = inner;
    }
    public static Function Create(Placeholder placeholder)
    {
        return new Function(placeholder);
    }
    public static Function Create(Scalar scalar)
    {
        return new Function(scalar);
    }
    public static Function Sin(Function inner)
    {
        return new Function(Op.Sin, inner);
    }
    public static Function Cos(Function inner)
    {
        return new Function(Op.Cos, inner);
    }
    public static Function Exp(Function inner)
    {
        return new Function(Op.Exp, inner);
    }
    public static Function Ln(Function inner)
    {
        return new Function(Op.Ln, inner);
    }
    public static Function Tanh(Function inner)
    {
        return new Function(Op.Tanh, inner);
    }
    public Scalar Evaluate()
    {
        ThrowIfDuplicateParameters();
        return EvaluateCore();
    }

    // Recursion goes through this method so the duplicate-parameter validation runs once at
    // the root instead of once per node (which made evaluation O(n^2)).
    private Scalar EvaluateCore()
    {
        return Operator switch
        {
            Op.Add => Left.EvaluateCore() + Right.EvaluateCore(),
            Op.Subtract => Left.EvaluateCore() - Right.EvaluateCore(),
            Op.Multiply => Left.EvaluateCore() * Right.EvaluateCore(),
            Op.Divide => Left.EvaluateCore() / Right.EvaluateCore(),
            Op.PlaceHolder => Placeholder.Scalar ?? throw new Exception(
                $"Parameter '{Placeholder.Identifier}' has no value assigned."),
            Op.Scalar => Scalar,
            Op.Sin => Scalar.Sin(Left.EvaluateCore()),
            Op.Cos => Scalar.Cos(Left.EvaluateCore()),
            Op.Exp => Scalar.Exp(Left.EvaluateCore()),
            Op.Ln => Scalar.Ln(Left.EvaluateCore()),
            Op.Tanh => Scalar.Tanh(Left.EvaluateCore()),
            _ => throw new Exception("Invalid operator")
        };
    }

    // The duplicate check is computed once during the Params walk and cached; the tree
    // structure is immutable after construction, so the cached result stays valid.
    internal void ThrowIfDuplicateParameters()
    {
        _ = Params;
        if (_duplicateParameterIdentifier != null)
        {
            throw new Exception(
                $"Parameter '{_duplicateParameterIdentifier}' has multiple distinct placeholder instances in this function tree.");
        }
    }

    // Reverse-mode differentiation: one forward + one backward pass over the tree yields the
    // value and ALL partial derivatives at once. For repeated gradient computations (training
    // loops) construct a GradientTape directly and reuse it instead of calling this.
    public (double Value, Dictionary<Placeholder, double> Gradients) Backpropagate()
    {
        GradientTape tape = new GradientTape(this);
        double value = tape.Compute();
        Dictionary<Placeholder, double> gradients = new Dictionary<Placeholder, double>(ReferenceEqualityComparer.Instance);
        foreach (Placeholder parameter in Params)
        {
            gradients[parameter] = tape.Gradient(parameter);
        }
        return (value, gradients);
    }
    public static Function operator +(Function left, Function right)
    {
        return new Function(Op.Add, left, right);
    }
    public static Function operator -(Function left, Function right)
    {
        return new Function(Op.Subtract, left, right);
    }
    public static Function operator *(Function left, Function right)
    {
        return new Function(Op.Multiply, left, right);
    }
    public static Function operator /(Function left, Function right)
    {
        return new Function(Op.Divide, left, right);
    }
    public enum Op
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        PlaceHolder,
        Scalar,
        Sin,
        Cos,
        Exp,
        Ln,
        Tanh
    }
    public override string ToString()
    {
        return "f = " + FunctionToString(this);
    }
    private string FunctionToString(Function function)
    {
        if (function.Operator == Op.Scalar)
        {
            return function.Scalar.Value.ToString(CultureInfo.InvariantCulture);
        }
        if(function.Operator == Op.PlaceHolder)
        {
            return function.Placeholder.Identifier;
        }
        if(function.Operator == Op.Add)
        {
            return "(" + FunctionToString(function.Left) + " + " + FunctionToString(function.Right) + ")";
        }
        if (function.Operator == Op.Subtract)
        {
            return "(" + FunctionToString(function.Left) + " - " + FunctionToString(function.Right) + ")";
        }
        if (function.Operator == Op.Multiply)
        {
            return "(" + FunctionToString(function.Left) + " * " + FunctionToString(function.Right) + ")";
        }
        if (function.Operator == Op.Divide)
        {
            return "(" + FunctionToString(function.Left) + " / " + FunctionToString(function.Right) + ")";
        }
        if (function.Operator == Op.Sin)
        {
            return "sin(" + FunctionToString(function.Left) + ")";
        }
        if (function.Operator == Op.Cos)
        {
            return "cos(" + FunctionToString(function.Left) + ")";
        }
        if (function.Operator == Op.Exp)
        {
            return "exp(" + FunctionToString(function.Left) + ")";
        }
        if (function.Operator == Op.Ln)
        {
            return "ln(" + FunctionToString(function.Left) + ")";
        }
        if (function.Operator == Op.Tanh)
        {
            return "tanh(" + FunctionToString(function.Left) + ")";
        }
        throw new Exception("Invalid operator");
    }

    private Placeholder[] GetParams()
    {
        Dictionary<string, Placeholder> distinct = new Dictionary<string, Placeholder>();
        foreach (Placeholder placeholder in GetIndependentVariables(this))
        {
            if (distinct.TryGetValue(placeholder.Identifier, out var existing))
            {
                if (!ReferenceEquals(existing, placeholder))
                {
                    _duplicateParameterIdentifier ??= placeholder.Identifier;
                }
            }
            else
            {
                distinct[placeholder.Identifier] = placeholder;
            }
        }
        return distinct.Values.ToArray();
    }

    private IEnumerable<Placeholder> GetIndependentVariables(Function function)
    {
        Queue<Function> functionsQueue = new Queue<Function>();
        functionsQueue.Enqueue(function);
        while (functionsQueue.Count != 0)
        {
            Function topFunction = functionsQueue.Dequeue();
            if(topFunction.Operator == Op.PlaceHolder)
            {
                yield return topFunction.Placeholder;
            }
            if(topFunction.Left != null)
                functionsQueue.Enqueue(topFunction.Left);
            if(topFunction.Right != null)
                functionsQueue.Enqueue(topFunction.Right);
        }
    }
}

public readonly record struct Scalar(double Value)
{
    public static Scalar Create(double number)
    {
        return new Scalar(number);
    }
    public static Scalar operator +(Scalar left, Scalar right)
    {
        return new Scalar(left.Value + right.Value);
    }
    public static Scalar operator -(Scalar left, Scalar right)
    {
        return new Scalar(left.Value - right.Value);
    }
    public static Scalar operator *(Scalar left, Scalar right)
    {
        return new Scalar(left.Value * right.Value);
    }
    public static Scalar operator /(Scalar left, Scalar right)
    {
        return new Scalar(left.Value / right.Value);
    }
    public static implicit operator Scalar(double number)
    {
        return new Scalar(number);
    }
    public static Scalar Zero => new Scalar(0);
    public static Scalar One => new Scalar(1);
    public static Scalar Sin(Scalar s)
    {
        return new Scalar(Math.Sin(s.Value));
    }
    public static Scalar Cos(Scalar s)
    {
        return new Scalar(Math.Cos(s.Value));
    }
    public static Scalar Exp(Scalar s)
    {
        return new Scalar(Math.Exp(s.Value));
    }
    public static Scalar Ln(Scalar s)
    {
        return new Scalar(Math.Log(s.Value));
    }
    public static Scalar Tanh(Scalar s)
    {
        return new Scalar(Math.Tanh(s.Value));
    }
}

public sealed record Placeholder
{
    public string Identifier { get; private set; }
    public Scalar? Scalar { get; set; }
    private static readonly object _lock = new object();
    private static int _idx = 0;

    // Auto-minted parameters get unique identifiers (w_1, x_2, ...) so a network can create
    // many independent weights/inputs/biases; explicitly-named placeholders keep their exact
    // identifier so lookups by name (Params, parsed variables) still work.
    public static Placeholder W => CreateUnique("w");
    public static Placeholder X => CreateUnique("x");
    public static Placeholder B => CreateUnique("b");

    private Placeholder(string identifier)
    {
        Identifier = identifier;
    }
    public  static Placeholder Create(string identifier)
    {
        return new Placeholder(identifier);
    }
    private static Placeholder CreateUnique(string prefix)
    {
        lock (_lock)
        {
            return new Placeholder(string.Format("{0}_{1}", prefix, ++_idx));
        }
    }
}
