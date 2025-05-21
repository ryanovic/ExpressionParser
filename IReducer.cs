namespace ExpressionParser
{
    public interface IReducer
    {
        void Push(int value);
        void Push(string variable);
        public void Reduce(OperationCode op);
    }
}
