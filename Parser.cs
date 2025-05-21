namespace ExpressionParser
{
    public class Parser
    {
        private readonly record struct Operator(Token Token, OperatorOrder Order, OperationCode Code);

        private bool pendingOperator = false;
        private readonly Stack<Operator> operators = new Stack<Operator>();
        private readonly IReducer reducer;

        public Parser(IReducer reducer)
        {
            this.reducer = reducer;
        }

        public void Parse(ReadOnlySpan<char> expression)
        {
            for (int i = 0; i < expression.Length; i++)
            {
                switch (expression[i])
                {
                    case '+':
                        if (i + 1 < expression.Length)
                        {
                            if (expression[i + 1] == '+')
                            {
                                HandleToken(Token.Increment, expression.Slice(i++, 2));
                                break;
                            }

                            if (expression[i + 1] == '=')
                            {
                                HandleToken(Token.AssignAdd, expression.Slice(i++, 2));
                                break;
                            }
                        }

                        HandleToken(Token.Add, expression.Slice(i, 1));
                        break;
                    case '-':
                        if (i + 1 < expression.Length)
                        {
                            if (expression[i + 1] == '-')
                            {
                                HandleToken(Token.Decrement, expression.Slice(i++, 2));
                                break;
                            }

                            if (expression[i + 1] == '=')
                            {
                                HandleToken(Token.AssignSub, expression.Slice(i++, 2));
                                break;
                            }
                        }

                        HandleToken(Token.Sub, expression.Slice(i, 1));
                        break;
                    case '/':
                        if (i + 1 < expression.Length && expression[i + 1] == '=')
                        {
                            HandleToken(Token.AssignDiv, expression.Slice(i++, 2));
                            break;
                        }

                        HandleToken(Token.Div, expression.Slice(i, 1));
                        break;
                    case '*':
                        if (i + 1 < expression.Length && expression[i + 1] == '=')
                        {
                            HandleToken(Token.AssignMul, expression.Slice(i++, 2));
                            break;
                        }

                        HandleToken(Token.Mul, expression.Slice(i, 1));
                        break;
                    case '=':
                        if (i + 1 < expression.Length && expression[i + 1] == '=')
                        {
                            HandleToken(Token.Eq, expression.Slice(i++, 2));
                            break;
                        }

                        HandleToken(Token.Assign, expression.Slice(i, 1));
                        break;
                    case '!':
                        if (i + 1 < expression.Length && expression[i + 1] == '=')
                        {
                            HandleToken(Token.Neq, expression.Slice(i++, 2));
                            break;
                        }

                        throw new InvalidOperationException($"Unexpected character '{expression[i]}' at {i} position.");
                    case '>':
                        if (i + 1 < expression.Length && expression[i + 1] == '=')
                        {
                            HandleToken(Token.Gte, expression.Slice(i++, 2));
                            break;
                        }

                        HandleToken(Token.Gt, expression.Slice(i, 1));
                        break;
                    case '<':
                        if (i + 1 < expression.Length && expression[i + 1] == '=')
                        {
                            HandleToken(Token.Lte, expression.Slice(i++, 2));
                            break;
                        }

                        HandleToken(Token.Lt, expression.Slice(i, 1));
                        break;
                    case '(':
                        HandleToken(Token.OpenGroup, expression.Slice(i, 1));
                        break;
                    case ')':
                        HandleToken(Token.CloseGroup, expression.Slice(i, 1));
                        break;
                    case '?':
                        HandleToken(Token.Condition, expression.Slice(i, 1));
                        break;
                    case ':':
                        HandleToken(Token.Otherwise, expression.Slice(i, 1));
                        break;
                    case char digit when IsDigit(digit):
                        int j = i;

                        while (i + 1 < expression.Length && IsDigit(expression[i + 1]))
                        {
                            i++;
                        }

                        HandleToken(Token.Number, expression.Slice(j, i - j + 1));
                        break;
                    case char alpha when IsVariable(alpha):
                        int k = i;

                        while (i + 1 < expression.Length && IsVariable(expression[i + 1]))
                        {
                            i++;
                        }

                        HandleToken(Token.Variable, expression.Slice(k, i - k + 1));
                        break;
                    case char ws when Char.IsWhiteSpace(ws):
                        break;
                    default:
                        throw new InvalidOperationException($"Unexpected character '{expression[i]}' at {i} position.");
                }
            }

            Complete();
        }

        private void HandleToken(Token token, ReadOnlySpan<char> value)
        {
            if (this.pendingOperator)
            {
                HandleOperator(token);
            }
            else
            {
                HandleOperand(token, value);
            }
        }

        private void HandleOperand(Token token, ReadOnlySpan<char> value)
        {
            switch (token)
            {
                case Token.Variable:
                    reducer.Push(value.ToString());
                    this.pendingOperator = true;
                    break;
                case Token.Number:
                    reducer.Push(Int32.Parse(value));
                    this.pendingOperator = true;
                    break;
                case Token.Add:
                    break;
                case Token.Sub:
                case Token.Increment:
                case Token.Decrement:
                case Token.OpenGroup:
                    operators.Push(CreateOperator(token, prefix: true));
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected token '{value}' of {token} type.");
            }
        }

        private void HandleOperator(Token token)
        {
            if (token == Token.CloseGroup)
            {
                Pop(Token.OpenGroup); // reduce to the nearest '('.
                return;
            }

            var op = CreateOperator(token, prefix: false);

            switch (op.Order)
            {
                case OperatorOrder.Postfix:
                    reducer.Reduce(op.Code); // highest precedence.
                    break;
                case OperatorOrder.Additive:
                case OperatorOrder.Multiplicative:
                case OperatorOrder.Equality:
                    Pop(op.Order); // reduce all pending operations with higher precedence.
                    operators.Push(op);
                    this.pendingOperator = false;
                    break;
                case OperatorOrder.Conditional:
                    if (token == Token.Condition)
                    {
                        Pop(OperatorOrder.Conditional - 1); // because ?: is right associative.
                        operators.Push(new Operator(token, OperatorOrder.Max, OperationCode.None));
                    }
                    else
                    {
                        Pop(Token.Condition);
                        operators.Push(new Operator(token, OperatorOrder.Conditional, OperationCode.Switch));
                    }

                    this.pendingOperator = false;
                    break;
                case OperatorOrder.Assignment:
                    Pop(op.Order - 1); // because all assigments are right associative as well.
                    operators.Push(op);
                    this.pendingOperator = false;
                    break;
            }
        }

        private Operator CreateOperator(Token token, bool prefix) => token switch
        {
            Token.OpenGroup => new Operator(token, OperatorOrder.Max, OperationCode.None),
            Token.CloseGroup => new Operator(token, OperatorOrder.Max, OperationCode.None),
            Token.Increment => prefix
                ? new Operator(token, OperatorOrder.Prefix, OperationCode.PrefixIncrement)
                : new Operator(token, OperatorOrder.Postfix, OperationCode.PostfixIncrement),
            Token.Decrement => prefix
                ? new Operator(token, OperatorOrder.Prefix, OperationCode.PrefixDecrement)
                : new Operator(token, OperatorOrder.Postfix, OperationCode.PostfixDecrement),
            Token.Add => prefix
                ? new Operator(token, OperatorOrder.Prefix, OperationCode.None)
                : new Operator(token, OperatorOrder.Additive, OperationCode.Add),
            Token.Sub => prefix
                ? new Operator(token, OperatorOrder.Prefix, OperationCode.Negate)
                : new Operator(token, OperatorOrder.Additive, OperationCode.Subtract),
            Token.Div => new Operator(token, OperatorOrder.Multiplicative, OperationCode.Divide),
            Token.Mul => new Operator(token, OperatorOrder.Multiplicative, OperationCode.Multiply),
            Token.Eq => new Operator(token, OperatorOrder.Equality, OperationCode.CompareEq),
            Token.Neq => new Operator(token, OperatorOrder.Equality, OperationCode.CompareNotEq),
            Token.Gt => new Operator(token, OperatorOrder.Equality, OperationCode.CompareGreater),
            Token.Gte => new Operator(token, OperatorOrder.Equality, OperationCode.CompareGreaterEq),
            Token.Lt => new Operator(token, OperatorOrder.Equality, OperationCode.CompareLess),
            Token.Lte => new Operator(token, OperatorOrder.Equality, OperationCode.CompareLessEq),
            Token.Condition => new Operator(token, OperatorOrder.Conditional, OperationCode.Switch),
            Token.Otherwise => new Operator(token, OperatorOrder.Conditional, OperationCode.Switch),
            Token.Assign => new Operator(token, OperatorOrder.Assignment, OperationCode.Assign),
            Token.AssignAdd => new Operator(token, OperatorOrder.Assignment, OperationCode.AssignAdd),
            Token.AssignSub => new Operator(token, OperatorOrder.Assignment, OperationCode.AssignSub),
            Token.AssignMul => new Operator(token, OperatorOrder.Assignment, OperationCode.AssignMul),
            Token.AssignDiv => new Operator(token, OperatorOrder.Assignment, OperationCode.AssignDiv),
            _ => throw new NotImplementedException()
        };

        private void Pop(Token stop)
        {
            while (operators.TryPeek(out var top) && top.Token != stop)
            {
                reducer.Reduce(operators.Pop().Code);
            }

            if (operators.Count == 0)
            {
                throw new InvalidOperationException($"Invalid expression. {stop} token expected on the stack.");
            }

            operators.Pop();
        }

        private void Pop(OperatorOrder maxOrder)
        {
            while (operators.TryPeek(out var top) && top.Order <= maxOrder)
            {
                reducer.Reduce(operators.Pop().Code);
            }
        }

        private void Complete()
        {
            Pop(OperatorOrder.Max - 1); // reduce anything except '?' and '('.

            if (operators.Count > 0)
            {
                var top = operators.Pop();
                throw new InvalidOperationException($"Invalid expression. '{top.Token}' has no corresponding match.");
            }
        }


        private static bool IsDigit(char ch) => ch >= '0' && ch <= '9';
        private static bool IsVariable(char ch) => (ch >= 'a' && ch <= 'z') || ch == '_';
    }
}
