using Evaluant.Uss.MongoDB.Bson;
using Evaluant.Uss.Domain;

namespace Evaluant.Uss.MongoDB.Protocol
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    ///  struct {
    ///       MsgHeader header;             // standard message header
    ///       int32     ZERO;               // 0 - reserved for future use
    ///       cstring   fullCollectionName; // "dbname.collectionname"
    ///       int32     flags;              // value 0 for upsert 1 for multiupdate operation
    ///       BSON      selector;           // the query to select the document
    ///       BSON      document;           // the document data to update with or insert
    ///  }
    /// </remarks>
    public class UpdateMessage : RequestMessageBase
    {
        public string FullCollectionName { get; set; }

        public Entity Selector { get; set; }

        public Entity Document { get; set; }

        public int Flags { get; set; }

        public UpdateMessage(){
            this.Header = new MessageHeader(OpCode.Update);
        }
        
        protected override void WriteBody (ISonWriter writer){
            writer.WriteValue(BsonDataType.Integer,0);
            writer.WriteString(this.FullCollectionName);
            writer.WriteValue(BsonDataType.Integer,this.Flags);
            writer.Write(Selector);
            writer.Write(Document);
        }
        
        protected override int CalculateBodySize(BsonWriter writer){
            int size = 4; //first int32
            size += writer.CalculateSize(this.FullCollectionName,false);
            size += 4; //flags
            size += writer.CalculateSize(this.Selector);
            size += writer.CalculateSize(this.Document);
            return size;
        }       
    }
}