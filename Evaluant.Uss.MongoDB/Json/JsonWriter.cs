using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Evaluant.Uss.MongoDB.Bson;
using Evaluant.Uss.Domain;
using System.Collections;

namespace Evaluant.Uss.MongoDB.Json
{
    class JsonWriter : ISonWriter
    {
        private TextWriter writer;
        private Encoding encoding = Encoding.UTF8;
        private int buffLength = 256;
        private byte[] buffer;
        int maxChars;

        public JsonWriter(TextWriter writer)
        {
            this.writer = writer;
            buffer = new byte[buffLength];
            maxChars = buffLength / encoding.GetMaxByteCount(1);
        }

        public void Write(Entity doc)
        {
            writer.Write("{ ");
            foreach (Entry key in doc)
            {
                Write(key);
                writer.Write(", ");
            }
            writer.Write("}");
        }

        private void Write(Entry key)
        {
            BsonDataType t = TranslateToBsonType(key);
            this.WriteString(key.Name);
            writer.Write(": ");
            if (key.IsMultiple)
            {
                this.WriteArray((IEnumerable<Entity>)key);
            }
            else
            {
                if (key.IsEntity)
                    this.Write((Entity)key.Value);
                else
                    this.WriteValue(t, key.Value);
            }
        }

        public void WriteArray(IEnumerable<Entity> arr)
        {
            writer.Write('[');
            int keyname = 0;
            foreach (Entity val in arr)
            {
                BsonDataType t = TranslateToBsonType(val);
                this.WriteValue(t, val);
                writer.Write(", ");
                keyname++;
            }
            writer.Write(']');
        }

        public void WriteArray(IEnumerable arr)
        {
            
            writer.Write('[');
            foreach (Object val in arr)
            {
                BsonDataType t = TranslateToBsonType(val);

                this.WriteValue(t, val);
                writer.Write(", ");
            }
            writer.Write(']');
        }

        public void WriteValue(BsonDataType dt, Object obj)
        {
            switch (dt)
            {
                case BsonDataType.MinKey:
                case BsonDataType.MaxKey:
                case BsonDataType.Null:
                    return;
                case BsonDataType.Boolean:
                    writer.Write((bool)obj);
                    return;
                case BsonDataType.Integer:
                    writer.Write((int)obj);
                    return;
                case BsonDataType.Long:
                    writer.Write((long)obj);
                    return;
                case BsonDataType.Date:
                    DateTime d = (DateTime)obj;
                    //TimeSpan diff = d.ToUniversalTime() - BsonInfo.Epoch;
                    //double time = Math.Floor(diff.TotalMilliseconds);
                    writer.Write(d.ToString());
                    return;
                case BsonDataType.Oid:
                    Oid id = (Oid)obj;
                    writer.Write(id.ToString());
                    return;
                case BsonDataType.Number:
                    writer.Write((double)obj);
                    return;
                case BsonDataType.String:
                    {
                        String str = (String)obj;
                        this.WriteString(str);
                        return;
                    }
                case BsonDataType.Obj:
                    if (obj is Entity)
                    {
                        this.Write((Entity)obj);
                    }
                    else if (obj is Entry)
                    {
                        this.Write((Entry)obj);
                    }
                    return;
                case BsonDataType.Array:
                    this.WriteArray((IEnumerable)obj);
                    return;
                case BsonDataType.Regex:
                    {
                        MongoRegex r = (MongoRegex)obj;
                        writer.Write('/');
                        this.WriteString(r.Expression);
                        writer.Write('/');
                        this.WriteString(r.Options);
                        return;
                    }
                //case BsonDataType.Code:
                //    {
                //        Code c = (Code)obj;
                //        this.WriteValue(BsonDataType.String, c.Value);
                //        return;
                //    }
                //case BsonDataType.CodeWScope:
                //    {
                //        CodeWScope cw = (CodeWScope)obj;
                //        writer.Write(CalculateSize(cw));
                //        this.WriteValue(BsonDataType.String, cw.Value);
                //        this.WriteValue(BsonDataType.Obj, cw.Scope);
                //        return;
                //    }
                case BsonDataType.Binary:
                    {
                        if (obj is Guid)
                        {
                            writer.Write(((Guid)obj).ToString());
                        }
                        else
                        {
                            Binary b = (Binary)obj;
                            if (b.Subtype == Binary.TypeCode.General)
                            {
                                writer.Write(b.Bytes.Length + 4);
                                writer.Write((byte)b.Subtype);
                                writer.Write(b.Bytes.Length);
                            }
                            else
                            {
                                writer.Write(b.Bytes.Length);
                                writer.Write((byte)b.Subtype);
                            }
                            writer.Write(b.Bytes);
                        }
                        return;
                    }
                default:
                    throw new NotImplementedException(String.Format("Writing {0} types not implemented.", obj.GetType().Name));
            }
        }

