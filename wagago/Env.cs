namespace Wagago
{
    public class Env
    {
        private readonly Env? _enclosing;
        private readonly Dictionary<string, object> _values = new ();

        public Env()
        {
            _enclosing = null;
        }

        public Env(Env? enclosing)
        {
            _enclosing = enclosing;
        }
        
        public void Define(string name, object value)
        {
            _values.Add(name, value);
        }

        /// <summary>
        /// Fetch a variable's value by name
        /// <para>A missing variable is a runtime (not syntax) error
        /// as part of Wagago's design
        /// </para>
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        /// <exception cref="RuntimeError"></exception>
        public object Get(Token name)
        {
            if (_values.ContainsKey(name.lexeme))
            {
                return _values[name.lexeme];
            }

            if (_enclosing != null)
            {
                return _enclosing.Get(name);
            }

            throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
        }

        public void Assign(Token name, object value)
        {
            if (_values.ContainsKey(name.lexeme))
            {
                _values[name.lexeme] = value;
                return;
            }

            if (_enclosing == null)
                throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
            _enclosing.Assign(name, value);
        }

        public object GetAt(int distance, string exprNameLexeme)
        {
            return Ancestor(distance)._values[exprNameLexeme];
        }

        // TODO: check if we can use the same ancestor if no enclosing
        private Env Ancestor(int distance)
        {
            var ancestor = this;
            for (var i = 0; i < distance; i++)
            {
                ancestor = ancestor._enclosing ?? ancestor;
            }

            return ancestor;
        }

        public void AssignAt(int distance, Token exprIdentifier, object value)
        {
            Ancestor(distance)._values[exprIdentifier.lexeme] = value;
        }
    }
}