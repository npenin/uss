using System;
using System.Collections.Generic;
using System.IO;

using Evaluant.Uss.MongoDB.Bson;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.MongoDB.Protocol
{
    /// <summary>
    /// Description of InsertMessage.
    /// </summary>
    /// <remarks>
    ///      MsgHeader header;             // standard message header
    ///      int32     ZERO;               // 0 - reserved for future use
    ///      cstring   fullCollectionName; // "dbname.collectionname"
    ///      BSON[]    documents;          // one or more documents to insert into the collection
    /// </remarks>
    public class InsertMessage : MessageBase, IRequestMessage
    {
        protected struct MessageChunk
        {
            public int Size;
            public List<Entity> Documents;
        }

        public string FullCollectionName { get; set; }

        public List<Entity> Documents { get; set; }

        private List<MessageChunk> chunks = new List<MessageChunk>();

        public InsertMessage()
        {
            this.Header = new MessageHeader(OpCode.Insert);
            Documents = new List<Entity>();
        }

        public void Write(Stream stream)
        {
            Write(stream, null);
        }

        public void Write(Stream stream, TextWriter logger)
        {
            MessageHeader header = this.Header;
            BufferedStream bstream = new BufferedStream(stream);

            BsonWriter bwriter = new BsonWriter(bstream);
            ChunkMessage(bwriter);

            foreach (MessageChunk chunk in chunks)
            {
                WriteChunk(bstream, chunk);
            }

            if (logger != null)
            {
                Json.JsonWriter writer = new Evaluant.Uss.MongoDB.Json.JsonWriter(logger);
                foreach(var chunk in chunks)
                {
                    foreach(var entity in chunk.Documents)
                    {
                        writer.Write(entity);
                    }
                }
            }
        }

        /// <summary>
        /// Breaks down an insert message that may be too large into managable sizes.  
        /// When inserting only one document there will be only one chunk.  However chances
        /// are that when inserting thousands of documents at once there will be many.
        /// </summary>
        protected void ChunkMessage(BsonWriter writer)
        {
            int baseSize = CalculateBaseSize(writer);

            MessageChunk chunk = new MessageChunk() { Size = baseSize, Documents = new List<Entity>() };
            foreach (Entity doc in this.Documents)
            {
                int docSize = writer.CalculateSize(doc);
                if (docSize + baseSize >= MessageBase.MaximumMessageSize) throw new MongoException("Document is too big to fit in a message.");

                if (docSize + chunk.Size > MessageBase.MaximumMessageSize)
                {
                    chunks.Add(chunk);
                    chunk = new MessageChunk() { Size = baseSize, Documents = new List<Entity>() };
                }
                chunk.Documents.Add(doc);
                chunk.Size += docSize;
            }
            chunks.Add(chunk);
        }

        /// <summary>
        /// The base size that all chunks will have.
        /// </summary>
        protected int CalculateBaseSize(BsonWriter writer)
        {
            int size = 4; //first int32
            size += writer.CalculateSize(this.FullCollectionName, false);
            size += Header.MessageLength;
            return size;
        }

        /// <summary>
        /// Writes out a header and the chunk of documents.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="chunk"></param>
        protected void WriteChunk(Stream stream, MessageChunk chunk)
        {
            WriteHeader(new BinaryWriter(stream), chunk.Size);

            BsonWriter writer = new BsonWriter(stream);
            writer.WriteValue(BsonDataType.Integer, 0);
            writer.WriteString(this.FullCollectionName);

            foreach (Entity doc in chunk.Documents)
            {
                writer.Write(doc);
            }
            writer.Flush();
        }

        protected void WriteHeader(BinaryWriter writer, int msgSize)
        {
            MessageHeader header = this.Header;
            writer.Write(msgSize);
            writer.Write(header.RequestId);
            writer.Write(header.ResponseTo);
            writer.Write((int)header.OpCode);
            writer.Flush();
        }
    }
}
