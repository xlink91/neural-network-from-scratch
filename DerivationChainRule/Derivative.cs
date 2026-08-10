namespace DerivationChainRule;
using Placeholders = Placeholder.Placeholders;

public class Derivative
{
    private Placeholders _placeholders;
    private Function _function;
    public Derivative(Function function, Placeholders placeholders)
    {
        _placeholders = placeholders;
        _function = function;
    }
    public Function Derive()
    {
        return Derive(_function);
    }

    private static Function Derive(Function function)
    {
        switch (function.Operator)
        {
            case Function.Op.Add:
            case Function.Op.Subtract:
                return DeriveSum(function);
            case Function.Op.Multiply:
                return DeriveProduct(function);
            case Function.Op.Divide:
                return DeriveDivide(function);
            case Function.Op.PlaceHolder:
                return DeriveVariable();
            case Function.Op.Scalar:
                return DeriveConstant();
            default:
                throw new Exception("Unknown operator: " + function.Operator);
        }
    }

    private static Function DeriveDivide(Function function)
    {
        return (Derive(function.Left) * function.Right - function.Left * Derive(function.Right)) 
               / (function.Right * function.Right);
    }

    private static Function DeriveSum(Function function)
    {
        return function.Operator switch
        {
            Function.Op.Add => Derive(function.Left) + Derive(function.Right),
            Function.Op.Subtract => Derive(function.Left) - Derive(function.Right),
            _ => throw new Exception("Expected operator +/- receive instead " + function.Operator)
        };
    }

    private static Function DeriveProduct(Function function)
    {
        return Derive(function.Left) * function.Right + function.Left * Derive(function.Right);
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