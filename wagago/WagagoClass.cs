namespace Wagago
{
    public class WagagoClass: IWagagoCallable
    {
        public readonly string Name;
        public readonly WagagoClass? SuperClass;
        private readonly Dictionary<string, WagagoFunction> _methods;

        public WagagoClass(string name, WagagoClass? superClass, Dictionary<string, WagagoFunction> methods)
        {
            Name = name;
            SuperClass = superClass;
            _methods = methods;
        }

        public override string ToString()
        {
            return Name;
        }

        public int Arity()
        {
            var initializer = FindMethod("init");
            return initializer?.Arity() ?? 0;
        }

        public object Invocation(Interpreter interpreter, List<object> args)
        {
            var instance = new WagagoInstance(this);

            var initializer = FindMethod("init");
            initializer?.Bind(instance).Invocation(interpreter, args);

            return instance;
        }

        public WagagoFunction? FindMethod(string name)
        {
            return _methods.TryGetValue(name, out var method) ? method : SuperClass?.FindMethod(name);
        }
    }
}