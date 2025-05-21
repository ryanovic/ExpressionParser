namespace ExpressionParser
{
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
        Max, // '(' and '?' that are not supposed to be reduced by any other operators
    }
}
