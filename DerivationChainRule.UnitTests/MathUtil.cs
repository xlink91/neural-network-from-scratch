namespace DerivationChainRule.UnitTests;

public static class MathUtil
{
    public static Function Translate(string expStr)
    {
        Exp exp = expStr.ToExp();
        return ExpToFunction(exp, new Dictionary<string, Placeholder>());
    }

    private static Function ExpToFunction(Exp exp, Dictionary<string, Placeholder> placeholders)
    {
        if (exp is ScalarExp scalarExp)
        {
            return Function.Create(Scalar.Create(decimal.Parse(scalarExp.Value)));
        }
        if (exp is VariableExp variableExp)
        {
            if (!placeholders.TryGetValue(variableExp.Identifier, out var placeholder))
            {
                placeholders[variableExp.Identifier] = placeholder = Placeholder.Create(variableExp.Identifier);
            }
            return Function.Create(placeholder);
        }
        if (exp is BinaryExp binaryExp)
        {
            Function left = ExpToFunction(binaryExp.Left, placeholders);
            Function right = ExpToFunction(binaryExp.Right, placeholders);
            return binaryExp.Op switch
            {
                ExpOp.Plus => left + right,
                ExpOp.Minus => left - right,
                ExpOp.Multiply => left * right,
                ExpOp.Divide => left / right,
                _ => throw new NotImplementedException()
            };
        }
        throw new NotImplementedException();
    }
    public static Exp ToExp(this string expStr)
    {
        string number = string.Empty;
        string variable = string.Empty;
        Stack<Exp> st = new Stack<Exp>();
        Stack<string> opSt = new Stack<string>();
        int idx = 0;
        while (idx < expStr.Length)
        {
            while(idx < expStr.Length && char.IsDigit(expStr[idx]))
            {
                number += expStr[idx];
                ++idx;
            }
            if (!string.IsNullOrEmpty(number))
            {
                st.Push(new ScalarExp(number));
                number = string.Empty;
            }
            while (idx < expStr.Length && char.IsLetter(expStr[idx]))
            {
                variable += expStr[idx];
                ++idx;
            }
            if (!string.IsNullOrEmpty(variable))
            {
                st.Push(new VariableExp(variable));
                variable = string.Empty;
            }
            if (idx < expStr.Length && !char.IsLetterOrDigit(expStr[idx]))
            {
                opSt.Push(expStr[idx].ToString());
                ++idx;
            }
        }
        if (opSt.Count == 0 && st.Count > 1)
        {
            throw new FormatException();
        }
        if (opSt.Count == 0 && st.Count == 1)
        {
            return st.Pop();
        }
        while (opSt.Count > 0)
        {
            var op = opSt.Pop();
            if("+-/*".Contains(op))
            {
                var right = st.Pop();
                var left = st.Pop();
                ExpOp expOp = op switch
                {
                    "+" => ExpOp.Plus,
                    "-" => ExpOp.Minus,
                    "*" => ExpOp.Multiply,
                    "/" => ExpOp.Divide,
                    _ => throw new NotImplementedException()
                };
                st.Push(new BinaryExp(expOp, left, right));
            }
        }
        return TransformOperationPriority(st.Pop());
    }
    private static Exp TransformOperationPriority(this Exp exp)
    {
        if (exp is ScalarExp scalar)
        {
            return scalar;
        }
        if (exp is VariableExp variable)
        {
            return variable;
        }
        if (exp is BinaryExp binary)
        {
            if (binary.Op is ExpOp.Divide or ExpOp.Multiply)
            {
                var rightExp = binary.Right;
                if (rightExp is BinaryExp binaryRightExp)
                {
                    var left = binaryRightExp.Left;
                    binary.Left = new BinaryExp(binary.Op, TransformOperationPriority(binary.Left), left);
                    binary.Right = TransformOperationPriority(binaryRightExp.Right);
                    binary.Op = binaryRightExp.Op;
                    return TransformOperationPriority(binary);
                }
            }
            else
            {
                if (binary.Op is ExpOp.Plus or ExpOp.Minus)
                {
                    var rightExp = binary.Right;
                    if (rightExp is BinaryExp { Op: ExpOp.Plus or ExpOp.Minus } binaryRightExp)
                    {
                        var left = binaryRightExp.Left;
                        binary.Left = new BinaryExp(binary.Op, TransformOperationPriority(binary.Left), left);
                        binary.Right = TransformOperationPriority(binaryRightExp.Right);
                        binary.Op = binaryRightExp.Op;
                        return TransformOperationPriority(binary);
                    }
                    return new BinaryExp(binary.Op, TransformOperationPriority(binary.Left), TransformOperationPriority(binary.Right));
                } 
            }
        }
        return exp;
    }
    public abstract class Exp
    {
        public ExpType Type { get; set; }
        protected Exp(ExpType type)
        {
            Type = type;
        }
    }
    public class ScalarExp : Exp
    {
        public ScalarExp(string value) : base(ExpType.SCALAR)
        {
            Value = value;
        }
        public string Value { get; set; }

        public override string ToString()
        {
            return string.Format("{0}", Value);
        }
    }
    public class VariableExp : Exp
    {
        public string Identifier { get; set; }

        public VariableExp(string identifier) : base(ExpType.UNARY)
        {
            Identifier = identifier;
        }

        public override string ToString()
        {
            return string.Format("{0}", Identifier);
        }
    }
    public class BinaryExp : Exp
    {
        public BinaryExp(ExpOp op, Exp left, Exp right) : base(ExpType.BINARY)
        {
            Left = left;
            Right = right;
            Op = op;
        }
        public Exp Left { get; set;  }
        public Exp Right { get; set; }
        public ExpOp Op { get; set; }

        public override string ToString()
        {
            return string.Format("({0}{1}{2})",Left,Op.SignName(),Right);
        }
    }
    public enum ExpType
    {
        SCALAR,
        BINARY,
        UNARY
    }
    public enum ExpOp
    {
        Plus,
        Minus,
        Multiply,
        Divide,
    }
}

public static class ExpOpExtensions
{
    public static string SignName(this MathUtil.ExpOp op)
    {
        return op switch
        {
            MathUtil.ExpOp.Plus => "+",
            MathUtil.ExpOp.Minus => "-",
            MathUtil.ExpOp.Multiply => "*",
            MathUtil.ExpOp.Divide => "/",
            _ => throw new NotImplementedException()
        };
    }
}