        public void WriteString(String str)
        {
            writer.Write('"');
            writer.Write(str);
            writer.Write('"');
        }

        public void Flush()
        {
            writer.Flush();
        }

        protected BsonDataType TranslateToBsonType(Entry val)
        {
            switch (val.TypeCode)
            {
                case TypeCode.Boolean:
                    return BsonDataType.Boolean;
                case TypeCode.Byte:
                    return BsonDataType.Binary;
                case TypeCode.Char:
                    return BsonDataType.String;
                case TypeCode.DBNull:
                    return BsonDataType.Null;
                case TypeCode.DateTime:
                    return BsonDataType.Date;
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int64:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return BsonDataType.Number;
                case TypeCode.Empty:
                    return BsonDataType.Null;
                case TypeCode.Int32:
                    return BsonDataType.Integer;
                case TypeCode.Object:
                    if (val.Value is Oid)
                        return BsonDataType.Oid;
                    return BsonDataType.Obj;
                case TypeCode.SByte:
                    return BsonDataType.Binary;
                case TypeCode.String:
                    return BsonDataType.String;
                default:
                    return BsonDataType.Obj;
            }
        }


        protected BsonDataType TranslateToBsonType(Object val)
        {
            if (val == null) return BsonDataType.Null;
            Type t = val.GetType();
            //special case enums
            if (val is Enum)
            {
                t = Enum.GetUnderlyingType(t);
            }
            BsonDataType ret;
            if (t == typeof(Double))
            {
                ret = BsonDataType.Number;
            }
            else if (t == typeof(Single))
            {
                ret = BsonDataType.Number;
            }
            else if (t == typeof(String))
            {
                ret = BsonDataType.String;
            }
            else if (t == typeof(Entity))
            {
                ret = BsonDataType.Obj;
            }
            else if (t == typeof(int))
            {
                ret = BsonDataType.Integer;
            }
            else if (t == typeof(long))
            {
                ret = BsonDataType.Long;
            }
            else if (t == typeof(bool))
            {
                ret = BsonDataType.Boolean;
            }
            else if (t == typeof(Oid))
            {
                ret = BsonDataType.Oid;
            }
            else if (t == typeof(DateTime))
            {
                ret = BsonDataType.Date;
            }
            else if (t == typeof(MongoRegex))
            {
                ret = BsonDataType.Regex;
            }
            else if (t == typeof(Entry))
            {
                ret = BsonDataType.Obj;
            }
            //else if (t == typeof(Code))
            //{
            //    ret = BsonDataType.Code;
            //}
            //else if (t == typeof(CodeWScope))
            //{
            //    ret = BsonDataType.CodeWScope;
            //}
            else if (t == typeof(DBNull))
            {
                ret = BsonDataType.Null;
            }
            else if (t == typeof(Binary))
            {
                ret = BsonDataType.Binary;
            }
            else if (t == typeof(Guid))
            {
                ret = BsonDataType.Binary;
            }
            else if (t == typeof(MongoMinKey))
            {
                ret = BsonDataType.MinKey;
            }
            else if (t == typeof(MongoMaxKey))
            {
                ret = BsonDataType.MaxKey;
            }
            else if (val is IEnumerable)
            {
                ret = BsonDataType.Array;
            }
            else
            {
                throw new ArgumentOutOfRangeException(String.Format("Type: {0} not recognized", t.FullName));
            }
            return ret;
        }
    }
}
