using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.SqlMapper
{
    public class SqlLoaderFileDriver : FileDriver
    {
        public override Type ConnectionType
        {
            get
            {
                return typeof(FileSqlLoaderConnection);
            }
        }

        public override System.Data.IDbConnection CreateConnection()
        {
            return new FileSqlLoaderConnection();
        }
    }
}
