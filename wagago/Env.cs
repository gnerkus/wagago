namespace Wagago
{
    public class Env
    {
        private readonly Dictionary<string, object> _values = new ();

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

            throw new RuntimeError(name, $"Undefined variable '{name.lexeme}'.");
        }
    }
}