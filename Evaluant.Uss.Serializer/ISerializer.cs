using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.Serializer
{
    public interface ISerializer
    {
        byte[] SerializeToArray(string key, object o);
        string SerializeToString(string key, object o);

        object Unserialize(string key, byte[] array);
        object Unserialize(string key, string s);


        byte[] SerializeToArray(object o);
        string SerializeToString(object o);

        object Unserialize(byte[] array);
        object Unserialize(string s);
    }
}
