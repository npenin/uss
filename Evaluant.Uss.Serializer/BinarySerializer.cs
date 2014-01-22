using System;
using System.Collections.Generic;
using System.Text;
//using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Evaluant.Uss.Serializer
{
    public class BinarySerializer : ISerializer
    {
        #region ISerializer Members

        public byte[] SerializeToArray(object o)
        {
            if (o == null)
                return null;

            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, o);
                return ms.ToArray();
            }
        }

        public string SerializeToString(object o)
        {
            if (o == null)
                return null;

            return Convert.ToBase64String(SerializeToArray(o));
        }

        public object Unserialize(byte[] bytes)
        {
            if (bytes == null)
                return null;

            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                ms.Write(bytes, 0, bytes.Length);
                ms.Position = 0;
                return bf.Deserialize(ms);
            }
        }

        public object Unserialize(string s)
        {
            if (s == null)
                return null;

            return Unserialize(Convert.FromBase64String(s));
        }

        #endregion

        #region ISerializer Members

        public byte[] SerializeToArray(string key, object o)
        {
            return SerializeToArray(o);
        }

        public string SerializeToString(string key, object o)
        {
            return SerializeToString(o);
        }

        public object Unserialize(string key, byte[] array)
        {
            return Unserialize(array);
        }

        public object Unserialize(string key, string s)
        {
            return Unserialize(s);
        }

        #endregion
    }
}
