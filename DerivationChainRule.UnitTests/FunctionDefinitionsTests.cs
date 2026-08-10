namespace DerivationChainRule.UnitTests;
using Placeholders = Placeholder.Placeholders;

public class FunctionDefinitionsTests
{
    private readonly Placeholders _placeholders = new Placeholders();
    [Fact]
    public void ConstantFunction()
    {
        var constant = Function.Create(Scalar.Create(5));
        Assert.Equal(Scalar.Create(5), constant.Evaluate(null));
    }
    [Fact]
    public void OriginLineFunction()
    {
        var line = Function.Create(_placeholders.Create("x"));
        for (int i = 0; i < 10; i++)
        {
            _placeholders["x"] = Scalar.Create(i);
            Assert.Equal(i, line.Evaluate(_placeholders).Value);
        }
    }
    
    [Fact]
    public void LineWithSlopeFunction()
    {
        //y = 2x+3
        var x = Function.Create(_placeholders.Create("x"));
        var m = Function.Create(Scalar.Create(2));
        var n =  Function.Create(Scalar.Create(3));
        var y = x * m + n;
        for (int i = 0; i < 10; i++)
        {
            _placeholders["x"] = Scalar.Create(i);
            Assert.Equal(2 * i + 3, y.Evaluate(_placeholders).Value);
        }
    }
    
    [Fact]
    public void QuadraticOriginFunction()
    {
        //y = x^2
        var x = Function.Create(_placeholders.Create("x"));
        var y = x * x;
        for (int i = 0; i < 10; i++)
        {
            _placeholders["x"] = Scalar.Create(i);
            Assert.Equal(i * i, y.Evaluate(_placeholders).Value);
        }
    }
    
    [Fact]
    public void QuadraticMovedOriginFunction()
    {
        //y = 2*x^2 + 5*x + 7
        var x = Function.Create(_placeholders.Create("x"));
        var a = Function.Create(_placeholders.Create("a"));
        var b = Function.Create(_placeholders.Create("b"));
        var c = Function.Create(_placeholders.Create("c"));
        var q = a*x*x + b*x + c;

        for(int aa=0; aa<10; aa++)
            for(int bb=0; bb<10; bb++)
                for(int cc=0; cc<10; cc++)
                    for(int xx=0;xx<100;xx++)
                    {
                        _placeholders["a"] = Scalar.Create(aa);
                        _placeholders["b"] = Scalar.Create(bb);
                        _placeholders["c"] = Scalar.Create(cc);
                        _placeholders["x"] = Scalar.Create(xx);
                        var expected = aa * xx * xx + bb * xx + cc;
                        var functionValue = q.Evaluate(_placeholders);
                        Assert.Equal(expected, functionValue.Value);
                    }
    }
}