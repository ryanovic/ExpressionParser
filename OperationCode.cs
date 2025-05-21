namespace ExpressionParser
{
    public enum OperationCode
    {
        PrefixIncrement,
        PrefixDecrement,
        PostfixIncrement,
        PostfixDecrement,
        Negate,
        Add,
        Subtract,
        Divide,
        Multiply,
        CompareEq,
        CompareNotEq,
        CompareGreater,
        CompareGreaterEq,
        CompareLess,
        CompareLessEq,
        Select, // ?: conditional operator.
        Assign,
        AssignAdd,
        AssignSub,
        AssignMul,
        AssignDiv,
    }
}
