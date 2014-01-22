using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.Serializer
{
    public class RouterSerializer : ISerializer
    {
        private ISerializer defaultSerializer;
        private Dictionary<string, ISerializer> serializers;

        public RouterSerializer(ISerializer defaultSerializer)
        {
            this.defaultSerializer = defaultSerializer;
            serializers = new Dictionary<string, ISerializer>();
        }

        public void Register(string key, ISerializer serializer)
        {
            serializers.Add(key, serializer);
        }

        #region ISerializer Members

        public byte[] SerializeToArray(object o)
        {
            return defaultSerializer.SerializeToArray(o);
        }

        public string SerializeToString(object o)
        {
            return defaultSerializer.SerializeToString(o);
        }

        public object Unserialize(byte[] array)
        {
            return defaultSerializer.Unserialize(array);
        }

        public object Unserialize(string s)
        {
            return defaultSerializer.Unserialize(s);
        }

        public byte[] SerializeToArray(string key, object o)
        {
            if (serializers.ContainsKey(key))
                return serializers[key].SerializeToArray(o);
            return defaultSerializer.SerializeToArray(o);
        }

        public string SerializeToString(string key, object o)
        {
            if (serializers.ContainsKey(key))
                return serializers[key].SerializeToString(o);
            return defaultSerializer.SerializeToString(o);
        }

        public object Unserialize(string key, byte[] array)
        {
            if (serializers.ContainsKey(key))
                return serializers[key].Unserialize(array);
            return Unserialize(array);
        }

        public object Unserialize(string key, string s)
        {
            if (serializers.ContainsKey(key))
                return serializers[key].Unserialize(s);
            return Unserialize(s);
        }

        #endregion
    }
}
