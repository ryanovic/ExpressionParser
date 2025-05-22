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
            Tokenizer.Tokenize(expression, HandleToken);
            CompletePendingOperations();
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

        private void CompletePendingOperations()
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
    }
}
