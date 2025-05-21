namespace ExpressionParser
{
    public enum Token
    {
        Number,
        Variable,
        GroupBegin,  // (
        GroupEnd,    // )
        Increment,
        Decrement,
        Add,
        Subtract,
        Divide,
        Multiply,
        Equal,
        NotEqual,
        Greater,
        GreaterEqual,
        Less,
        LessEqual,
        SelectBegin,  // ?
        SelectEnd,    // :
        Assign,
        AssignAdd,
        AssignSub,
        AssignMul,
        AssignDiv,
    }
}
