using NumericalMethods;
using Serilog;
using Xunit.Abstractions;

namespace DerivationChainRule.NumericalMethods.UnitTests;

public class TrainNeuronTests
{
    private readonly ILogger _logger;

    public TrainNeuronTests(ITestOutputHelper output)
    {
        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.TestOutput(output)
            .CreateLogger();
    }

    [Fact]
    public void Train_Simple_Function()
    {
        //Arrange
        string fStr = "tanh(ln(x1*w1 + x2*w2 + b))";
        Function f = MathUtil.Translate(fStr);
        var variables = f.Params.ToDictionary(x => x.Identifier, x => x);
        var trainingSet = TrainingData.Create();
        for (int i = -50; i < 50; i++)
        {
            for (int j = -50; j < 50; j++)
            {
                trainingSet               
                    .Add(TrainingEntry.Create(i+j == 0 ? 0 : (i+j)/-(i+j), [(variables["x1"], Scalar.Create(i)), (variables["x2"], Scalar.Create(j))]));
            }
        }
        GradientDescend gr = new GradientDescend(f, epochs: 000);
        //Act
        Placeholder[] placeholders = gr.Train(trainingSet);
        _logger.Information(placeholders.Select(x => string.Format("{0}={1}", x.Identifier, x.Scalar.Value.Value)).Aggregate((x, y) => x + ", " + y));
        //Assert
        for (int i = -50; i < 50; i++)
        {
            for (int j = -50; j < 50; j++)
            {
                variables["x1"].Scalar = Scalar.Create(i);
                variables["x2"].Scalar = Scalar.Create(j);
                _logger.Information("f({0},{1})={2}", i, j, f.Evaluate().Value);
                Assert.True((i+j == 0? 0 : (i+j)/(i+j)) * f.Evaluate().Value < 0);
            }
        }
    }

    [Fact]
    public void Train_Simple_Line()
    {
        //Arrange
        var fStr = "w*x-b";
        Function f = MathUtil.Translate(fStr);
        var variables = f.Params.ToDictionary(x => x.Identifier, x => x);
        var trainingSet = TrainingData.Create();
        for (int i = -25; i < 50; i++)
        {
            trainingSet.Add(TrainingEntry.Create(2*i-3, [(variables["x"], Scalar.Create(i))]));
        }
        GradientDescend gradientDescend = new GradientDescend(f, epochs: 1_000_000);
        //Act
        Placeholder[] placeholders = gradientDescend.Train(trainingSet);
        _logger.Information(placeholders.Select(x => string.Format("{0}={1}", x.Identifier, x.Scalar.Value.Value)).Aggregate((x, y) => x + ", " + y));
        //Assert
        for (int i = -25; i < 50; i++)
        {
            variables["x"].Scalar = Scalar.Create(i);
            Assert.Equal(2*i-3, Math.Round(f.Evaluate().Value), 9);
        }
    }
}