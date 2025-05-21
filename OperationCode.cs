namespace ExpressionParser
{
    public enum OperationCode
    {
        None,
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
        Switch,     // ?:
        Assign,
        AssignAdd,
        AssignSub,
        AssignMul,
        AssignDiv,
    }
}
