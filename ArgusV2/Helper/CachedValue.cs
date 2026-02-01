using System;

namespace IngameScript.Helper
{

    public struct CachedValue<T>
    {
        private T _value;
        private bool _valid;
        private readonly Func<T> _getter;

        public CachedValue(Func<T> getter)
        {
            if (getter == null)
                throw new ArgumentNullException("getter");

            _getter = getter;
            _value = default(T);
            _valid = false;
        }

        public T Value
        {
            get
            {
                if (!_valid)
                {
                    _value = _getter();
                    _valid = true;
                }
                return _value;
            }
        }

        public void Invalidate() => _valid = false;
    }

}