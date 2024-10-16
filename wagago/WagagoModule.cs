namespace Wagago
{
    public class WagagoModule
    {
        public readonly Token Name;
        private readonly Dictionary<string, WagagoFunction> _moduleFuncs;
        public WagagoModule(Token name, Dictionary<string, WagagoFunction> moduleFuncs)
        {
            Name = name;
            _moduleFuncs = moduleFuncs;
        }

        public override string ToString()
        {
            return $"'{Name.lexeme}' module";
        }

        public object Get(Token name)
        {
            var moduleFunc = _moduleFuncs[name.lexeme];

            if (moduleFunc != null) return moduleFunc;
            
            throw new WagagoRuntimeException(name, $"Undefined module function '{name.lexeme}'.");
        }
    }
}