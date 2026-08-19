namespace DerivationChainRule.UnitTests;

public class FunctionDerivationTests
{
    [Fact]
    public void Derive_Constant()
    {
        Function constant = Function.Create(Scalar.Create(5));
        Assert.Equal(Scalar.Zero, GetDerivative(constant, Placeholder.Create("x")).Evaluate());
    }

    [Fact]
    public void Derive_LinewithSlope_One()
    {
        var xPlaceholder = Placeholder.Create("x");
        Function line = Function.Create(xPlaceholder);
        Function derivative = GetDerivative(line, xPlaceholder);
        for (int i = 0; i < 100; i++)
        {
            xPlaceholder.Scalar = Scalar.Create(i);
            Assert.Equal(Scalar.One, derivative.Evaluate());
        }
    }

    [Fact]
    public void Derive_LinewithSlope()
    {
        var xPlaceholder = Placeholder.Create("x");
        Function x  = Function.Create(xPlaceholder);
        Function m  = Function.Create(Scalar.Create(2));
        Function c =  Function.Create(Scalar.Create(3));
        Function y = x * m + c;
        var dy = GetDerivative(y, xPlaceholder);
        for (int xx = 1; xx < 100; xx++)
        {
            xPlaceholder.Scalar = Scalar.Create(xx);
            Assert.Equal(Scalar.Create(2), dy.Evaluate());
        }
    }

    [Fact]
    public void Derive_Parabolic_Function_With_Vertex_Moved()
    {
        var xPlaceholder = Placeholder.Create("x");
        Function x = Function.Create(xPlaceholder);
        Function a = Function.Create(Scalar.Create(2));
        Function b = Function.Create(Scalar.Create(3));
        Function c = Function.Create(Scalar.Create(5));
        Function y = a * x * x + b * x + c;
        var dy = GetDerivative(y, xPlaceholder);
        for (int xx = 1; xx < 100; xx++)
        {
            xPlaceholder.Scalar = Scalar.Create(xx);
            Assert.Equal(Scalar.Create(4 * xx + 3), dy.Evaluate());
        }
    }

    private Function GetDerivative(Function function, Placeholder placeholder)
    {
        return new Derivative(function).Derive(placeholder);
    }

    [Fact]
    public void Derive_Sin_Function_Uses_Chain_Rule()
    {
        // g(x) = 2x, f(x) = sin(g(x)) = sin(2x); f'(x) = cos(2x) * 2
        var xPlaceholder = Placeholder.Create("x");
        Function x = Function.Create(xPlaceholder);
        Function g = x * Function.Create(Scalar.Create(2));
        Function f = Function.Sin(g);
        Function df = GetDerivative(f, xPlaceholder);

        for (int xx = 0; xx < 10; xx++)
        {
            xPlaceholder.Scalar = Scalar.Create(xx);
            var expected = Math.Cos(2 * xx) * 2;
            var actual = df.Evaluate().Value;
            Assert.True(Math.Abs(expected - actual) < 0.0001, $"Expected {expected}, got {actual}");
        }
    }

    [Fact]
    public void Derive_Exp_Function_Uses_Chain_Rule()
    {
        // g(x) = 3x, f(x) = exp(g(x)); f'(x) = exp(3x) * 3
        var xPlaceholder = Placeholder.Create("x");
        Function x = Function.Create(xPlaceholder);
        Function g = x * Function.Create(Scalar.Create(3));
        Function f = Function.Exp(g);
        Function df = GetDerivative(f, xPlaceholder);

        for (int xx = 0; xx < 5; xx++)
        {
            xPlaceholder.Scalar = Scalar.Create(xx);
            var expected = Math.Exp(3 * xx) * 3;
            var actual = df.Evaluate().Value;
            Assert.True(Math.Abs(expected - actual) < 0.001, $"Expected {expected}, got {actual}");
        }
    }

    [Fact]
    public void Derive_Ln_Function_Uses_Chain_Rule()
    {
        // g(x) = x + 1, f(x) = ln(g(x)); f'(x) = 1 / (x+1)
        var xPlaceholder = Placeholder.Create("x");
        Function x = Function.Create(xPlaceholder);
        Function g = x + Function.Create(Scalar.Create(1));
        Function f = Function.Ln(g);
        Function df = GetDerivative(f, xPlaceholder);

        for (int xx = 0; xx < 10; xx++)
        {
            xPlaceholder.Scalar = Scalar.Create(xx);
            var expected = 1.0 / (xx + 1);
            var actual = df.Evaluate().Value;
            Assert.True(Math.Abs(expected - actual) < 0.0001, $"Expected {expected}, got {actual}");
        }
    }

    [Fact]
    public void Derive_Cos_Function_Uses_Chain_Rule()
    {
        // g(x) = 4x, f(x) = cos(g(x)); f'(x) = -sin(4x) * 4
        var xPlaceholder = Placeholder.Create("x");
        Function x = Function.Create(xPlaceholder);
        Function g = x * Function.Create(Scalar.Create(4));
        Function f = Function.Cos(g);
        Function df = GetDerivative(f, xPlaceholder);

        for (int xx = 0; xx < 5; xx++)
        {
            xPlaceholder.Scalar = Scalar.Create(xx);
            var expected = -Math.Sin(4 * xx) * 4;
            var actual = df.Evaluate().Value;
            Assert.True(Math.Abs(expected - actual) < 0.0001, $"Expected {expected}, got {actual}");
        }
    }
}
