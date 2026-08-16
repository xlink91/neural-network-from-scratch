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
}