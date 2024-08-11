namespace Wagago
{
    public class WagagoClass: IWagagoCallable
    {
        public readonly string Name;

        public WagagoClass(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }

        public int Arity()
        {
            return 0;
        }

        public object Invocation(Interpreter interpreter, List<object> args)
        {
            var instance = new WagagoInstance(this);
            return instance;
        }
    }
}