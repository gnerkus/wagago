namespace Wagago
{
    public class WagagoClass
    {
        readonly string _name;

        public WagagoClass(string name)
        {
            _name = name;
        }

        public override string ToString()
        {
            return _name;
        }
    }
}