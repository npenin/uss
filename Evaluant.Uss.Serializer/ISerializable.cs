using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Evaluant.Uss.Serializer
{
    public interface ISerializable
    {
        void WriteTo(BinaryWriter wsriter);

        void ReadFrom(BinaryReader reader);
    }
}
