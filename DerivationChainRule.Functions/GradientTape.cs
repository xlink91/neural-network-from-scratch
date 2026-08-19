namespace DerivationChainRule;

// Reverse-mode automatic differentiation (backpropagation) over a Function tree.
//
// The tree's structure is immutable, so the topological order and child indexes are computed
// once at construction; every Compute() afterwards is just two array sweeps (forward values,
// backward adjoints) with no allocations. One pass yields the function value and the partial
// derivative with respect to EVERY placeholder simultaneously — O(n) total, versus O(P * n)
// for evaluating P symbolic derivative trees.
public sealed class GradientTape
{
    private readonly Function[] _nodes; // topological order: children always before parents
    private readonly int[] _leftIndex;
    private readonly int[] _rightIndex;
    private readonly double[] _values;
    private readonly double[] _adjoints;
    private readonly Dictionary<Placeholder, List<int>> _parameterNodeIndexes;

    public GradientTape(Function function)
    {
        function.ThrowIfDuplicateParameters();
        List<Function> order = new List<Function>();
        HashSet<Function> visited = new HashSet<Function>(ReferenceEqualityComparer.Instance);
        Visit(function, visited, order);
        _nodes = order.ToArray();
        _leftIndex = new int[_nodes.Length];
        _rightIndex = new int[_nodes.Length];
        _values = new double[_nodes.Length];
        _adjoints = new double[_nodes.Length];
        _parameterNodeIndexes = new Dictionary<Placeholder, List<int>>(ReferenceEqualityComparer.Instance);
        Dictionary<Function, int> indexOf = new Dictionary<Function, int>(ReferenceEqualityComparer.Instance);
        for (int i = 0; i < _nodes.Length; i++)
        {
            indexOf[_nodes[i]] = i;
        }
        for (int i = 0; i < _nodes.Length; i++)
        {
            Function node = _nodes[i];
            _leftIndex[i] = node.Left != null ? indexOf[node.Left] : -1;
            _rightIndex[i] = node.Right != null ? indexOf[node.Right] : -1;
            if (node.Operator == Function.Op.PlaceHolder)
            {
                if (!_parameterNodeIndexes.TryGetValue(node.Placeholder, out List<int>? indexes))
                {
                    _parameterNodeIndexes[node.Placeholder] = indexes = new List<int>();
                }
                indexes.Add(i);
            }
        }
    }

    // Runs one forward + backward pass at the placeholders' current values.
    // Returns the function value; per-placeholder derivatives are then read via Gradient().
    public double Compute()
    {
        Forward();
        Backward();
        return _values[_values.Length - 1];
    }

    public double Gradient(Placeholder parameter)
    {
        if (!_parameterNodeIndexes.TryGetValue(parameter, out List<int>? indexes))
        {
            return 0;
        }
        double gradient = 0;
        foreach (int index in indexes)
        {
            gradient += _adjoints[index];
        }
        return gradient;
    }

    private void Forward()
    {
        for (int i = 0; i < _nodes.Length; i++)
        {
            Function node = _nodes[i];
            _values[i] = node.Operator switch
            {
                Function.Op.Add => _values[_leftIndex[i]] + _values[_rightIndex[i]],
                Function.Op.Subtract => _values[_leftIndex[i]] - _values[_rightIndex[i]],
                Function.Op.Multiply => _values[_leftIndex[i]] * _values[_rightIndex[i]],
                Function.Op.Divide => _values[_leftIndex[i]] / _values[_rightIndex[i]],
                Function.Op.PlaceHolder => (node.Placeholder.Scalar ?? throw new Exception(
                    $"Parameter '{node.Placeholder.Identifier}' has no value assigned.")).Value,
                Function.Op.Scalar => node.Scalar.Value,
                Function.Op.Sin => Math.Sin(_values[_leftIndex[i]]),
                Function.Op.Cos => Math.Cos(_values[_leftIndex[i]]),
                Function.Op.Exp => Math.Exp(_values[_leftIndex[i]]),
                Function.Op.Ln => Math.Log(_values[_leftIndex[i]]),
                _ => throw new Exception("Invalid operator")
            };
        }
    }

    private void Backward()
    {
        Array.Clear(_adjoints);
        _adjoints[_adjoints.Length - 1] = 1;
        for (int i = _nodes.Length - 1; i >= 0; i--)
        {
            double adjoint = _adjoints[i];
            switch (_nodes[i].Operator)
            {
                case Function.Op.Add:
                    _adjoints[_leftIndex[i]] += adjoint;
                    _adjoints[_rightIndex[i]] += adjoint;
                    break;
                case Function.Op.Subtract:
                    _adjoints[_leftIndex[i]] += adjoint;
                    _adjoints[_rightIndex[i]] -= adjoint;
                    break;
                case Function.Op.Multiply:
                    _adjoints[_leftIndex[i]] += adjoint * _values[_rightIndex[i]];
                    _adjoints[_rightIndex[i]] += adjoint * _values[_leftIndex[i]];
                    break;
                case Function.Op.Divide:
                    // d(L/R)/dL = 1/R, d(L/R)/dR = -L/R^2 = -value/R
                    _adjoints[_leftIndex[i]] += adjoint / _values[_rightIndex[i]];
                    _adjoints[_rightIndex[i]] -= adjoint * _values[i] / _values[_rightIndex[i]];
                    break;
                case Function.Op.Sin:
                    _adjoints[_leftIndex[i]] += adjoint * Math.Cos(_values[_leftIndex[i]]);
                    break;
                case Function.Op.Cos:
                    _adjoints[_leftIndex[i]] -= adjoint * Math.Sin(_values[_leftIndex[i]]);
                    break;
                case Function.Op.Exp:
                    // value = exp(L), which is also the derivative
                    _adjoints[_leftIndex[i]] += adjoint * _values[i];
                    break;
                case Function.Op.Ln:
                    _adjoints[_leftIndex[i]] += adjoint / _values[_leftIndex[i]];
                    break;
            }
        }
    }

    private static void Visit(Function node, HashSet<Function> visited, List<Function> order)
    {
        if (!visited.Add(node))
        {
            return;
        }
        if (node.Left != null)
        {
            Visit(node.Left, visited, order);
        }
        if (node.Right != null)
        {
            Visit(node.Right, visited, order);
        }
        order.Add(node);
    }
}
