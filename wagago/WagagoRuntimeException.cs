namespace Wagago
{
    public class WagagoRuntimeException : SystemException
    {
        public readonly Token Token;

        public WagagoRuntimeException(Token token, string message) : base(message)
        {
            Token = token;
        }
    }
}