using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.SqlExpressions;

namespace Evaluant.Uss.SqlExpressions.Mapping
{
    public class Table
    {
        public Table(string tableName)
        {
            this.TableName = tableName;
            Fields = new Dictionary<string, Field>();
        }

        public Table(string schema, string tableName)
        {
            Schema = schema;
            this.TableName = tableName;
            Fields = new Dictionary<string, Field>();
        }

        public string Schema { get; set; }

        public string TableName { get; private set; }

        public IDictionary<string, Field> Fields { get; set; }

        public string[] Ids
        {
            get
            {
                if (Fields == null)
                    return null;
                List<string> ids = new List<string>();
                foreach (var field in Fields)
                {
                    if (field.Value.IsPrimaryKey)
                        ids.Add(field.Key);
                }
                return ids.ToArray();
            }
        }
    }
}
