namespace ExpressionParser
{
    /// <summary>
    /// Determines the order in which operators are evaluated (lower order means higher precedence). 
    /// </summary>
    public enum OperatorOrder
    {
        Postfix, // x++, x--
        Prefix,  // --x, -x,
        Multiplicative,
        Additive,
        Relational,
        Equality,
        Conditional, // ?:
        Assignment,
        Max, // Special case for '(' and '?', which do not represent a complete operation on th stack and and should not be reduced by any other operators.
    }
}
