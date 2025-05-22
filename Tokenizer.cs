namespace ExpressionParser
{
    public static class Tokenizer
    {
        public static void Tokenize(ReadOnlySpan<char> expression, Action<Token, ReadOnlySpan<char>> next)
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
                                next(Token.Increment, expression.Slice(i++, 2));
                                break;
                            }

                            if (expression[i + 1] == '=')
                            {
                                next(Token.AssignAdd, expression.Slice(i++, 2));
                                break;
                            }
                        }

                        next(Token.Add, expression.Slice(i, 1));
                        break;
                    case '-':
                        if (i + 1 < expression.Length)
                        {
                            if (expression[i + 1] == '-')
                            {
                                next(Token.Decrement, expression.Slice(i++, 2));
                                break;
                            }

                            if (expression[i + 1] == '=')
                            {
                                next(Token.AssignSub, expression.Slice(i++, 2));
                                break;
                            }
                        }

                        next(Token.Subtract, expression.Slice(i, 1));
                        break;
                    case '/':
                        if (i + 1 < expression.Length && expression[i + 1] == '=')
                        {
                            next(Token.AssignDiv, expression.Slice(i++, 2));
                            break;
                        }

                        next(Token.Divide, expression.Slice(i, 1));
                        break;
                    case '*':
                        if (i + 1 < expression.Length && expression[i + 1] == '=')
                        {
                            next(Token.AssignMul, expression.Slice(i++, 2));
                            break;
                        }

                        next(Token.Multiply, expression.Slice(i, 1));
                        break;
                    case '=':
                        if (i + 1 < expression.Length && expression[i + 1] == '=')
                        {
                            next(Token.Equal, expression.Slice(i++, 2));
                            break;
                        }

                        next(Token.Assign, expression.Slice(i, 1));
                        break;
                    case '!':
                        if (i + 1 < expression.Length && expression[i + 1] == '=')
                        {
                            next(Token.NotEqual, expression.Slice(i++, 2));
                            break;
                        }

                        throw new InvalidOperationException($"Unexpected character '{expression[i]}' at {i} position.");
                    case '>':
                        if (i + 1 < expression.Length && expression[i + 1] == '=')
                        {
                            next(Token.GreaterEqual, expression.Slice(i++, 2));
                            break;
                        }

                        next(Token.Greater, expression.Slice(i, 1));
                        break;
                    case '<':
                        if (i + 1 < expression.Length && expression[i + 1] == '=')
                        {
                            next(Token.LessEqual, expression.Slice(i++, 2));
                            break;
                        }

                        next(Token.Less, expression.Slice(i, 1));
                        break;
                    case '(':
                        next(Token.GroupBegin, expression.Slice(i, 1));
                        break;
                    case ')':
                        next(Token.GroupEnd, expression.Slice(i, 1));
                        break;
                    case '?':
                        next(Token.SelectBegin, expression.Slice(i, 1));
                        break;
                    case ':':
                        next(Token.SelectEnd, expression.Slice(i, 1));
                        break;
                    case char digit when IsDigit(digit):
                        int j = i;

                        while (i + 1 < expression.Length && IsDigit(expression[i + 1]))
                        {
                            i++;
                        }

                        next(Token.Number, expression.Slice(j, i - j + 1));
                        break;
                    case char alpha when IsVariable(alpha):
                        int k = i;

                        while (i + 1 < expression.Length && IsVariable(expression[i + 1]))
                        {
                            i++;
                        }

                        next(Token.Variable, expression.Slice(k, i - k + 1));
                        break;
                    case char ws when Char.IsWhiteSpace(ws):
                        break;
                    default:
                        throw new InvalidOperationException($"Unexpected character '{expression[i]}' at {i} position.");
                }
            }
        }

        private static bool IsDigit(char ch) => ch >= '0' && ch <= '9';
        private static bool IsVariable(char ch) => (ch >= 'a' && ch <= 'z') || ch == '_';
    }
}
