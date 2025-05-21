namespace ExpressionParser
{
    /// <summary>
    /// Determines the order in which operators are evaluated (lower order means higher precedence). 
    /// </summary>
    public enum OperationPrecedence
    {
        Postfix, // x++, x--
        Prefix,  // --x, -x,
        Multiplicative,
        Additive,
        Relational,
        Equality,
        Conditional, // ?:
        Assignment,
        Partial,
    }
}
