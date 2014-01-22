using System.Collections.Generic;
using System.IO;
using Evaluant.Uss.MongoDB.Bson;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.MongoDB.Protocol
{
    public class ReplyMessage:MessageBase
    {
        // normally zero, non-zero on query failure     
        public int ResponseFlag { get; set; }

        // id of the cursor created for this query response 
        public long CursorID { get; set; }

        // indicates where in the cursor this reply is starting     
        public int StartingFrom { get; set; }

        // number of documents in the reply       
        public int NumberReturned { get; set; }

        public Entity[] Documents { get; set; }

        public void Read(Stream stream){
            stream = new BufferedStream(stream, 256);
            BinaryReader reader = new BinaryReader(stream);
            this.Header = ReadHeader(reader);
            this.ResponseFlag = reader.ReadInt32();
            this.CursorID = reader.ReadInt64();
            this.StartingFrom = reader.ReadInt32();
            this.NumberReturned = reader.ReadInt32();            
            
            BsonReader breader = new BsonReader(stream);
            List<Entity> docs = new List<Entity>();
            for(int num = 0; num < this.NumberReturned; num++){
                docs.Add(breader.Read());
            }
            this.Documents = docs.ToArray();            
        }

        protected MessageHeader ReadHeader(BinaryReader reader){
            MessageHeader hdr = new MessageHeader(OpCode.Reply);
            hdr.MessageLength = reader.ReadInt32();
            hdr.RequestId = reader.ReadInt32();
            hdr.ResponseTo = reader.ReadInt32();
            int op = reader.ReadInt32();
            if((OpCode)op != OpCode.Reply) throw new InvalidDataException("Should have been a reply but wasn't");
            return hdr;
        }
        
//        public void Read(Stream stream){        
//            /* Used during debugging of the stream.
//            BsonReader headerreader = new BsonReader(stream);
//            this.Header = ReadHeader(headerreader);
//            
//            //buffer the whole response into a memorystream for debugging.
//            MemoryStream buffer = new MemoryStream();
//            BinaryReader buffReader = new BinaryReader(stream);
//            BinaryWriter buffWriter = new BinaryWriter(buffer);
//            byte[] body = buffReader.ReadBytes(this.Header.MessageLength - 16);
//            System.Console.WriteLine(BitConverter.ToString(body));
//            buffWriter.Write(body);
//            buffer.Seek(0, SeekOrigin.Begin);
//            
//            BsonReader reader = new BsonReader(buffer);*/
//            
//            //BsonReader reader = new BsonReader(stream);
//            //BsonReader reader = new BsonReader(new BufferedStream(stream));
//            BsonReader reader = new BsonReader(new BufferedStream(stream, 4 * 1024));
//            this.Header = ReadHeader(reader);
//            
//            this.ResponseFlag = reader.ReadInt32();
//            this.CursorID = reader.ReadInt64();
//            this.StartingFrom = reader.ReadInt32();
//            this.NumberReturned = reader.ReadInt32();
//
//            List<BsonDocument> docs = new List<BsonDocument>();
//            for(int num = 0; num < this.NumberReturned; num++){
//                BsonDocument doc = new BsonDocument();
//                doc.Read(reader);
//                docs.Add(doc);
//            }
//            this.Documents = docs.ToArray();
//        }
//        
//        protected MessageHeader ReadHeader(BsonReader reader){
//            MessageHeader hdr = new MessageHeader(OpCode.Reply);
//            hdr.MessageLength = reader.ReadInt32();
//            hdr.RequestId = reader.ReadInt32();
//            hdr.ResponseTo = reader.ReadInt32();
//            int op = reader.ReadInt32();
//            if((OpCode)op != OpCode.Reply) throw new InvalidDataException("Should have been a reply but wasn't");
//            return hdr;
//        }
    }
}
