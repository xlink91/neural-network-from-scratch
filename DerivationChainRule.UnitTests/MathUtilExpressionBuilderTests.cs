
namespace DerivationChainRule.UnitTests;

public class MathUtilExpressionBuilderTests
{
    [Fact]
    public void ToExp_SingleDigitScalar_ReturnsScalarExp()
    {
        AssertToExp("5", "5");
    }

    [Fact]
    public void ToExp_MultiDigitScalar_ReturnsScalarExp()
    {
        AssertToExp("42", "42");
    }

    [Fact]
    public void ToExp_SingleCharVariable_ReturnsVariableExp()
    {
        AssertToExp("x", "x");
    }

    [Fact]
    public void ToExp_MultiCharVariable_ReturnsVariableExp()
    {
        AssertToExp("abc", "abc");
    }

    [Fact]
    public void ToExp_Addition_BuildsPlusBinaryExp()
    {
        AssertToExp("2+3", "(2+3)");
    }

    [Fact]
    public void ToExp_Subtraction_BuildsMinusBinaryExp()
    {
        AssertToExp("5-2", "(5-2)");
    }

    [Fact]
    public void ToExp_Multiplication_BuildsMultiplyBinaryExp()
    {
        AssertToExp("2*3", "(2*3)");
    }

    [Fact]
    public void ToExp_Division_BuildsDivideBinaryExp()
    {
        AssertToExp("6/2", "(6/2)");
    }

    [Fact]
    public void ToExp_VariablePlusScalar_BuildsPlusBinaryExp()
    {
        AssertToExp("x+1", "(x+1)");
    }

    [Fact]
    public void ToExp_ScalarTimesVariable_BuildsMultiplyBinaryExp()
    {
        AssertToExp("2*x", "(2*x)");
    }

    // Multiplication binds tighter than addition, so "2*3+4" must mean (2*3)+4, not 2*(3+4).
    [Fact]
    public void ToExp_MultiplyThenAdd_MultiplicationBindsTighter()
    {
        AssertToExp("2*3+4", "((2*3)+4)");
    }

    // Multiplication binds tighter than addition, so "2+3*4" must mean 2+(3*4), not (2+3)*4.
    [Fact]
    public void ToExp_AddThenMultiply_MultiplicationBindsTighter()
    {
        AssertToExp("2+3*4", "(2+(3*4))");
    }

    // Division binds tighter than subtraction, so "8/2-1" must mean (8/2)-1, not 8/(2-1).
    [Fact]
    public void ToExp_DivideThenSubtract_DivisionBindsTighter()
    {
        AssertToExp("8/2-1", "((8/2)-1)");
    }

    // A division chain must preserve operand order/identity: "100/5/2" means (100/5)/2,
    // never substituting one operand for another.
    [Fact]
    public void ToExp_DivisionChain_PreservesOperandOrder()
    {
        AssertToExp("100/5/2", "((100/5)/2)");
    }

    // "10-3-2" is left-associative: (10-3)-2 = 5, not 10-(3-2) = 9. Now that BinaryExp.ToString()
    // parenthesizes every node, the tree shape is directly observable in the printed string.
    [Fact]
    public void ToExp_SubtractionChain_IsLeftAssociative()
    {
        AssertToExp("10-3-2", "((10-3)-2)");
    }

    // "1+2+3" is left-associative: (1+2)+3.
    [Fact]
    public void ToExp_AdditionChain_IsLeftAssociative()
    {
        AssertToExp("1+2+3", "((1+2)+3)");
    }

    // Implicit multiplication ("2x" with no operator between the operands) is not
    // supported by this parser and is documented to fail loudly rather than silently
    // drop a term.
    [Fact]
    public void ToExp_ImplicitMultiplication_ThrowsFormatException()
    {
        Assert.Throws<FormatException>(() => "2x".ToExp());
    }

