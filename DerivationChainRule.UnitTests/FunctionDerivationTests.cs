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

    [Fact]
    public void Derive_Sin_Function_Uses_Chain_Rule()
    {
        // g(x) = 2x, f(x) = sin(g(x)) = sin(2x); f'(x) = cos(2x) * 2
        Function x = Function.Create(_placeholders.Create("x"));
        Function g = x * Function.Create(Scalar.Create(2));
        Function f = Function.Sin(g);
        Function df = GetDerivative(f, _placeholders);

        for (int xx = 0; xx < 10; xx++)
        {
            _placeholders["x"] = Scalar.Create(xx);
            var expected = (decimal)(Math.Cos(2 * xx) * 2);
            var actual = df.Evaluate(_placeholders).Value;
            Assert.True(Math.Abs(expected - actual) < 0.0001m, $"Expected {expected}, got {actual}");
        }
    }

    [Fact]
    public void Derive_Exp_Function_Uses_Chain_Rule()
    {
        // g(x) = 3x, f(x) = exp(g(x)); f'(x) = exp(3x) * 3
        Function x = Function.Create(_placeholders.Create("x"));
        Function g = x * Function.Create(Scalar.Create(3));
        Function f = Function.Exp(g);
        Function df = GetDerivative(f, _placeholders);

        for (int xx = 0; xx < 5; xx++)
        {
            _placeholders["x"] = Scalar.Create(xx);
            var expected = (decimal)(Math.Exp(3 * xx) * 3);
            var actual = df.Evaluate(_placeholders).Value;
            Assert.True(Math.Abs(expected - actual) < 0.001m, $"Expected {expected}, got {actual}");
        }
    }

    [Fact]
    public void Derive_Ln_Function_Uses_Chain_Rule()
    {
        // g(x) = x + 1, f(x) = ln(g(x)); f'(x) = 1 / (x+1)
        Function x = Function.Create(_placeholders.Create("x"));
        Function g = x + Function.Create(Scalar.Create(1));
        Function f = Function.Ln(g);
        Function df = GetDerivative(f, _placeholders);

        for (int xx = 0; xx < 10; xx++)
        {
            _placeholders["x"] = Scalar.Create(xx);
            var expected = 1m / (xx + 1);
            var actual = df.Evaluate(_placeholders).Value;
            Assert.True(Math.Abs(expected - actual) < 0.0001m, $"Expected {expected}, got {actual}");
        }
    }

    [Fact]
    public void Derive_Cos_Function_Uses_Chain_Rule()
    {
        // g(x) = 4x, f(x) = cos(g(x)); f'(x) = -sin(4x) * 4
        Function x = Function.Create(_placeholders.Create("x"));
        Function g = x * Function.Create(Scalar.Create(4));
        Function f = Function.Cos(g);
        Function df = GetDerivative(f, _placeholders);

        for (int xx = 0; xx < 5; xx++)
        {
            _placeholders["x"] = Scalar.Create(xx);
            var expected = (decimal)(-Math.Sin(4 * xx) * 4);
            var actual = df.Evaluate(_placeholders).Value;
            Assert.True(Math.Abs(expected - actual) < 0.0001m, $"Expected {expected}, got {actual}");
        }
    }
}