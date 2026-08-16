namespace DerivationChainRule.UnitTests;

public class FunctionGetParamsTests
{
    [Fact]
    public void GetParams_ConstantFunction_ReturnsEmptyArray()
    {
        var constant = Function.Create(Scalar.Create(5));

        Assert.Empty(constant.Params);
    }

    [Fact]
    public void GetParams_SingleVariable_ReturnsThatVariable()
    {
        var x = Function.Create(Placeholder.Create("x"));

        AssertIdentifiers(x.Params, "x");
    }

    [Fact]
    public void GetParams_MultipleDifferentVariables_ReturnsAllOfThem()
    {
        // y = a*x*x + b*x + c
        var a = Function.Create(Placeholder.Create("a"));
        var b = Function.Create(Placeholder.Create("b"));
        var c = Function.Create(Placeholder.Create("c"));
        var x1 = Function.Create(Placeholder.Create("x"));
        var x2 = Function.Create(Placeholder.Create("x"));
        var x3 = Function.Create(Placeholder.Create("x"));
        var y = a * x1 * x2 + b * x3 + c;

        AssertIdentifiers(y.Params, "a", "b", "c", "x");
    }

    // The same identifier can appear as several independently-created Placeholder
    // instances (e.g. one per occurrence in a parsed expression), so GetParams must
    // dedupe by identifier, not by object reference.
    [Fact]
    public void GetParams_RepeatedVariable_ReturnsDistinctOnce()
    {
        var x1 = Function.Create(Placeholder.Create("x"));
        var x2 = Function.Create(Placeholder.Create("x"));
        var y = x1 * x2;

        AssertIdentifiers(y.Params, "x");
    }

    [Fact]
    public void GetParams_VariableInsideUnaryFunction_FindsIt()
    {
        var x = Function.Create(Placeholder.Create("x"));
        var y = Function.Sin(x);

        AssertIdentifiers(y.Params, "x");
    }

    // "if they exist, if not resolve them": the first call computes and caches into
    // Params; the second call must return that same cached array, not recompute it.
    [Fact]
    public void GetParams_CalledTwice_ReturnsSameCachedArrayInstance()
    {
        var x = Function.Create(Placeholder.Create("x"));

        var first = x.Params;
        var second = x.Params;

        Assert.Same(first, second);
    }

    private static void AssertIdentifiers(Placeholder[] actual, params string[] expectedIdentifiers)
    {
        Assert.Equal(
            expectedIdentifiers.OrderBy(id => id),
            actual.Select(p => p.Identifier).OrderBy(id => id));
    }
}
