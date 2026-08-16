using NumericalMethods;
using Serilog;
using Xunit.Abstractions;

namespace DerivationChainRule.NumericalMethods.UnitTests;
using GradienDescentAlgorithm = GradientDescend.GradienDescentAlgorithm;

public class GradientDescendTests
{
    private readonly ILogger _logger;

    public GradientDescendTests(ITestOutputHelper output)
    {
        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.TestOutput(output)
            .CreateLogger();
    }

    [Theory]
    [InlineData(GradienDescentAlgorithm.GradientDescent)]
    [InlineData(GradienDescentAlgorithm.GradientDescentMomentum)]
    public void GetMinimun_From_Quadratic(GradienDescentAlgorithm algorithm)
    {
        //Arrange
        var xPlaceholder = Placeholder.Create("x");
        Function a = Function.Create(Scalar.Create(2));
        Function b =  Function.Create(Scalar.Create(1));
        Function x = Function.Create(xPlaceholder);
        Function y = (a*x+b)*(a*x+b);
        //Act
        GradientDescend gradientDescend = new GradientDescend(y, epochs: 1000_000, logger: _logger);
        Scalar min = gradientDescend.GetMinimun(algorithm);
        //Assert
        Assert.True(Math.Abs(-0.5m - min.Value) < GradientDescend.Threshold);
        xPlaceholder.Scalar = Scalar.Create(-0.5m);
        Assert.True(y.Evaluate().Value < GradientDescend.Threshold);
    }

    [Fact]
    public void GetMinimun_Of_Lost_Function()
    {
        //Arrange
        var xPlaceholder = Placeholder.Create("x");
        Function a = Function.Create(Scalar.Create(2));
        Function b =  Function.Create(Scalar.Create(1));
        Function x = Function.Create(xPlaceholder);
        Function y = (a*x+b)*(a*x+b);
        //Act
        Function o = Function.Create(121);
        Function l = (o - y) * (o - y);
        GradientDescend gradientDescend = new GradientDescend(l, logger: _logger);
        Scalar min = gradientDescend.GetMinimun();
        //Assert
        Assert.True(Math.Abs(5m - min.Value) < GradientDescend.Threshold);
        xPlaceholder.Scalar = Scalar.Create(5m);
        Assert.True(l.Evaluate().Value < GradientDescend.Threshold);
    }
}
