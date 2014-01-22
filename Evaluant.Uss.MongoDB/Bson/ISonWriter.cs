using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.MongoDB.Bson
{
    public interface ISonWriter
    {
        void WriteString(string p);

        void WriteValue(BsonDataType bsonDataType, Object obj);

        void Write(Evaluant.Uss.Domain.Entity entity);
    }
}
