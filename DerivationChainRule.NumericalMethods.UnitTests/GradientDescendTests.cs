using NumericalMethods;
using Serilog;
using Xunit.Abstractions;

namespace DerivationChainRule.NumericalMethods.UnitTests;
using Placeholders = DerivationChainRule.Placeholder.Placeholders;
using GradienDescentAlgorithm = GradientDescend.GradienDescentAlgorithm;

public class GradientDescendTests
{
    private Placeholders _placeholders = new Placeholders();
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
        Function a = Function.Create(Scalar.Create(2));   
        Function b =  Function.Create(Scalar.Create(1));
        Function x = Function.Create(_placeholders.Create("x"));
        Function y = (a*x+b)*(a*x+b);
        //Act
        GradientDescend gradientDescend = new GradientDescend(y, _placeholders, epochs: 1000_000, logger: _logger);
        Scalar min = gradientDescend.GetMinimun(algorithm);
        //Assert
        Assert.True(Math.Abs(-0.5m - min.Value) < GradientDescend.Threshold);
        _placeholders["x"] = Scalar.Create(-0.5m);
        Assert.True(y.Evaluate(_placeholders).Value < GradientDescend.Threshold);
    }
    
    [Fact]
    public void GetMinimun_Of_Lost_Function()
    {
        //Arrange
        Function a = Function.Create(Scalar.Create(2));   
        Function b =  Function.Create(Scalar.Create(1));
        Function x = Function.Create(_placeholders.Create("x"));
        Function y = (a*x+b)*(a*x+b);
        //Act
        Function o = Function.Create(121);
        Function l = (o - y) * (o - y);
        GradientDescend gradientDescend = new GradientDescend(l, _placeholders, logger: _logger);
        Scalar min = gradientDescend.GetMinimun();
        //Assert
        Assert.True(Math.Abs(5m - min.Value) < GradientDescend.Threshold);
        _placeholders["x"] = Scalar.Create(5m);
        Assert.True(l.Evaluate(_placeholders).Value < GradientDescend.Threshold);
    }
}