    // "a*b+c*d" respects that multiplication binds tighter than addition on both sides:
    // (a*b)+(c*d).
    [Fact]
    public void ToExp_TwoMultiplicationsJoinedByAddition_EvaluatesCorrectly()
    {
        AssertValue("a*b+c*d", 41m, new Dictionary<string, decimal>
        {
            ["a"] = 2m, ["b"] = 3m, ["c"] = 5m, ["d"] = 7m
        });
    }

    // "a-b*c" respects that multiplication binds tighter than subtraction: a-(b*c).
    [Fact]
    public void ToExp_SubtractThenMultiply_EvaluatesCorrectly()
    {
        AssertValue("a-b*c", 4m, new Dictionary<string, decimal>
        {
            ["a"] = 10m, ["b"] = 2m, ["c"] = 3m
        });
    }

    // "2*a+3*b-4*c" mixes two multiplications with a trailing +/- chain: ((2*a)+(3*b))-(4*c).
    [Fact]
    public void ToExp_ChainedMultiplyAddSubtract_EvaluatesCorrectly()
    {
        AssertValue("2*a+3*b-4*c", -7m, new Dictionary<string, decimal>
        {
            ["a"] = 2m, ["b"] = 3m, ["c"] = 5m
        });
    }

    // "a+b*c+d" must mean (a+(b*c))+d, not let the trailing "+d" get absorbed into the
    // multiplication as a+(b*(c+d)).
    [Fact]
    public void ToExp_AddThenMultiplyThenAdd_DoesNotDistributeAcrossAddition()
    {
        AssertValue("a+b*c+d", 24m, new Dictionary<string, decimal>
        {
            ["a"] = 2m, ["b"] = 3m, ["c"] = 5m, ["d"] = 7m
        });
    }

    // "a/b*c+d" must mean ((a/b)*c)+d, not let the trailing "+d" get absorbed into the
    // multiplication as (a/b)*(c+d).
    [Fact]
    public void ToExp_DivideThenMultiplyThenAdd_DoesNotDistributeAcrossAddition()
    {
        AssertValue("a/b*c+d", 67m, new Dictionary<string, decimal>
        {
            ["a"] = 100m, ["b"] = 5m, ["c"] = 3m, ["d"] = 7m
        });
    }

    // "x*y*z+w" must mean ((x*y)*z)+w, not let the trailing "+w" get absorbed into the
    // multiplication chain as (x*y)*(z+w).
    [Fact]
    public void ToExp_MultiplyChainThenAdd_DoesNotDistributeAcrossAddition()
    {
        AssertValue("x*y*z+w", 37m, new Dictionary<string, decimal>
        {
            ["x"] = 2m, ["y"] = 3m, ["z"] = 5m, ["w"] = 7m
        });
    }

    private static void AssertToExp(string input, string expected)
    {
        Assert.Equal(expected, input.ToExp().ToString());
    }

    private static void AssertValue(string input, decimal expected, IReadOnlyDictionary<string, decimal> variables)
    {
        Assert.Equal(expected, Evaluate(input.ToExp(), variables));
    }

    private static decimal Evaluate(MathUtil.Exp exp, IReadOnlyDictionary<string, decimal> variables) => exp switch
    {
        MathUtil.ScalarExp scalar => decimal.Parse(scalar.Value),
        MathUtil.VariableExp variable => variables[variable.Identifier],
        MathUtil.BinaryExp binary => binary.Op switch
        {
            MathUtil.ExpOp.Plus => Evaluate(binary.Left, variables) + Evaluate(binary.Right, variables),
            MathUtil.ExpOp.Minus => Evaluate(binary.Left, variables) - Evaluate(binary.Right, variables),
            MathUtil.ExpOp.Multiply => Evaluate(binary.Left, variables) * Evaluate(binary.Right, variables),
            MathUtil.ExpOp.Divide => Evaluate(binary.Left, variables) / Evaluate(binary.Right, variables),
            _ => throw new NotSupportedException()
        },
        _ => throw new NotSupportedException()
    };
}
