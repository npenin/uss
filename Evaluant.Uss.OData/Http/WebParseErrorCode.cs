namespace Evaluant.Uss.OData.Http
{
    using System;

    internal enum WebParseErrorCode
    {
        Generic,
        InvalidHeaderName,
        InvalidContentLength,
        IncompleteHeaderLine,
        CrLfError,
        InvalidChunkFormat,
        UnexpectedServerResponse
    }
}

