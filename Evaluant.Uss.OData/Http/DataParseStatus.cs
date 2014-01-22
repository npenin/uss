using System;

namespace Evaluant.Uss.OData.Http
{
    internal enum DataParseStatus
    {
        NeedMoreData,
        ContinueParsing,
        Done,
        Invalid,
        DataTooBig
    }
}

