namespace ExpressionParser
{
    internal class Program
    {
        static void Main(string[] args)
        {
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
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
