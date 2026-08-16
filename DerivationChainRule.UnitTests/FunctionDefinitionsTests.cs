namespace DerivationChainRule.UnitTests;

public class FunctionDefinitionsTests
{
    [Fact]
    public void ConstantFunction()
    {
        var constant = Function.Create(Scalar.Create(5));
        Assert.Equal(Scalar.Create(5), constant.Evaluate());
    }
    [Fact]
    public void OriginLineFunction()
    {
        var xPlaceholder = Placeholder.Create("x");
        var line = Function.Create(xPlaceholder);
        for (int i = 0; i < 10; i++)
        {
            xPlaceholder.Scalar = Scalar.Create(i);
            Assert.Equal(i, line.Evaluate().Value);
        }
    }

    [Fact]
    public void LineWithSlopeFunction()
    {
        //y = 2x+3
        var xPlaceholder = Placeholder.Create("x");
        var x = Function.Create(xPlaceholder);
        var m = Function.Create(Scalar.Create(2));
        var n =  Function.Create(Scalar.Create(3));
        var y = x * m + n;
        for (int i = 0; i < 10; i++)
        {
            xPlaceholder.Scalar = Scalar.Create(i);
            Assert.Equal(2 * i + 3, y.Evaluate().Value);
        }
    }

    [Fact]
    public void QuadraticOriginFunction()
    {
        //y = x^2
        var xPlaceholder = Placeholder.Create("x");
        var x = Function.Create(xPlaceholder);
        var y = x * x;
        for (int i = 0; i < 10; i++)
        {
            xPlaceholder.Scalar = Scalar.Create(i);
            Assert.Equal(i * i, y.Evaluate().Value);
        }
    }

    [Fact]
    public void QuadraticMovedOriginFunction()
    {
        //y = 2*x^2 + 5*x + 7
        var xPlaceholder = Placeholder.Create("x");
        var aPlaceholder = Placeholder.Create("a");
        var bPlaceholder = Placeholder.Create("b");
        var cPlaceholder = Placeholder.Create("c");
        var x = Function.Create(xPlaceholder);
        var a = Function.Create(aPlaceholder);
        var b = Function.Create(bPlaceholder);
        var c = Function.Create(cPlaceholder);
        var q = a*x*x + b*x + c;

        for(int aa=0; aa<10; aa++)
            for(int bb=0; bb<10; bb++)
                for(int cc=0; cc<10; cc++)
                    for(int xx=0;xx<100;xx++)
                    {
                        aPlaceholder.Scalar = Scalar.Create(aa);
                        bPlaceholder.Scalar = Scalar.Create(bb);
                        cPlaceholder.Scalar = Scalar.Create(cc);
                        xPlaceholder.Scalar = Scalar.Create(xx);
                        var expected = aa * xx * xx + bb * xx + cc;
                        var functionValue = q.Evaluate();
                        Assert.Equal(expected, functionValue.Value);
                    }
    }
}
