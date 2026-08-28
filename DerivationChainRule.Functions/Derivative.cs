namespace DerivationChainRule;

public class Derivative
{
    private Function _function;
    public Derivative(Function function)
    {
        _function = function;
    }
    public Function Derive(Placeholder placeholder)
    {
        return Derive(_function, placeholder);
    }

    private static Function Derive(Function function, Placeholder placeholder)
    {
        switch (function.Operator)
        {
            case Function.Op.Add:
            case Function.Op.Subtract:
                return DeriveSum(function, placeholder);
            case Function.Op.Multiply:
                return DeriveProduct(function, placeholder);
            case Function.Op.Divide:
                return DeriveDivide(function, placeholder);
            case Function.Op.PlaceHolder when placeholder == function.Placeholder:
                return DeriveVariable();
            case Function.Op.PlaceHolder when placeholder != function.Placeholder:
            case Function.Op.Scalar:
                return DeriveConstant();
            case Function.Op.Sin:
            case Function.Op.Cos:
            case Function.Op.Exp:
            case Function.Op.Ln:
            case Function.Op.Tanh:
                return DeriveUnary(function, placeholder);
            default:
                throw new Exception("Unknown operator: " + function.Operator);
        }
    }

    private static Function DeriveUnary(Function function, Placeholder placeholder)
    {
        return function.Operator switch
        {
            // chain rule: (f(g(x)))' = f'(g(x)) * g'(x)
            Function.Op.Sin => Function.Cos(function.Left) * Derive(function.Left, placeholder),
            Function.Op.Cos => (Function.Create(Scalar.Zero) - Function.Sin(function.Left)) * Derive(function.Left, placeholder),
            Function.Op.Exp => Function.Exp(function.Left) * Derive(function.Left, placeholder),
            Function.Op.Ln => Derive(function.Left, placeholder) / function.Left,
            Function.Op.Tanh => (Function.Create(Scalar.One) - Function.Tanh(function.Left) * Function.Tanh(function.Left)) * Derive(function.Left, placeholder),
            _ => throw new Exception("Expected unary operator, received " + function.Operator)
        };
    }

    private static Function DeriveDivide(Function function, Placeholder placeholder)
    {
        return (Derive(function.Left, placeholder) * function.Right - function.Left * Derive(function.Right, placeholder)) 
               / (function.Right * function.Right);
    }

    private static Function DeriveSum(Function function, Placeholder placeholder)
    {
        return function.Operator switch
        {
            Function.Op.Add => Derive(function.Left, placeholder) + Derive(function.Right, placeholder),
            Function.Op.Subtract => Derive(function.Left, placeholder) - Derive(function.Right, placeholder),
            _ => throw new Exception("Expected operator +/- receive instead " + function.Operator)
        };
    }

    private static Function DeriveProduct(Function function, Placeholder placeholder)
    {
        return Derive(function.Left, placeholder) * function.Right + function.Left * Derive(function.Right, placeholder);
    }
    private static Function DeriveConstant()
    {
        return Function.Create(Scalar.Create(0));
    }
    private static Function DeriveVariable()
    {
        return Function.Create(Scalar.Create(1));
    }
}