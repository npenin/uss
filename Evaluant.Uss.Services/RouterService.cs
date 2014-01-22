using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.Services.Routing;

namespace Evaluant.Uss.Services
{
    public class RouterService : IService
    {
        private IList<Rule> rules = new List<Rule>();
        public IList<Rule> Rules
        {
            get { return rules; }
        }

        #region IService Membres

#if !EUSS12
        public void Visit(object item, ServiceContext context)
        {
            foreach (Rule rule in rules)
            {
                if (rule.Condition.IsSatisfied(item))
                    rule.ServicesPointed.Visit(item, context);
            }
        }

        public void Visit(object parent, object child, ReferenceServiceContext context)
        {
            foreach (Rule rule in rules)
            {
                if (rule.Condition.IsSatisfied(parent))
                    rule.ServicesPointed.Visit(parent, child, context);
            }
        }


#else

        public void Visit<T>(T item, ServiceContext context)
            where T : class
        {
            foreach (Rule rule in rules)
            {
                if (rule.Condition.IsSatisfied(item))
                    rule.ServicesPointed.Visit<T>(item, context);
            }
        }

        public void Visit<Parent, Child>(Parent parent, Child child, ReferenceServiceContext<Child> context)
            where Parent : class
            where Child : class
        {
            foreach (Rule rule in rules)
            {
                if (rule.Condition.IsSatisfied(parent))
                    rule.ServicesPointed.Visit<Parent, Child>(parent, child, context);
            }
        }

#endif

        public void ServiceAdded()
        {
        }

        public void ServiceRemoved()
        {
        }

        #endregion
    }
}
