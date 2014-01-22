using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;
using System.Data;

namespace Evaluant.Uss.Data.FileClient
{
    public class FileParameter : DbParameter
    {
        DbType dbType;

        public override System.Data.DbType DbType
        {
            get
            {
                return dbType;
            }
            set
            {
                dbType = value;
            }
        }

        public override System.Data.ParameterDirection Direction
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public override bool IsNullable
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        string parameterName;

        public override string ParameterName
        {
            get
            {
                return parameterName;
            }
            set
            {
                parameterName = value;
            }
        }

        public override void ResetDbType()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        int size;

        public override int Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
            }
        }

        public override string SourceColumn
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public override bool SourceColumnNullMapping
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        public override System.Data.DataRowVersion SourceVersion
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        object value;

        public override object Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }
    }
}
