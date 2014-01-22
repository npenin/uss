using System;
using System.IO;
using System.Reflection;
using Evaluant.Uss.Utility;

namespace Evaluant.Uss.Serializer
{
    public class BinaryFormatter
    {
        public void Serialize(Stream s, object objectToSerialize)
        {
            BinaryWriter writer = new BinaryWriter(s);
            if (objectToSerialize == null)
                throw new ArgumentNullException("objectToSerialize");
            Type objectType = objectToSerialize.GetType();
            writer.Write(objectType.AssemblyQualifiedName);
            if (objectToSerialize is ISerializable)
            {
                ((ISerializable)objectToSerialize).WriteTo(writer);
            }
            else
            {
                foreach (PropertyInfo pi in objectType.GetProperties(BindingFlags.Public))
                {
                    if (!pi.CanRead || !pi.CanWrite)
                        continue;
                    writer.Write('#');
                    writer.Write(pi.Name);
                    if (pi.PropertyType.IsPrimitive || pi.PropertyType == typeof(string) || pi.PropertyType == typeof(byte[]))
                    {
                        object value = pi.GetValue(objectToSerialize, null);
                        switch (Helper.GetTypeCode(value))
                        {
                            case TypeCode.Boolean:
                                writer.Write("bool:");
                                writer.Write((bool)value);
                                break;
                            case TypeCode.Byte:
                                writer.Write("byte:");
                                writer.Write((byte)value);
                                break;
                            case TypeCode.Char:
                                writer.Write("char:");
                                writer.Write((char)value);
                                break;
                            case TypeCode.DateTime:
                                writer.Write("datetime:");
                                writer.Write(((DateTime)value).Ticks);
                                break;
                            case TypeCode.Decimal:
                                writer.Write("decimal:");
                                writer.Write((double)value);
                                break;
                            case TypeCode.Double:
                                writer.Write("double:");
                                writer.Write((double)value);
                                break;
                            case TypeCode.Int16:
                                writer.Write("short:");
                                writer.Write((short)value);
                                break;
                            case TypeCode.Int32:
                                writer.Write("int:");
                                writer.Write((int)value);
                                break;
                            case TypeCode.Int64:
                                writer.Write("long:");
                                writer.Write((long)value);
                                break;
                            case TypeCode.Object:
                                if (value is byte[])
                                {
                                    writer.Write("byte[" + ((byte[])value).Length + "]:");
                                    writer.Write((byte[])value);
                                }
                                else
                                    throw new NotSupportedException();
                                break;
                            case TypeCode.SByte:
                                writer.Write("sbyte:");
                                writer.Write((sbyte)value);
                                break;
                            case TypeCode.Single:
                                writer.Write("float:");
                                writer.Write((float)value);
                                break;
                            case TypeCode.String:
                                writer.Write("string:");
                                writer.Write((string)value);
                                break;
                            case TypeCode.UInt16:
                                writer.Write("ushort:");
                                writer.Write((ushort)value);
                                break;
                            case TypeCode.UInt32:
                                writer.Write("uint:");
                                writer.Write((uint)value);
                                break;
                            case TypeCode.UInt64:
                                writer.Write("ulong:");
                                writer.Write((ulong)value);
                                break;
                            default:
                                throw new NotSupportedException();
                        }
                    }
                    else
                        Serialize(s, pi.GetValue(objectToSerialize, null));
                }
            }
        }

        public object Deserialize(Stream s)
        {
            BinaryReader reader = new BinaryReader(s);
            Type objectType = Type.GetType(reader.ReadString());
            object objectToDeserialize = Activator.CreateInstance(objectType);
            if (objectToDeserialize is ISerializable)
            {
                ((ISerializable)objectToDeserialize).ReadFrom(reader);
            }
            else
            {
                char propertyOfField = reader.ReadChar();
                if (propertyOfField == '#')
                {
                    PropertyInfo pi = objectType.GetProperty(reader.ReadString());
                    if (pi.PropertyType.IsPrimitive || pi.PropertyType == typeof(string) || pi.PropertyType == typeof(byte[]))
                    {
                        string type = reader.ReadString();
                        string value = null;
                        if (!type.EndsWith(":"))
                        {
                            int indexOfColon = type.IndexOf(':') + 1;
                            value = type.Substring(indexOfColon);
                            type = type.Substring(0, indexOfColon);
                        }

                        switch (type)
                        {
                            case "bool:":
                                pi.SetValue(objectToDeserialize, reader.ReadBoolean(), null);
                                break;
                            case "byte:":
                                pi.SetValue(objectToDeserialize, reader.ReadByte(), null);
                                break;
                            case "char:":
                                pi.SetValue(objectToDeserialize, reader.ReadChar(), null);
                                break;
                            case "datetime:":
                                pi.SetValue(objectToDeserialize, new DateTime(reader.ReadInt64()), null);
                                break;
                            case "decimal:":
                                pi.SetValue(objectToDeserialize, new decimal(reader.ReadDouble()), null);
                                break;
                            case "double:":
                                pi.SetValue(objectToDeserialize, reader.ReadDouble(), null);
                                break;
                            case "short:":
                                pi.SetValue(objectToDeserialize, reader.ReadInt16(), null);
                                break;
                            case "int:":
                                pi.SetValue(objectToDeserialize, reader.ReadInt32(), null);
                                break;
                            case "long:":
                                pi.SetValue(objectToDeserialize, reader.ReadInt64(), null);
                                break;
                            case "sbyte:":
                                pi.SetValue(objectToDeserialize, reader.ReadSByte(), null);
                                break;
                            case "float:":
                                pi.SetValue(objectToDeserialize, reader.ReadSingle(), null);
                                break;
                            case "string:":
                                if (string.IsNullOrEmpty(value))
                                    pi.SetValue(objectToDeserialize, reader.ReadString(), null);
                                else
                                    pi.SetValue(objectToDeserialize, value, null);
                                break;
                            case "ushort:":
                                pi.SetValue(objectToDeserialize, reader.ReadUInt16(), null);
                                break;
                            case "uint:":
                                pi.SetValue(objectToDeserialize, reader.ReadUInt32(), null);
                                break;
                            case "ulong:":
                                pi.SetValue(objectToDeserialize, reader.ReadUInt64(), null);
                                break;
                            default:
                                if (type.StartsWith("byte["))
                                    pi.SetValue(objectToDeserialize, reader.ReadBytes(int.Parse(type.Substring(5, type.Length - 5 - 2))), null);
                                throw new NotSupportedException();
                        }
                    }
                    else
                        pi.SetValue(objectToDeserialize, Deserialize(s), null);
                }
            }

            return objectToDeserialize;
        }
    }
}
