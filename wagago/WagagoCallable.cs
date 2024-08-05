namespace Wagago
{
    public interface IWagagoCallable
    {
        object Invocation(Interpreter interpreter, List<object> args);
    } 
}