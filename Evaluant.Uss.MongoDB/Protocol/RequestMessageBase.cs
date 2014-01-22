using System.IO;
using Evaluant.Uss.MongoDB.Bson;
using System;

namespace Evaluant.Uss.MongoDB.Protocol
{
    /// <summary>
    /// Description of Message.
    /// </summary>
    public abstract class RequestMessageBase : MessageBase, IRequestMessage
    {

        public void Write(Stream stream)
        {
            Write(stream, null);
        }

        public void Write (Stream stream,TextWriter logger){
            MessageHeader header = this.Header;
            BufferedStream bstream = new BufferedStream(stream);
            BinaryWriter writer = new BinaryWriter(bstream);
            BsonWriter bwriter = new BsonWriter(bstream);
            
            Header.MessageLength += this.CalculateBodySize(bwriter);
            if(Header.MessageLength > MessageBase.MaximumMessageSize){
                throw new MongoException("Maximum message length exceeded");
            }
            
            writer.Write(header.MessageLength);
            writer.Write(header.RequestId);
            writer.Write(header.ResponseTo);
            writer.Write((int)header.OpCode);
            writer.Flush();
            WriteBody(bwriter);
            bwriter.Flush();
            if (logger != null)
            {
                Json.JsonWriter jwriter = new Evaluant.Uss.MongoDB.Json.JsonWriter(logger);
                WriteBody(jwriter);
                jwriter.WriteString(Environment.NewLine);
            }
        }
        
        protected abstract void WriteBody(ISonWriter writer);
        
        protected abstract int CalculateBodySize(BsonWriter writer);
    }
}
