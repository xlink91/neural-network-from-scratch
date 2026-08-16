namespace DerivationChainRule;

public record Function
{
    public Function? Left { get; private set; }
    public Function? Right { get; private set; }
    public Op Operator { get; private set; }
    public Placeholder Placeholder { get; private set; }
    private Placeholder[] _parameters;
    public Placeholder[] Params
    {
        get
        {
            return _parameters = _parameters ?? GetParams();
        }
    }
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
    public Scalar Evaluate()
    {
        ThrowIfDuplicateParameters();
        return Operator switch
        {
            Op.Add => Left.Evaluate() + Right.Evaluate(),
            Op.Subtract => Left.Evaluate() - Right.Evaluate(),
            Op.Multiply => Left.Evaluate() * Right.Evaluate(),
            Op.Divide => Left.Evaluate() / Right.Evaluate(),
            Op.PlaceHolder => Placeholder.Scalar,
            Op.Scalar => Scalar,
            Op.Sin => Scalar.Sin(Left.Evaluate()),
            Op.Cos => Scalar.Cos(Left.Evaluate()),
            Op.Exp => Scalar.Exp(Left.Evaluate()),
            Op.Ln => Scalar.Ln(Left.Evaluate()),
            _ => throw new Exception("Invalid operator")
        };
    }

    private void ThrowIfDuplicateParameters()
    {
        var duplicate = GetIndependentVariables(this)
            .GroupBy(p => p.Identifier)
            .FirstOrDefault(g => g.Distinct(ReferenceEqualityComparer.Instance).Count() > 1);
        if (duplicate != null)
        {
            throw new Exception(
                $"Parameter '{duplicate.Key}' has multiple distinct placeholder instances in this function tree.");
        }
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
        Ln
    }
    public override string ToString()
    {
        return "f = " + FunctionToString(this);
    }
    private string FunctionToString(Function function)
    {
        if (function.Operator == Op.Scalar)
        {
            return function.Scalar.Value.ToString();
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
        throw new Exception("Invalid operator");
    }

    private Placeholder[] GetParams()
    {
        return GetIndependentVariables(this).DistinctBy(p => p.Identifier).ToArray();
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

public record Scalar
{
    public decimal Value { get; private set; }
    public static Scalar Create(decimal number)
    {
        return new Scalar { Value = number };
    }
    public static Scalar operator +(Scalar left, Scalar right)
    {
        return new Scalar { Value = left.Value + right.Value };
    }
    public static Scalar operator -(Scalar left, Scalar right)
    {
        return new Scalar { Value = left.Value - right.Value };
    }
    public static Scalar operator *(Scalar left, Scalar right)
    {
        return new Scalar { Value = left.Value * right.Value };
    }
    public static Scalar operator /(Scalar left, Scalar right)
    {
        return new Scalar { Value = left.Value / right.Value };
    }
    public static implicit operator Scalar(decimal number)
    {
        return new Scalar { Value = number };
    }
    public static Scalar Zero => new Scalar(0);
    public static Scalar One => new Scalar(1);
    public static Scalar Sin(Scalar s)
    {
        return new Scalar { Value = (decimal)Math.Sin((double)s.Value) };
    }
    public static Scalar Cos(Scalar s)
    {
        return new Scalar { Value = (decimal)Math.Cos((double)s.Value) };
    }
    public static Scalar Exp(Scalar s)
    {
        return new Scalar { Value = (decimal)Math.Exp((double)s.Value) };
    }
    public static Scalar Ln(Scalar s)
    {
        return new Scalar { Value = (decimal)Math.Log((double)s.Value) };
    }
}

public sealed record Placeholder
{
    public string Identifier { get; private set; }
    public  Scalar Scalar { get; set; }
    
    protected Placeholder(string identifier)
    {
        Identifier = identifier;
    }
    public  static Placeholder Create(string identifier)
    {
        return new Placeholder(identifier);
    }
}