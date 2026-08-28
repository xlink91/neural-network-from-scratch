namespace DerivationChainRule.UnitTests;

public class FunctionBackpropagationTests
{
    // Backpropagation must produce exactly the same partial derivatives as the symbolic
    // Derivative engine — for every parameter of the function, in a single pass. The symbolic
    // engine acts as the correctness oracle; x/y/a/b/c fill whichever identifiers appear.
    [Theory]
    [InlineData("x", 5, 0, 0, 0, 0)]
    [InlineData("2*x+3", 10, 0, 0, 0, 0)]
    [InlineData("x*x", 3, 0, 0, 0, 0)]
    [InlineData("x*x*x", 2, 0, 0, 0, 0)]
    [InlineData("a*x+b", 4, 0, 5, 7, 0)]
    [InlineData("a*x*x+b*x+c", 4, 0, 2, 3, 5)]
    [InlineData("x*x+y*y", 3, 5, 0, 0, 0)]
    [InlineData("x*y", 9, 7, 0, 0, 0)]
    [InlineData("x/a", 6, 0, 2, 0, 0)]
    [InlineData("a*x*x+b*x*y+c*y*y", 5, 6, 2, 3, 4)]
    public void Backpropagate_MatchesSymbolicDerivative_ForEveryParameter(
        string expression, double x, double y, double a, double b, double c)
    {
        Function function = MathUtil.Translate(expression);
        SetIfPresent(function, "x", x);
        SetIfPresent(function, "y", y);
        SetIfPresent(function, "a", a);
        SetIfPresent(function, "b", b);
        SetIfPresent(function, "c", c);

        (double value, Dictionary<Placeholder, double> gradients) = function.Backpropagate();

        Assert.Equal(function.Evaluate().Value, value, 9);
        foreach (Placeholder parameter in function.Params)
        {
            double symbolic = new Derivative(function).Derive(parameter).Evaluate().Value;
            Assert.Equal(symbolic, gradients[parameter], 9);
        }
    }

    // Unary operators aren't reachable through the string parser, and x is deliberately the
    // SAME node shared by several subtrees — exercising the DAG path of the tape.
    [Fact]
    public void Backpropagate_UnaryComposite_MatchesSymbolicDerivative()
    {
        var xPlaceholder = Placeholder.Create("x");
        Function x = Function.Create(xPlaceholder);
        Function f = Function.Exp(Function.Sin(x * Function.Create(Scalar.Create(2))) + Function.Ln(x * x))
                     + Function.Cos(x);

        for (double value = 0.5; value <= 2.5; value += 0.5)
        {
            xPlaceholder.Scalar = Scalar.Create(value);
            (double result, Dictionary<Placeholder, double> gradients) = f.Backpropagate();
            double symbolic = new Derivative(f).Derive(xPlaceholder).Evaluate().Value;

            Assert.Equal(f.Evaluate().Value, result, 9);
            Assert.Equal(symbolic, gradients[xPlaceholder], 9);
        }
    }

    // tanh is the classic neural-network activation; f = tanh(w*x + d) is one neuron.
    [Fact]
    public void Backpropagate_TanhNeuron_MatchesSymbolicDerivative()
    {
        Function inner = MathUtil.Translate("w*x+d");
        Function neuron = Function.Tanh(inner);
        SetIfPresent(neuron, "w", 0.7);
        SetIfPresent(neuron, "x", 1.3);
        SetIfPresent(neuron, "d", -0.2);

        (double value, Dictionary<Placeholder, double> gradients) = neuron.Backpropagate();

        Assert.Equal(neuron.Evaluate().Value, value, 9);
        foreach (Placeholder parameter in neuron.Params)
        {
            double symbolic = new Derivative(neuron).Derive(parameter).Evaluate().Value;
            Assert.Equal(symbolic, gradients[parameter], 9);
        }
    }

    [Fact]
    public void GradientTape_Reused_RecomputesGradientsAtNewValues()
    {
        var xPlaceholder = Placeholder.Create("x");
        Function x = Function.Create(xPlaceholder);
        Function f = x * x;
        GradientTape tape = new GradientTape(f);

        xPlaceholder.Scalar = Scalar.Create(2);
        Assert.Equal(4, tape.Compute(), 9);
        Assert.Equal(4, tape.Gradient(xPlaceholder), 9);

        xPlaceholder.Scalar = Scalar.Create(5);
        Assert.Equal(25, tape.Compute(), 9);
        Assert.Equal(10, tape.Gradient(xPlaceholder), 9);
    }

    [Fact]
    public void Backpropagate_DuplicatePlaceholderInstancesForSameIdentifier_Throws()
    {
        var x1 = Function.Create(Placeholder.Create("x"));
        var x2 = Function.Create(Placeholder.Create("x"));
        var y = x1 * x2;

        var exception = Assert.Throws<Exception>(() => y.Backpropagate());
        Assert.Contains("x", exception.Message);
    }

    [Fact]
    public void Backpropagate_UnsetPlaceholder_ThrowsDescriptiveException()
    {
        Function f = Function.Create(Placeholder.Create("w")) + Function.Create(Scalar.One);

        var exception = Assert.Throws<Exception>(() => f.Backpropagate());
        Assert.Contains("w", exception.Message);
    }

    private static void SetIfPresent(Function function, string identifier, double value)
    {
        var placeholder = function.Params.FirstOrDefault(p => p.Identifier == identifier);
        if (placeholder != null)
        {
            placeholder.Scalar = Scalar.Create(value);
        }
    }
}
