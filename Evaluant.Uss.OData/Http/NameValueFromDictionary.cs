using System;
using System.Collections.Generic;

namespace Evaluant.Uss.OData.Http
{
    internal class NameValueFromDictionary : Dictionary<string, List<string>>
    {
        // Methods
        public NameValueFromDictionary(int capacity, IEqualityComparer<string> comparer)
            : base(capacity, comparer)
        {
        }

        public void Add(string key, string value)
        {
            List<string> list;
            if (base.ContainsKey(key))
            {
                list = base[key];
            }
            else
            {
                list = new List<string>();
            }
            list.Add(value);
            base[key] = list;
        }

        public string Get(string name)
        {
            string str = null;
            if (base.ContainsKey(name))
            {
                List<string> list = base[name];
                for (int i = 0; i < list.Count; i++)
                {
                    if (i == 0)
                    {
                        str = list[i];
                    }
                    else
                    {
                        str = str + list[i];
                    }
                    if (i != (list.Count - 1))
                    {
                        str = str + ",";
                    }
                }
            }
            return str;
        }

        public void Set(string key, string value)
        {
            List<string> list = new List<string>();
            list.Add(value);
            base[key] = list;
        }
    }

 

}
