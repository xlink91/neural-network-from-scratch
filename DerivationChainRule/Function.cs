namespace DerivationChainRule;

public record Function
{
    public Function Left { get; private set; }
    public Function Right { get; private set; }
    public Op Operator { get; private set; }
    public Placeholder Placeholder { get; private set; }
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
    public static Function Create(Placeholder placeholder)
    {
        return new Function(placeholder);
    }
    public static Function Create(Scalar scalar)
    {
        return new Function(scalar);
    }
    public Scalar Evaluate(Placeholder.Placeholders placeholders)
    {
        return Operator switch
        {
            Op.Add => Left.Evaluate(placeholders) + Right.Evaluate(placeholders),
            Op.Subtract => Left.Evaluate(placeholders) - Right.Evaluate(placeholders),
            Op.Multiply => Left.Evaluate(placeholders) * Right.Evaluate(placeholders),
            Op.Divide => Left.Evaluate(placeholders) / Right.Evaluate(placeholders),
            Op.PlaceHolder => placeholders[Placeholder.Identifier],
            Op.Scalar => Scalar,
            _ => throw new Exception("Invalid operator")
        };
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
        Scalar
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
            return "(" + FunctionToString(function.Left) + " + " + FunctionToString(function.Right) + ")";
        }
        if (function.Operator == Op.Multiply)
        {
            return "(" + FunctionToString(function.Left) + " * " + FunctionToString(function.Right) + ")";
        }
        if (function.Operator == Op.Divide)
        {
            return "(" + FunctionToString(function.Left) + " / " + FunctionToString(function.Right) + ")";
        }
        throw new Exception("Invalid operator");
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
}

public sealed record Placeholder
{
    public string Identifier { get; private set; }
    public  Scalar Scalar { get; set; }
    
    protected Placeholder(string identifier)
    {
        Identifier = identifier;
    }
    private  static Placeholder Create(string identifier)
    {
        return new Placeholder(identifier);
    }
    public class Placeholders
    {
        private Dictionary<string, Placeholder> Functions { get; set; }
        public Placeholders()
        {
            Functions = new Dictionary<string, Placeholder>();
        }
        public Placeholder Create(string identifier)
        {
            var placeholder = new Placeholder(identifier);
            return CreatePlaceholderEntry(placeholder);
        }
        private Placeholder CreatePlaceholderEntry(Placeholder placeholder)
        {
            return Functions.TryAdd(placeholder.Identifier, placeholder) 
                ? placeholder 
                : throw new Exception("Placeholder entry already exists: " + placeholder.Identifier + ".");
        }

        private Placeholder Get(string identifier)
        {
            return !Functions.TryGetValue(identifier, out var function)
                ? throw new Exception("Placeholder entry does not exists: " + identifier)
                : function;
        }
        
        public Scalar this[string name]
        {
            get => Get(name).Scalar;
            set
            {
                Placeholder placeholder = Get(name);
                placeholder.Scalar = value;
            }
        }
    }
}