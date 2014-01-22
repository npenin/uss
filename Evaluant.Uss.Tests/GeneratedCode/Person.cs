using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.EntityResolver.Proxy;
using Evaluant.Uss.ObjectContext.Contracts;

namespace Evaluant.Uss.Tests.GeneratedCode
{
    public class Person : IPersistableProxy
    {
        public Person()
        {
            Id = Guid.NewGuid();
            Friends = new List<Person>();
        }

        public Guid Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }

        public virtual Address Address { get; set; }

        public IList<Person> Friends { get; set; }

        #region IPersistableProxy Members

        bool processing;

        public void Set()
        {
            if (processing)
                return;

            processing = true;

            object value = Entity.GetValue("Id");

            if (value != null)
                Id = (Guid)value;

            value = Entity.GetValue("FirstName");

            if (value != null)
                FirstName = (string)value;

            value = Entity.GetValue("LastName");

            if (value != null)
                LastName = (string)value;


            processing = false;
        }

        public void SetReferences()
        {
            if (processing)
                return;

            processing = true;

            if (ocAsync == null)
                Friends = new IPersistableCollection<Person>(oc, this, "Friends", null);
            else
                Friends = new IPersistableCollection<Person>(ocAsync, this, "Friends", null);


            processing = false;
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

        private IPersistenceEngineObjectContext oc;

        public IPersistenceEngineObjectContext ObjectContext
        {
            get { return oc; }
            set { oc = value; }
        }

        private IPersistenceEngineObjectContextAsync ocAsync;

        public IPersistenceEngineObjectContextAsync ObjectContextAsync
        {
            get { return ocAsync; }
            set { ocAsync = value; }
        }
        #endregion
    }
}
