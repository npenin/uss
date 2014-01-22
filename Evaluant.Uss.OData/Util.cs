namespace Evaluant.Uss.OData
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Xml;
    using Evaluant.Uss.OData.Http;
    using System.Collections.Generic;

    internal static class Util
    {
        internal const string CodeGeneratorToolName = "System.Data.Services.Design";
        internal static readonly Version DataServiceVersion1 = new Version(1, 0);
        internal static readonly Version DataServiceVersion2 = new Version(2, 0);
        internal static readonly Version DataServiceVersionEmpty = new Version(0, 0);
        internal static readonly char[] ForwardSlash = new char[] { '/' };
        internal static readonly Version MaxResponseVersion = DataServiceVersion2;
        internal static readonly Version[] SupportedResponseVersions = new Version[] { DataServiceVersion1, DataServiceVersion2 };
        internal const string VersionSuffix = ";NetFx";
        private static char[] whitespaceForTracing = new char[] { '\r', '\n', ' ', ' ', ' ', ' ', ' ' };


        internal static bool AreSame(string value1, string value2)
        {
            return (value1 == value2);
        }

        internal static bool AreSame(XmlReader reader, string localName, string namespaceUri)
        {
            return ((((XmlNodeType.Element == reader.NodeType) || (XmlNodeType.EndElement == reader.NodeType)) && AreSame(reader.LocalName, localName)) && AreSame(reader.NamespaceURI, namespaceUri));
        }

        internal static void CheckArgumentNotEmpty<T>(T[] value, string parameterName) where T : class
        {
            CheckArgumentNull<T[]>(value, parameterName);
            if (value.Length == 0)
            {
                throw new ArgumentException("Empty array {0}", parameterName);
            }
            for (int i = 0; i < value.Length; i++)
            {
                if (object.ReferenceEquals(value[i], null))
                {
                    throw new ArgumentNullException(parameterName);
                }
            }
        }

        internal static void CheckArgumentNotEmpty(string value, string parameterName)
        {
            CheckArgumentNull<string>(value, parameterName);
            if (value.Length == 0)
            {
                throw new ArgumentException("Empty string {0}", parameterName);
            }
        }

        internal static T CheckArgumentNull<T>(T value, string parameterName) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
            return value;
        }

        internal static HttpStack CheckEnumerationValue(HttpStack value, string parameterName)
        {
            switch (value)
            {
                case HttpStack.Auto:
                case HttpStack.ClientHttp:
                case HttpStack.XmlHttp:
                    return value;
            }
            throw new ArgumentOutOfRangeException(parameterName);
        }

        public static int IndexOfReference<T>(T[] array, T value)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (EqualityComparer<T>.Default.Equals(array[i], value))
                    return i;
            }
            return -1;
        }

        internal static bool ContainsReference<T>(T[] array, T value) where T : class
        {

            return (0 <= IndexOfReference<T>(array, value));
        }

        internal static Uri CreateUri(string value, UriKind kind)
        {
            if (value != null)
            {
                return new Uri(value, kind);
            }
            return null;
        }

        internal static Uri CreateUri(Uri baseUri, Uri requestUri)
        {
            CheckArgumentNull<Uri>(requestUri, "requestUri");
            if (!requestUri.IsAbsoluteUri)
            {
                if (baseUri.OriginalString.EndsWith("/", StringComparison.Ordinal))
                {
                    if (requestUri.OriginalString.StartsWith("/", StringComparison.Ordinal))
                    {
                        requestUri = new Uri(baseUri, CreateUri(requestUri.OriginalString.TrimStart(ForwardSlash), UriKind.Relative));
                        return requestUri;
                    }
                    requestUri = new Uri(baseUri, requestUri);
                    return requestUri;
                }
                requestUri = CreateUri(baseUri.OriginalString + "/" + requestUri.OriginalString.TrimStart(ForwardSlash), UriKind.Absolute);
            }
            return requestUri;
        }

        [Conditional("DEBUG")]
        internal static void DebugInjectFault(string state)
        {
        }

        internal static string DereferenceIdentity(string uri)
        {
            return uri;
        }

        internal static void Dispose<T>(T disposable) where T : class, IDisposable
        {
            if (disposable != null)
            {
                disposable.Dispose();
            }
        }

        internal static void Dispose<T>(ref T disposable) where T : class, IDisposable
        {
            Dispose<T>((T)disposable);
            disposable = default(T);
        }

        internal static bool DoesNullAttributeSayTrue(XmlReader reader)
        {
            string attribute = reader.GetAttribute("null", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            return ((attribute != null) && XmlConvert.ToBoolean(attribute));
        }

        internal static bool DoNotHandleException(Exception ex)
        {
            if (ex == null)
            {
                return false;
            }
            return (((ex is StackOverflowException) || (ex is OutOfMemoryException)) || (ex is ThreadAbortException));
        }

        internal static Type GetTypeAllowingNull(Type type)
        {
            if (!TypeAllowsNull(type))
            {
                return typeof(Nullable<>).MakeGenericType(new Type[] { type });
            }
            return type;
        }

        internal static char[] GetWhitespaceForTracing(int depth)
        {
            char[] whitespaceForTracing = Util.whitespaceForTracing;
            while (whitespaceForTracing.Length <= depth)
            {
                char[] chArray2 = new char[2 * whitespaceForTracing.Length];
                chArray2[0] = '\r';
                chArray2[1] = '\n';
                for (int i = 2; i < chArray2.Length; i++)
                {
                    chArray2[i] = ' ';
                }
                Interlocked.CompareExchange<char[]>(ref Util.whitespaceForTracing, chArray2, whitespaceForTracing);
                whitespaceForTracing = chArray2;
            }
            return whitespaceForTracing;
        }

        internal static string ReferenceIdentity(string uri)
        {
            return uri;
        }

        internal static bool TypeAllowsNull(Type type)
        {
            if (type.IsValueType)
            {
                return IsNullableType(type);
            }
            return true;
        }

        internal static bool IsNullableType(Type type)
        {
            return type.IsClass || type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        internal static string UriToString(Uri uri)
        {
            if (!uri.IsAbsoluteUri)
            {
                return uri.OriginalString;
            }
            return uri.AbsoluteUri;
        }

        internal static void Write(Stream from, Stream to)
        {
            int length = 0;
            byte[] buffer = new byte[1024];
            while ((length = from.Read(buffer, 0, buffer.Length)) > 0)
            {
                to.Write(buffer, 0, length);
            }
        }
    }
}

