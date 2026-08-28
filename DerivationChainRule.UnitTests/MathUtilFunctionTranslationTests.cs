
namespace DerivationChainRule.UnitTests;

public class MathUtilFunctionTranslationTests
{
    [Fact]
    public void Translate_SingleScalar_EvaluatesToScalar()
    {
        AssertValue("5", 5);
    }

    [Fact]
    public void Translate_SingleVariable_EvaluatesToVariableValue()
    {
        AssertValue("x", 7, ("x", 7));
    }

    [Fact]
    public void Translate_Addition_EvaluatesSum()
    {
        AssertValue("2+3", 5);
    }

    [Fact]
    public void Translate_Subtraction_EvaluatesDifference()
    {
        AssertValue("5-2", 3);
    }

    [Fact]
    public void Translate_Multiplication_EvaluatesProduct()
    {
        AssertValue("2*3", 6);
    }

    [Fact]
    public void Translate_Division_EvaluatesQuotient()
    {
        AssertValue("6/2", 3);
    }

    [Fact]
    public void Translate_VariablePlusScalar_EvaluatesCorrectly()
    {
        AssertValue("x+1", 5, ("x", 4));
    }

    [Fact]
    public void Translate_MultiplyThenAdd_RespectsPrecedence()
    {
        AssertValue("2*3+4", 10);
    }

    [Fact]
    public void Translate_AddThenMultiply_RespectsPrecedence()
    {
        AssertValue("2+3*4", 14);
    }

    [Fact]
    public void Translate_DivideThenSubtract_RespectsPrecedence()
    {
        AssertValue("8/2-1", 3);
    }

    [Fact]
    public void Translate_DivisionChain_IsLeftAssociative()
    {
        AssertValue("100/5/2", 10);
    }

    [Fact]
    public void Translate_MultipleDifferentVariablesWithMixedOperators_EvaluatesCorrectly()
    {
        AssertValue("a+b*c+d", 24, ("a", 2), ("b", 3), ("c", 5), ("d", 7));
    }

    [Fact]
    public void Translate_DivideMultiplyThenAdd_WithDifferentVariables_EvaluatesCorrectly()
    {
        AssertValue("a/b*c+d", 67, ("a", 100), ("b", 5), ("c", 3), ("d", 7));
    }

    [Fact]
    public void Translate_MultiplyChainThenAdd_WithDifferentVariables_EvaluatesCorrectly()
    {
        AssertValue("x*y*z+w", 37, ("x", 2), ("y", 3), ("z", 5), ("w", 7));
    }

    // "10-3-2" is left-associative: (10-3)-2 = 5, not 10-(3-2) = 9.
    [Fact]
    public void Translate_SubtractionChain_IsLeftAssociative()
    {
        AssertValue("10-3-2", 5);
    }

    [Fact]
    public void Translate_ImplicitMultiplication_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => MathUtil.Translate("2x"));
    }

    [Fact]
    public void Translate_Tanh_EvaluatesCorrectly()
    {
        AssertValue("tanh(x)", Math.Tanh(0.5), ("x", 0.5));
    }

    // A variable inside a tanh call and the same variable outside it must share one
    // placeholder, so the function has a single "x" parameter.
    [Fact]
    public void Translate_TanhArgument_SharesPlaceholderWithOutside()
    {
        Function function = MathUtil.Translate("tanh(x)+x");

        var xParam = Assert.Single(function.Params);
        xParam.Scalar = Scalar.Create(2);
        Assert.Equal(Math.Tanh(2) + 2, function.Evaluate().Value, 9);
    }

    // The neuron shape from the training tests: spaces and alphanumeric identifiers.
    [Fact]
    public void Translate_TanhNeuron_EvaluatesCorrectly()
    {
        AssertValue("tanh(x1*w1 + x2*w2 + b)",
            Math.Tanh(0.5 * 0.8 + -1.5 * 0.3 + 0.1),
            ("x1", 0.5), ("w1", 0.8), ("x2", -1.5), ("w2", 0.3), ("b", 0.1));
    }

    private static void AssertValue(string input, double expected, params (string Name, double Value)[] variables)
    {
        Function function = MathUtil.Translate(input);
        foreach (var (name, value) in variables)
        {
            function.Params.First(p => p.Identifier == name).Scalar = Scalar.Create(value);
        }
        double result = function.Evaluate().Value;
        Assert.Equal(expected, result);
    }
}
