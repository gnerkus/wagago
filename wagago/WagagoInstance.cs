namespace Wagago
{
    public class WagagoInstance
    {
        private readonly WagagoClass _klass;

        public WagagoInstance(WagagoClass klass)
        {
            _klass = klass;
        }

        public override string ToString()
        {
            return _klass.Name + " instance";
        }
    }
}