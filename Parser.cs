namespace ExpressionParser
{
    public class Parser
    {
        private readonly record struct Operator(Token Token, OperationPrecedence Precedence);

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

                        HandleToken(Token.Subtract, expression.Slice(i, 1));
                        break;
                    case '/':
                        if (i + 1 < expression.Length && expression[i + 1] == '=')
                        {
                            HandleToken(Token.AssignDiv, expression.Slice(i++, 2));
                            break;
                        }

                        HandleToken(Token.Divide, expression.Slice(i, 1));
                        break;
                    case '*':
                        if (i + 1 < expression.Length && expression[i + 1] == '=')
                        {
                            HandleToken(Token.AssignMul, expression.Slice(i++, 2));
                            break;
                        }

                        HandleToken(Token.Multiply, expression.Slice(i, 1));
                        break;
                    case '=':
                        if (i + 1 < expression.Length && expression[i + 1] == '=')
                        {
                            HandleToken(Token.Equal, expression.Slice(i++, 2));
                            break;
                        }

                        HandleToken(Token.Assign, expression.Slice(i, 1));
                        break;
                    case '!':
                        if (i + 1 < expression.Length && expression[i + 1] == '=')
                        {
                            HandleToken(Token.NotEqual, expression.Slice(i++, 2));
                            break;
                        }

                        throw new InvalidOperationException($"Unexpected character '{expression[i]}' at {i} position.");
                    case '>':
                        if (i + 1 < expression.Length && expression[i + 1] == '=')
                        {
                            HandleToken(Token.GreaterEqual, expression.Slice(i++, 2));
                            break;
                        }

                        HandleToken(Token.Greater, expression.Slice(i, 1));
                        break;
                    case '<':
                        if (i + 1 < expression.Length && expression[i + 1] == '=')
                        {
                            HandleToken(Token.LessEqual, expression.Slice(i++, 2));
                            break;
                        }

                        HandleToken(Token.Less, expression.Slice(i, 1));
                        break;
                    case '(':
                        HandleToken(Token.GroupBegin, expression.Slice(i, 1));
                        break;
                    case ')':
                        HandleToken(Token.GroupEnd, expression.Slice(i, 1));
                        break;
                    case '?':
                        HandleToken(Token.SelectBegin, expression.Slice(i, 1));
                        break;
                    case ':':
                        HandleToken(Token.SelectEnd, expression.Slice(i, 1));
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

            CompleteAll();
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
                    break;
                case Token.Number:
                    reducer.Push(Int32.Parse(value));
                    break;
                case Token.Add:
                    break;
                case Token.Subtract:
                case Token.Increment:
                case Token.Decrement:
                    operators.Push(new Operator(token, OperationPrecedence.Prefix));
                    break;
                case Token.GroupBegin:
                    operators.Push(new Operator(token, OperationPrecedence.Partial));
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected token '{value}' of {token} type.");
            }

            this.pendingOperator = token == Token.Number || token == Token.Variable;
        }

        private void HandleOperator(Token token)
        {
            this.pendingOperator = false;

            switch (token)
            {
                case Token.GroupEnd:
                    CompletePartialOperation(Token.GroupBegin);
                    this.pendingOperator = true;
                    break;
                case Token.Increment:
                case Token.Decrement:
                    operators.Push(new Operator(token, OperationPrecedence.Postfix));
                    this.pendingOperator = true;
                    break;
                case Token.Multiply:
                case Token.Divide:
                    CompleteOperations(OperationPrecedence.Multiplicative);
                    operators.Push(new Operator(token, OperationPrecedence.Multiplicative));
                    break;
                case Token.Add:
                case Token.Subtract:
                    CompleteOperations(OperationPrecedence.Additive);
                    operators.Push(new Operator(token, OperationPrecedence.Additive));
                    break;
                case Token.Greater:
                case Token.GreaterEqual:
                case Token.Less:
                case Token.LessEqual:
                    CompleteOperations(OperationPrecedence.Relational);
                    operators.Push(new Operator(token, OperationPrecedence.Relational));
                    break;
                case Token.Equal:
                case Token.NotEqual:
                    CompleteOperations(OperationPrecedence.Equality);
                    operators.Push(new Operator(token, OperationPrecedence.Equality));
                    break;
                case Token.SelectBegin: // ?
                    CompleteOperations(OperationPrecedence.Conditional - 1); // respect right associativity.
                    operators.Push(new Operator(token, OperationPrecedence.Partial)); // unspecified since is not yet completed.
                    break;
                case Token.SelectEnd:   // :
                    CompletePartialOperation(Token.SelectBegin);
                    operators.Push(new Operator(token, OperationPrecedence.Conditional));
                    break;
                case Token.Assign:
                case Token.AssignAdd:
                case Token.AssignSub:
                case Token.AssignDiv:
                case Token.AssignMul:
                    CompleteOperations(OperationPrecedence.Assignment - 1); // respect right associativity.
                    operators.Push(new Operator(token, OperationPrecedence.Assignment));
                    break;
                default:
                    throw new InvalidOperationException($"Unexpected token '{token}'");
            }
        }

        private void CompletePartialOperation(Token start)
        {
            while (operators.TryPeek(out var top) && top.Token != start)
            {
                CompleteOperation(operators.Pop());
            }

            if (operators.Count == 0)
            {
                throw new InvalidOperationException($"Invalid expression. {start} token expected on the stack.");
            }

            operators.Pop();
        }

        private void CompleteOperations(OperationPrecedence precedence)
        {
            while (operators.TryPeek(out var top) && top.Precedence <= precedence)
            {
                CompleteOperation(operators.Pop());
            }
        }

        private void CompleteAll()
        {
            CompleteOperations(OperationPrecedence.Partial - 1); // reduce anything except '?' and '('.

            if (operators.Count > 0)
            {
                var top = operators.Pop();
                throw new InvalidOperationException($"Invalid expression. '{top.Token}' has no corresponding match.");
            }
        }

        private void CompleteOperation(Operator op)
        {
            reducer.Reduce(op.Token switch
            {
                Token.Increment => op.Precedence == OperationPrecedence.Prefix ? OperationCode.PrefixIncrement : OperationCode.PostfixIncrement,
                Token.Decrement => op.Precedence == OperationPrecedence.Prefix ? OperationCode.PrefixDecrement : OperationCode.PostfixDecrement,
                Token.Add => OperationCode.Add,
                Token.Subtract => op.Precedence == OperationPrecedence.Prefix ? OperationCode.Negate : OperationCode.Subtract,
                Token.Divide => OperationCode.Divide,
                Token.Multiply => OperationCode.Multiply,
                Token.Equal => OperationCode.CompareEq,
                Token.NotEqual => OperationCode.CompareNotEq,
                Token.Greater => OperationCode.CompareGreater,
                Token.GreaterEqual => OperationCode.CompareGreaterEq,
                Token.Less => OperationCode.CompareLess,
                Token.LessEqual => OperationCode.CompareLessEq,
                Token.SelectEnd => OperationCode.Select,
                Token.Assign => OperationCode.Assign,
                Token.AssignAdd => OperationCode.AssignAdd,
                Token.AssignSub => OperationCode.AssignSub,
                Token.AssignMul => OperationCode.AssignMul,
                Token.AssignDiv => OperationCode.AssignDiv,
                _ => throw new NotImplementedException(),
            });
        }

        private static bool IsDigit(char ch) => ch >= '0' && ch <= '9';
        private static bool IsVariable(char ch) => (ch >= 'a' && ch <= 'z') || ch == '_';
    }
}
