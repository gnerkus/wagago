namespace Wagago
{
    /// <summary>
    /// runtime representation for a callable in Wagago
    /// 
    /// <p>allows for a programming object to be marked as a callable. This enables user-defined
    /// functions.</p>
    /// </summary>
    public interface IWagagoCallable
    {
        int Arity();
        object Invocation(Interpreter interpreter, List<object> args);
    }

    public class WagagoClock : IWagagoCallable
    {
        public int Arity()
        {
            return 0;
        }

        public object Invocation(Interpreter interpreter, List<object> args)
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000.0;
        }

        public override string ToString()
        {
            return "<native fn>";
        }
    }
}