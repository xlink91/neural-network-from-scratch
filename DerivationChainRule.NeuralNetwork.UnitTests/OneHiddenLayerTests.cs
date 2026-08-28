using NumericalMethods;
using Serilog;
using Xunit.Abstractions;

namespace DerivationChainRule.NeuralNetwork.UnitTests;

public class OneHiddenLayerTests
{
    private readonly ILogger _logger;

    public OneHiddenLayerTests(ITestOutputHelper output)
    {
        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.TestOutput(output)
            .CreateLogger();
    }
    
    [Fact]
    public void Xor_Function()
    {
        //Arrange
        var input1 = Placeholder.X;
        var input2 = Placeholder.X;
        Neuron hiddenNeuron1 = Neuron.Create([input1, input2]);
        Neuron hiddenNeuron2 = Neuron.Create([input1, input2]);
        Layer hiddenLayer = new Layer([hiddenNeuron1, hiddenNeuron2]);
        TrainingData trainingData = TrainingData.Create()
            .Add(TrainingEntry.Create(0, [(input1, 0), (input2, 0)]))
            .Add(TrainingEntry.Create(1, [(input1, 0), (input2, 1)]))
            .Add(TrainingEntry.Create(1, [(input1, 1), (input2, 0)]))
            .Add(TrainingEntry.Create(0, [(input1, 1), (input2, 1)]));
        GradientDescend gradientDescend = new GradientDescend(hiddenLayer.F, epochs: 30_000_000);
        //Act
        Placeholder[] placeholders = gradientDescend.Train(trainingData);
        _logger.Information(placeholders.Select(x => string.Format("{0}={1}", x.Identifier, x.Scalar.Value.Value)).Aggregate((x, y) => x + ", " + y));
        //Assert
        var f = hiddenLayer.F;
        /*
        input1.Scalar = Scalar.One;
        input2.Scalar = Scalar.One;
        Assert.True(Math.Abs(f.Evaluate().Value - 0.0) <= 1e-3);
        */
        for (int i = 0; i < 10; ++i)
        {
            for (int j = 0; j < 10; ++j)
            {
                input1.Scalar = new Scalar(i);
                input2.Scalar = new Scalar(j);
                double expected = (i ^ j) == 1 ? 1.0 : 0.0;
                _logger.Information("Expected: {i}^{j}={Expected} <-> Actual: {Actual}", i, j, expected, f.Evaluate().Value);
            }
        }
    }
}