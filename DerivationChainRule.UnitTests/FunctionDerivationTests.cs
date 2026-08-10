namespace DerivationChainRule.UnitTests;
using Placeholders = Placeholder.Placeholders;

public class FunctionDerivationTests
{
    private Placeholders _placeholders = new Placeholders();
    [Fact]
    public void Derive_Constant()
    {
        Function constant = Function.Create(Scalar.Create(5));
        Assert.Equal(Scalar.Zero, GetDerivative(constant, _placeholders).Evaluate(_placeholders));
    }

    [Fact]
    public void Derive_LinewithSlope_One()
    {
        Function line = Function.Create(_placeholders.Create("x"));
        Function derivative = GetDerivative(line, _placeholders);
        for (int i = 0; i < 100; i++)
        {
            _placeholders["x"] = Scalar.Create(i);
            Assert.Equal(Scalar.One, derivative.Evaluate(_placeholders));
        }
    }

    [Fact]
    public void Derive_LinewithSlope()
    {
        Function x  = Function.Create(_placeholders.Create("x"));
        Function m  = Function.Create(Scalar.Create(2));
        Function c =  Function.Create(Scalar.Create(3));
        Function y = x * m + c;
        var dy = GetDerivative(y, _placeholders);
        for (int xx = 1; xx < 100; xx++)
        {
            _placeholders["x"] = Scalar.Create(xx);
            Assert.Equal(Scalar.Create(2), dy.Evaluate(_placeholders));
        }
    }

    [Fact]
    public void Derive_Parabolic_Function_With_Vertex_Moved()
    {
        Function x = Function.Create(_placeholders.Create("x"));
        Function a = Function.Create(Scalar.Create(2));
        Function b = Function.Create(Scalar.Create(3));
        Function c = Function.Create(Scalar.Create(5));
        Function y = a * x * x + b * x + c;
        var dy = GetDerivative(y, _placeholders);
        for (int xx = 1; xx < 100; xx++)
        {
            _placeholders["x"] = Scalar.Create(xx);
            Assert.Equal(Scalar.Create(4 * xx + 3), dy.Evaluate(_placeholders));
        }
    }

    private Function GetDerivative(Function function, Placeholders placeholders)
    {
        return new Derivative(function, placeholders).Derive();
    }
}