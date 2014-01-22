using System;
using System.IO;
using System.Collections.Generic;

namespace Evaluant.Uss.OData.Operations
{
    internal interface IOperation
    {
        UriExpressions.RootContainerExpression Expression { get; }

        IDictionary<string,string> Headers { get; }

        int WriteTo(StreamWriter s, ODataPersistenceEngineAsync engine);

        string Method { get; }

        void Complete(Model.Model model);
    }
}
