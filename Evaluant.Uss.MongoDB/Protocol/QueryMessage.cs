using System;
using Evaluant.Uss.MongoDB.Bson;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.MongoDB.Protocol
{
    /// <summary>
    /// Description of QueryMessage.
    /// </summary>
    /// <remarks>
    ///    MsgHeader header;                 // standard message header
    ///    int32     opts;                   // query options.  See QueryOptions for values
    ///    cstring   fullCollectionName;     // "dbname.collectionname"
    ///    int32     numberToSkip;           // number of documents to skip when returning results
    ///    int32     numberToReturn;         // number of documents to return in the first OP_REPLY
    ///    BSON      query ;                 // query object.  See below for details.
    ///  [ BSON      returnFieldSelector; ]  // OPTIONAL : selector indicating the fields to return.  See below for details.
    /// </remarks>
    public class QueryMessage : RequestMessageBase
    {
        public QueryOptions Options { get; set; }

        public string FullCollectionName { get; set; }

        public int NumberToSkip { get; set; }

        public int NumberToReturn { get; set; }

        public Entity Query { get; set; }

        public Entity ReturnFieldSelector { get; set; }
        
        public QueryMessage(){
            this.Header = new MessageHeader(OpCode.Query);
        }

        public QueryMessage(Entity query, String fullCollectionName)
            :this(query,fullCollectionName,0,0){
        }

        public QueryMessage(Entity query, String fullCollectionName, Int32 numberToReturn, Int32 numberToSkip)
            :this(query,fullCollectionName,numberToReturn, numberToSkip, null){
        }

        public QueryMessage(Entity query, String fullCollectionName, Int32 numberToReturn,
                            Int32 numberToSkip, Entity returnFieldSelector)
        {
            this.Header = new MessageHeader(OpCode.Query);
            this.Query = query;
            this.FullCollectionName = fullCollectionName;
            this.NumberToReturn = numberToReturn;
            this.NumberToSkip = numberToSkip;
            this.ReturnFieldSelector = returnFieldSelector;
        }

        protected override void WriteBody (ISonWriter writer){
            writer.WriteValue(BsonDataType.Integer,(int)this.Options);
            writer.WriteString(this.FullCollectionName);
            writer.WriteValue(BsonDataType.Integer,(int)this.NumberToSkip);
            writer.WriteValue(BsonDataType.Integer,(int)this.NumberToReturn);
            writer.Write(this.Query);
            if(this.ReturnFieldSelector != null){
                writer.Write(this.ReturnFieldSelector);
            }
        }
		
		protected override int CalculateBodySize(BsonWriter writer){
            int size = 12; //options, numbertoskip, numbertoreturn
            size += writer.CalculateSize(this.FullCollectionName,false);
            size += writer.CalculateSize(this.Query);
            if(this.ReturnFieldSelector != null){
                size += writer.CalculateSize(this.ReturnFieldSelector);
            }
            return size;
        }        
    }
}
