using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.Services.Routing
{
    public class Rule
    {
        protected Condition condition;
        public Condition Condition
        {
            get { return condition; }
            set { condition = value; }
        }

        protected ServicesCollection servicesPointed = new ServicesCollection();
        public ServicesCollection ServicesPointed
        {
            get { return servicesPointed; }
        }
    }
}
