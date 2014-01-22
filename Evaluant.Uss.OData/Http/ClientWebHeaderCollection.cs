using System;
using System.Collections.Generic;
using System.Net;

namespace Evaluant.Uss.OData.Http
{
    internal sealed class ClientWebHeaderCollection : WebHeaderCollection
    {
        // Fields
        private System.Net.WebHeaderCollection innerCollection;
        private System.Net.HttpWebRequest request;

        // Methods
        internal ClientWebHeaderCollection(System.Net.WebHeaderCollection collection)
        {
            this.innerCollection = collection;
        }

        internal ClientWebHeaderCollection(System.Net.WebHeaderCollection collection, System.Net.HttpWebRequest request)
        {
            this.innerCollection = collection;
            this.request = request;
        }

        // Properties
        public override ICollection<string> AllKeys
        {
            get
            {
                return this.innerCollection.AllKeys;
            }
        }

        public override int Count
        {
            get
            {
                return this.innerCollection.Count;
            }
        }

        public override string this[HttpRequestHeader header]
        {
            get
            {
                return this[HttpHeaderToName.RequestHeaderNames[header]];
            }
            set
            {
                this[HttpHeaderToName.RequestHeaderNames[header]] = value;
            }
        }

        public override string this[string name]
        {
            get
            {
                return this.innerCollection[name];
            }
            set
            {
                if ((name != "Content-Length") && (name != "Accept-Charset"))
                {
                    if (name == "Cookie")
                    {
                        if (this.request != null)
                        {
                            CookieContainer container = new CookieContainer();
                            container.SetCookies(this.request.RequestUri, value);
                            this.request.CookieContainer = container;
                        }
                        else
                        {
                            this.innerCollection[name] = value;
                        }
                    }
                    else
                    {
                        this.innerCollection[name] = value;
                    }
                }
            }
        }
    }
}
