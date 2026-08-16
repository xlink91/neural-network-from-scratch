namespace DerivationChainRule.UnitTests;

public class MathUtilFunctionTranslationTests
{
    [Fact]
    public void Translate_SingleScalar_EvaluatesToScalar()
    {
        AssertValue("5", 5m);
    }

    [Fact]
    public void Translate_SingleVariable_EvaluatesToVariableValue()
    {
        AssertValue("x", 7m, ("x", 7m));
    }

    [Fact]
    public void Translate_Addition_EvaluatesSum()
    {
        AssertValue("2+3", 5m);
    }

    [Fact]
    public void Translate_Subtraction_EvaluatesDifference()
    {
        AssertValue("5-2", 3m);
    }

    [Fact]
    public void Translate_Multiplication_EvaluatesProduct()
    {
        AssertValue("2*3", 6m);
    }

    [Fact]
    public void Translate_Division_EvaluatesQuotient()
    {
        AssertValue("6/2", 3m);
    }

    [Fact]
    public void Translate_VariablePlusScalar_EvaluatesCorrectly()
    {
        AssertValue("x+1", 5m, ("x", 4m));
    }

    [Fact]
    public void Translate_MultiplyThenAdd_RespectsPrecedence()
    {
        AssertValue("2*3+4", 10m);
    }

    [Fact]
    public void Translate_AddThenMultiply_RespectsPrecedence()
    {
        AssertValue("2+3*4", 14m);
    }

    [Fact]
    public void Translate_DivideThenSubtract_RespectsPrecedence()
    {
        AssertValue("8/2-1", 3m);
    }

    [Fact]
    public void Translate_DivisionChain_IsLeftAssociative()
    {
        AssertValue("100/5/2", 10m);
    }

    [Fact]
    public void Translate_MultipleDifferentVariablesWithMixedOperators_EvaluatesCorrectly()
    {
        AssertValue("a+b*c+d", 24m, ("a", 2m), ("b", 3m), ("c", 5m), ("d", 7m));
    }

    [Fact]
    public void Translate_DivideMultiplyThenAdd_WithDifferentVariables_EvaluatesCorrectly()
    {
        AssertValue("a/b*c+d", 67m, ("a", 100m), ("b", 5m), ("c", 3m), ("d", 7m));
    }

    [Fact]
    public void Translate_MultiplyChainThenAdd_WithDifferentVariables_EvaluatesCorrectly()
    {
        AssertValue("x*y*z+w", 37m, ("x", 2m), ("y", 3m), ("z", 5m), ("w", 7m));
    }

    // "10-3-2" is left-associative: (10-3)-2 = 5, not 10-(3-2) = 9.
    [Fact]
    public void Translate_SubtractionChain_IsLeftAssociative()
    {
        AssertValue("10-3-2", 5m);
    }

    [Fact]
    public void Translate_ImplicitMultiplication_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => MathUtil.Translate("2x"));
    }

    private static void AssertValue(string input, decimal expected, params (string Name, decimal Value)[] variables)
    {
        Function function = MathUtil.Translate(input);
        foreach (var (name, value) in variables)
        {
            function.Params.First(p => p.Identifier == name).Scalar = Scalar.Create(value);
        }
        decimal result = function.Evaluate().Value;
        Assert.Equal(expected, result);
    }
}
