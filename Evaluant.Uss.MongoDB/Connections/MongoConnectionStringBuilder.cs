using System;
using System.Collections.Generic;
using System.Text;
using System.Data.Common;

namespace Evaluant.Uss.MongoDB.Connections
{
    public class MongoConnectionStringBuilder : DbConnectionStringBuilder
    {
        public MongoConnectionStringBuilder(string connectionString)
        {
            ConnectionString = connectionString;
        }

        protected TimeSpan timeout = TimeSpan.MinValue;
        protected TimeSpan lifeTime = TimeSpan.MinValue;

        public TimeSpan Timeout
        {
            get
            {
                if (timeout == TimeSpan.MinValue)
                {
                    object timeoutString;
                    if (TryGetValue("Timeout", out timeoutString))
                        timeout = TimeSpan.Parse(timeoutString.ToString());
                    else
                        timeout = TimeSpan.MaxValue;
                }
                return timeout;
            }
            set
            {
                this["Timeout"] = timeout = value;
            }
        }

        public string Database
        {
            get { return (string)this["Database"]; }
        }

        private List<string> servers;

        public string[] Servers
        {
            get
            {
                if (servers == null)
                    servers = new List<string>(((string)this["Server"]).Split(','));
                return servers.ToArray();
            }
        }

        public int MaximumPoolSize
        {
            get { return (int)this["MaximumPoolSize"]; }
            set { this["MaximumPoolSize"] = value; }
        }

        public int MinimumPoolSize
        {
            get { return (int)this["MinimumPoolSize"]; }
            set { this["MinimumPoolSize"] = value; }
        }

        public TimeSpan ConnectionLifetime
        {
            get
            {
                if (timeout == TimeSpan.MinValue)
                {
                    object timeoutString;
                    if (TryGetValue("ConnectionLifeTime", out timeoutString))
                        lifeTime = TimeSpan.Parse(timeoutString.ToString());
                    else
                        lifeTime = TimeSpan.MaxValue;
                }
                return lifeTime;
            }
            set
            {
                this["ConnectionLifeTime"] = lifeTime = value;
            }
        }

        public bool Pooled
        {
            get
            {
                object pooled;
                if (TryGetValue("Pooled", out pooled))
                {
                    return bool.Parse(pooled as string);
                }
                return false;
            }
        }
    }
}
