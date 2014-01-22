using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace Evaluant.Uss.Data
{
    public class FileDataReader : IDataReader
    {
        public FileDataReader()
        {
            isClosed = false;
        }

        #region IDataReader Members

        public void Close()
        {
            isClosed = true;
        }

        public int Depth
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public DataTable GetSchemaTable()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        bool isClosed;

        public bool IsClosed
        {
            get { return isClosed; }
        }

        public bool NextResult()
        {
            return false;
        }

        public bool Read()
        {
            return false;
        }

        public int RecordsAffected
        {
            get { return 0; }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        #region IDataRecord Members

        public int FieldCount
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public bool GetBoolean(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public byte GetByte(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public char GetChar(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public long GetChars(int i, long fieldoffset, char[] buffer, int bufferoffset, int length)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public IDataReader GetData(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public string GetDataTypeName(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public DateTime GetDateTime(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public decimal GetDecimal(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public double GetDouble(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public Type GetFieldType(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public float GetFloat(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public Guid GetGuid(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public short GetInt16(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public int GetInt32(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public long GetInt64(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public string GetName(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public int GetOrdinal(string name)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public string GetString(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public object GetValue(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public int GetValues(object[] values)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool IsDBNull(int i)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public object this[string name]
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public object this[int i]
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        #endregion
    }
}
