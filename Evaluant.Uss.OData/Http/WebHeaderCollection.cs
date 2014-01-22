using System;
using System.Net;
using System.Collections.Generic;

namespace Evaluant.Uss.OData.Http
{
    internal abstract class WebHeaderCollection
    {
        // Methods
        protected WebHeaderCollection()
        {
        }

        public virtual void Set(HttpRequestHeader header, string value)
        {
            this[header] = value;
        }

        // Properties
        public abstract ICollection<string> AllKeys { get; }

        public abstract int Count { get; }

        public abstract string this[HttpRequestHeader header] { get; set; }

        public abstract string this[string name] { get; set; }
    }
}
