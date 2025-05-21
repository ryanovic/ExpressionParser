using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ExpressionParser
{
    internal class ExpressionEvaluator : IReducer
    {
        private readonly record struct Operand(string? Variable, int Value);

        private readonly Stack<Operand> evaluationStack = new Stack<Operand>();
        private readonly Dictionary<string, int> variables;

        public ExpressionEvaluator(Dictionary<string, int> variables)
        {
            this.variables = variables;
        }

        public void Push(int value)
        {
            evaluationStack.Push(new Operand(null, value));
        }

        public void Push(string variable)
        {
            evaluationStack.Push(new Operand(variable, default));
        }

        public int GetResult()
        {
            if (evaluationStack.Count != 1)
            {
                throw new InvalidOperationException("Invalid expression.");
            }

            return Pop();
        }

        public void Reduce(OperationCode op)
        {
            int result, value;

            switch (op)
            {
                case OperationCode.PrefixIncrement:
                    ref int variable = ref PopRef(create: false);
                    result = ++variable;
                    break;
                case OperationCode.PrefixDecrement:
                    variable = ref PopRef(create: false);
                    result = --variable;
                    break;
                case OperationCode.PostfixIncrement:
                    variable = ref PopRef(create: false);
                    result = variable++;
                    break;
                case OperationCode.PostfixDecrement:
                    variable = ref PopRef(create: false);
                    result = variable--;
                    break;
                case OperationCode.Negate:
                    result = -Pop();
                    break;
                case OperationCode.Add:
                    (int a, int b) = Pop2();
                    result = a + b;
                    break;
                case OperationCode.Subtract:
                    (a, b) = Pop2();
                    result = a - b;
                    break;
                case OperationCode.Divide:
                    (a, b) = Pop2();
                    result = a / b;
                    break;
                case OperationCode.Multiply:
                    (a, b) = Pop2();
                    result = a * b;
                    break;
                case OperationCode.CompareEq:
                    (a, b) = Pop2();
                    result = FromBoolean(a == b);
                    break;
                case OperationCode.CompareNotEq:
                    (a, b) = Pop2();
                    result = FromBoolean(a != b);
                    break;
                case OperationCode.CompareGreater:
                    (a, b) = Pop2();
                    result = FromBoolean(a > b);
                    break;
                case OperationCode.CompareGreaterEq:
                    (a, b) = Pop2();
                    result = FromBoolean(a >= b);
                    break;
                case OperationCode.CompareLess:
                    (a, b) = Pop2();
                    result = FromBoolean(a < b);
                    break;
                case OperationCode.CompareLessEq:
                    (a, b) = Pop2();
                    result = FromBoolean(a <= b);
                    break;
                case OperationCode.Switch:
                    (int cond, a, b) = Pop3();
                    result = ToBoolean(cond) ? a : b;
                    break;
                case OperationCode.Assign:
                    variable = ref PopRef2(create: true, out value);
                    result = variable = value;
                    break;
                case OperationCode.AssignAdd:
                    variable = ref PopRef2(create: false, out value);
                    result = variable += value;
                    break;
                case OperationCode.AssignSub:
                    variable = ref PopRef2(create: false, out value);
                    result = variable -= value;
                    break;
                case OperationCode.AssignMul:
                    variable = ref PopRef2(create: false, out value);
                    result = variable *= value;
                    break;
                case OperationCode.AssignDiv:
                    variable = ref PopRef2(create: false, out value);
                    result = variable /= value;
                    break;
                default:
                    throw new NotImplementedException();
            }

            Push(result);
        }

        private ref int PopRef(bool create)
        {
            var tmp = evaluationStack.Pop();

            if (tmp.Variable == null)
            {
                throw new InvalidOperationException("LValue is expected.");
            }

            return ref create
                ? ref GetOrAddVariableRef(tmp.Variable)
                : ref GetVariableRef(tmp.Variable);
        }

        private ref int PopRef2(bool create, out int value)
        {
            value = Pop();
            return ref PopRef(create);
        }

        private int Pop()
        {
            var tmp = evaluationStack.Pop();
            return tmp.Variable == null ? tmp.Value : GetVariableRef(tmp.Variable);
        }

        private (int, int) Pop2()
        {
            var b = Pop();
            var a = Pop();
            return (a, b);
        }

        private (int, int, int) Pop3()
        {
            var c = Pop();
            var b = Pop();
            var a = Pop();
            return (a, b, c);
        }

        private ref int GetOrAddVariableRef(string name)
        {
            return ref CollectionsMarshal.GetValueRefOrAddDefault(variables, name, out _);
        }

        private ref int GetVariableRef(string name)
        {
            ref int value = ref CollectionsMarshal.GetValueRefOrNullRef(variables, name);

            if (Unsafe.IsNullRef(ref value))
            {
                throw new InvalidOperationException($"Variable '{name}' is not defined.");
            }

            return ref value;
        }

        private static bool ToBoolean(int x) => x > 0;

        private static int FromBoolean(bool flag) => flag ? 1 : 0;
    }
}
