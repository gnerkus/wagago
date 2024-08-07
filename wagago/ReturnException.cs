namespace Wagago
{
    public class ReturnException: SystemException
    {
        public readonly object Value;

        public ReturnException(object value): base("", null)
        {
            Value = value;
        }
    }
}