using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.EntityResolver.Proxy;

namespace Evaluant.Uss.Tests.GeneratedCode
{
    public class Address : IPersistableProxy
    {


        public Address()
        {
            Id = Guid.NewGuid().ToString();
        }
        private string id;

        public string Id
        {
            get { return id; }
            set { id = value; }
        }


        private string city;

        public string City
        {
            get { return city; }
            set { city = value; }
        }

        bool processing;

        #region IPersistableProxy Members

        public void Set()
        {
            if (processing)
                return;

            processing = true;

            object value=Entity.GetValue("Id");

            if (value != null)
                Id = (string)value;

            value = Entity.GetValue("City");

            if (value != null)
                City = (string)value;

            processing = false;
        }

        public void SetReferences()
        {
            if (processing)
                return;

            processing = true;


        }

        public void Update()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IPersistable Members

        public Domain.Entity Entity
        {
            get;
            set;
        }

        public Uss.ObjectContext.Contracts.IPersistenceEngineObjectContext ObjectContext
        {
            get;
            set;
        }

        public Uss.ObjectContext.Contracts.IPersistenceEngineObjectContextAsync ObjectContextAsync
        {
            get;
            set;
        }

        #endregion
    }
}
