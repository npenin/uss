using System;
using System.Collections.Generic;

namespace Evaluant.Uss.OData.Operations
{
    internal class Get : IOperation
    {
        public Get()
        {
            headers = new Dictionary<string, string>();
        }

        private System.Collections.Generic.IDictionary<string, string> headers;
        public UriExpressions.RootContainerExpression Expression { get; set; }

        #region IOperation Members


        public int WriteTo(System.IO.StreamWriter s, ODataPersistenceEngineAsync engine)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IOperation Members


        public string Method
        {
            get { return "GET"; }
        }

        #endregion

        #region IOperation Members


        public System.Collections.Generic.IDictionary<string, string> Headers
        {
            get { return headers; }
        }

        #endregion

        #region IOperation Members


        public void Complete(Model.Model model)
        {
        }

        #endregion
    }
}
