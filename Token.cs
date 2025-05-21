namespace ExpressionParser
{
    public enum Token
    {
        Number,
        Variable,
        OpenGroup,  // (
        CloseGroup, // )
        Increment,
        Decrement,
        Add,
        Sub,
        Div,
        Mul,
        Eq,
        Neq,
        Gt,
        Gte,
        Lt,
        Lte,
        Condition,     // ?
        Otherwise, // :
        Assign,
        AssignAdd,
        AssignSub,
        AssignMul,
        AssignDiv,
    }
}
