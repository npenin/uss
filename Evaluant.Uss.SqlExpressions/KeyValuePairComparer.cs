using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss
{
    class KeyValuePairComparer<TKey, TValue> : EqualityComparer<KeyValuePair<TKey, TValue>>
    {
        public override bool Equals(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
        {
            if (EqualityComparer<TKey>.Default.Equals(x.Key, y.Key))
                return true;
            return EqualityComparer<TValue>.Default.Equals(x.Value, y.Value);
        }

        public override int GetHashCode(KeyValuePair<TKey, TValue> obj)
        {
            return obj.GetHashCode();
        }
    }
}
