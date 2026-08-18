
namespace DerivationChainRule.UnitTests;

public class MultivariateDerivate
{
    [Theory]
    [InlineData("1+1", "(1+1)")]
    [InlineData("2*x*x+5*x+3", "((((2*x)*x)+(5*x))+3)")]
    public void CheckExpressionTreeConstruction(string expression, string expected)
    {
        var exp = expression.ToExp();
        Assert.Equal(expected, exp.ToString());
    }

    // expression is parsed via MathUtil.Translate; x/y/a/b/c supply values for whichever of
    // those identifiers actually appear in it (unused slots are ignored); differentiateWith
    // picks which one the partial derivative is taken against; expected is the derivative's
    // value at that point.
    [Theory]
    [InlineData("5", "x", 0, 0, 0, 0, 0, 0)] // constant: derivative is 0 regardless of x
    [InlineData("x", "x", 5, 0, 0, 0, 0, 1)] // identity: derivative is 1
    [InlineData("2*x+3", "x", 10, 0, 0, 0, 0, 2)] // linear: slope 2
    [InlineData("x*x", "x", 3, 0, 0, 0, 0, 6)] // quadratic: d/dx(x^2) = 2x
    [InlineData("x*x*x", "x", 2, 0, 0, 0, 0, 12)] // cubic via left-assoc (x*x)*x: d/dx(x^3) = 3x^2
    [InlineData("a*x+b", "x", 0, 0, 5, 7, 0, 5)] // multivariate linear, d/dx(a*x+b) = a
    [InlineData("a*x+b", "a", 4, 0, 5, 7, 0, 4)] // same function, d/da(a*x+b) = x
    [InlineData("a*x*x+b*x+c", "x", 4, 0, 2, 3, 5, 19)] // d/dx(a*x^2+b*x+c) = 2ax+b = 2*2*4+3
    [InlineData("x*x+y*y", "x", 3, 100, 0, 0, 0, 6)] // two independent variables, d/dx = 2x
    [InlineData("x*x+y*y", "y", 100, 5, 0, 0, 0, 10)] // same function, d/dy = 2y
    [InlineData("x*y", "x", 0, 7, 0, 0, 0, 7)] // product of two variables, d/dx(x*y) = y
    [InlineData("x*y", "y", 9, 0, 0, 0, 0, 9)] // same function, d/dy(x*y) = x
    [InlineData("x/a", "x", 0, 0, 2, 0, 0, 0.5)] // quotient rule: d/dx(x/a) = 1/a
    [InlineData("a*x*x+b*x*y+c*y*y", "x", 5, 6, 2, 3, 4, 38)] // full quadratic form, d/dx = 2ax+by
    [InlineData("a*x*x+b*x*y+c*y*y", "y", 5, 6, 2, 3, 4, 63)] // same function, d/dy = bx+2cy
    public void Derivative_OfStringDefinedFunction_EvaluatesCorrectly(
        string expression, string differentiateWith,
        int x, int y, int a, int b, int c,
        double expected)
    {
        Function function = MathUtil.Translate(expression);
        SetIfPresent(function, "x", x);
        SetIfPresent(function, "y", y);
        SetIfPresent(function, "a", a);
        SetIfPresent(function, "b", b);
        SetIfPresent(function, "c", c);

        Placeholder target = function.Params.FirstOrDefault(p => p.Identifier == differentiateWith)
            ?? Placeholder.Create(differentiateWith);
        Function derivative = new Derivative(function).Derive(target);

        Assert.Equal((decimal)expected, derivative.Evaluate().Value);
    }

    private static void SetIfPresent(Function function, string identifier, decimal value)
    {
        var placeholder = function.Params.FirstOrDefault(p => p.Identifier == identifier);
        if (placeholder != null)
        {
            placeholder.Scalar = Scalar.Create(value);
        }
    }
}
