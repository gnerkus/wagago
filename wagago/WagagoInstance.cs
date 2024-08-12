namespace Wagago
{
    public class WagagoInstance
    {
        private readonly WagagoClass _klass;
        private readonly Dictionary<string, object> _fields = new();

        public WagagoInstance(WagagoClass klass)
        {
            _klass = klass;
        }

        public object Get(Token name)
        {
            if (_fields.ContainsKey(name.lexeme))
            {
                return _fields[name.lexeme];
            }

            throw new RuntimeError(name, $"Undefined property '{name.lexeme}'.");
        }

        public void Set(Token name, object value)
        {
            _fields[name.lexeme] = value;
        }

        public override string ToString()
        {
            return _klass.Name + " instance";
        }
    }
}