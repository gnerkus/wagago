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
}