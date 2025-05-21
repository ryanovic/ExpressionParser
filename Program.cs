namespace ExpressionParser
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("""                
                Sample implementation of operator-precedence expression parser. The following lexemes and operations are supported:

                - Numbers               One or more '0'-'9' characters.Limited  to 32 bit integer values.
                - Variables             One or more 'a'-'z' and '_' characters.
                - Grouping              ((x + y) * z)
                - Prefix operations     -x, --x, ++x
                - Postfix operations    x++, x--
                - Multiplicative        x * y, x / y
                - Additive              x + y, x - y
                - Relational            x > y, x < y, x >= y, x <= y
                - Equality              x == y, x != y
                - Conditional           Ternary ?: operator. Expression that evaluates to a positive integer will be considered a true condition.
                - Assignment            x = y, x += y, x -= y, x *= y, x /= y

                """);

            Console.WriteLine("Please, enter the expression:");
            string? expression = Console.ReadLine();
            var variables = new Dictionary<string, int>();

            while (!string.IsNullOrEmpty(expression))
            {
                Process(expression, variables);
                expression = Console.ReadLine();
            }
        }

        static void Process(string expression, Dictionary<string, int> variables)
        {
            try
            {
                var reducer = new ExpressionEvaluator(variables);
                var parser = new Parser(reducer);
                parser.Parse(expression);
                Console.WriteLine($"Result: {reducer.GetResult()}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to evaluated with the following error: " + ex.Message);
            }
        }
    }
}
