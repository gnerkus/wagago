namespace Wagago
{
    /// <summary>
    /// runtime representation for a callable in Wagago
    /// 
    /// <para>allows for a programming object to be marked as a callable. This enables user-defined
    /// functions.</para>
    /// <para>each callable type has direct access to the interpreter so it can execute itself</para>
    /// </summary>
    public interface IWagagoCallable
    {
        int Arity();
        object Invocation(Interpreter interpreter, List<object> args);
    }

    /// <summary>
    /// native clock function
    /// </summary>
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

    /// <summary>
    /// base for user-defined functions
    /// <para>executes the function body using the interpreter</para>
    /// </summary>
    public class WagagoFunction : IWagagoCallable
    {
        private readonly Func _declaration;
        private readonly Env _closure;
        private readonly bool _isInitializer;

        internal WagagoFunction(Func declaration, Env closure, bool isInitializer)
        {
            _declaration = declaration;
            _closure = closure;
            _isInitializer = isInitializer;
        }
        public int Arity()
        {
            return _declaration.FuncParams.Count;
        }

        /// <summary>
        ///     executes the code defined in a function statement's block and returns a result if any
        ///     <para>
        ///         also returns the class instance when the constructor is called (_isInitializer is true)
        ///     </para>
        /// </summary>
        /// <param name="interpreter"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public object Invocation(Interpreter interpreter, List<object> args)
        {
            // the function call gets its own environment (execution context), bound to the global as a child environment
            // this is to enable recursion
            var env = new Env(_closure);
            for (var i = 0; i < _declaration.FuncParams.Count; i++)
            {
                // store each argument in the environment under the associated param name
                env.Define(_declaration.FuncParams[i].lexeme, args[i]);
            }

            try
            {
                interpreter.ExecuteBlock(_declaration.Body, env);
            }
            catch (ReturnException returnValue)
            {
                if (_isInitializer) return _closure.GetAt(0, "this");
                return returnValue.Value;
            }
            
            if (_isInitializer) return _closure.GetAt(0, "this");
            
            return null!;
        }

        public WagagoFunction Bind(WagagoInstance instance)
        {
            var env = new Env(_closure);
            env.Define("this", instance);
            return new WagagoFunction(_declaration, env, _isInitializer);
        }

        public override string ToString()
        {
            return $"<fn {_declaration.Name.lexeme} >";
        }
    }
}