namespace DerivationChainRule.UnitTests;

public class FunctionEvaluateTests
{
    // If the same identifier is backed by two distinct Placeholder instances, setting one
    // wouldn't affect the other, which would silently evaluate with a stale/unset value.
    // Evaluate must refuse to do that instead of guessing which instance is authoritative.
    [Fact]
    public void Evaluate_DuplicatePlaceholderInstancesForSameIdentifier_ThrowsDescriptiveException()
    {
        var x1 = Function.Create(Placeholder.Create("x"));
        var x2 = Function.Create(Placeholder.Create("x"));
        var y = x1 * x2;

        var exception = Assert.Throws<Exception>(() => y.Evaluate());
        Assert.Contains("x", exception.Message);
    }

    // Regression test for the MathUtil.ExpToFunction fix: a repeated variable parsed from a
    // string must resolve to a single shared Placeholder, so setting it via Params affects
    // every occurrence in the tree.
    [Fact]
    public void Translate_RepeatedVariable_SharesOnePlaceholderAndEvaluatesCorrectly()
    {
        Function function = MathUtil.Translate("x*x");

        var xParam = Assert.Single(function.Params);
        Assert.Equal("x", xParam.Identifier);

        xParam.Scalar = Scalar.Create(4m);

        Assert.Equal(16m, function.Evaluate().Value);
    }
}
