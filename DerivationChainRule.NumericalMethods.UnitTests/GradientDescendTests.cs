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
        Placeholder[] min = gradientDescend.GetMinimun(algorithm);
        //Assert
        Assert.True(Math.Abs(-0.5m - min[0].Scalar.Value) < GradientDescend.Threshold);
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
        Placeholder[] min = gradientDescend.GetMinimun();
        //Assert
        Assert.True(Math.Abs(5m - min[0].Scalar.Value) < GradientDescend.Threshold);
        xPlaceholder.Scalar = Scalar.Create(5m);
        Assert.True(l.Evaluate().Value < GradientDescend.Threshold);
    }
    
    [Fact]
    public void GetMinimun_Of_Lost_Function_To_Train_A_Function()
    {
        //Arrange
        string functionStr = "w*x+d";
        Function line = MathUtil.Translate(functionStr);
        Function o = Function.Create(1);
        Function l =  (o - line) * (o - line);
        line.Params.First(x => x.Identifier == "x").Scalar = Scalar.Create(1m);
        GradientDescend gradientDescend = new GradientDescend(l, epochs: 1_000_000);
        //Act
        _ = gradientDescend.GetMinimun();
        Assert.True(l.Evaluate().Value < GradientDescend.Threshold);
    }
    
    [Fact]
    public void GetMinimun_Of_Lost_Function_To_Train_A_Quadratic_Function()
    {
        //Arrange
        decimal y_value = 5;
        string functionStr = "x*x+d";
        Function line = MathUtil.Translate(functionStr);
        Function o = Function.Create(y_value);
        Function l =  (o - line) * (o - line);
        line.Params.First(x => x.Identifier == "x").Scalar = Scalar.Create(2m);
        GradientDescend gradientDescend = new GradientDescend(l, epochs: 1_000_000);
        //Act
        _ = gradientDescend.GetMinimun();
        Assert.True(l.Evaluate().Value < GradientDescend.Threshold);
    }
    
    [Fact]
    public void Train_A_Linear_Function()
    {
        //Arrange
        string functionStr = "w*x+d";
        Function line = MathUtil.Translate(functionStr);
        Placeholder xPlaceHolder = line.Params.First(x => x.Identifier == "x");
        var trainingSet = TrainingData.Create()
            .Add(TrainingEntry.Create(1, [(xPlaceHolder, 1)]))
            .Add(TrainingEntry.Create(2, [(xPlaceHolder, 2)]))
            .Add(TrainingEntry.Create(3, [(xPlaceHolder, 3)]))
            .Add(TrainingEntry.Create(4, [(xPlaceHolder, 4)]))
            .Add(TrainingEntry.Create(5, [(xPlaceHolder, 5)]))
            .Add(TrainingEntry.Create(6, [(xPlaceHolder, 6)]))
            .Add(TrainingEntry.Create(7, [(xPlaceHolder, 7)]))
            .Add(TrainingEntry.Create(8, [(xPlaceHolder, 8)]))
            .Add(TrainingEntry.Create(9, [(xPlaceHolder, 9)]))
            .Add(TrainingEntry.Create(10, [(xPlaceHolder, 10)]));
        GradientDescend gradientDescend = new GradientDescend(line, epochs: 1_000_000);
        //Act
        var placeholders = gradientDescend.Train(trainingSet);
        foreach (var placeholder in placeholders)
        {
            _logger.Information(placeholder.Identifier + ": "  + placeholder.Scalar.Value);
        }
        foreach(var entry in trainingSet.Entries)
        {
            xPlaceHolder.Scalar = entry.IndependentVariables[0].Item2;
            Assert.True(Math.Abs(line.Evaluate().Value - entry.DependentVariable) < 0.5m);
        }
    }
}
