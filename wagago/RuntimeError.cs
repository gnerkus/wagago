namespace Wagago
{
    public class RuntimeError : SystemException
    {
        public readonly Token Token;

        public RuntimeError(Token token, string message) : base(message)
        {
            Token = token;
        }
    }
}