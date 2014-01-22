using System;
using System.Net;
using System.Text;
using System.Collections.Generic;

namespace Evaluant.Uss.OData.Http
{
internal sealed class XHRWebHeaderCollection : WebHeaderCollection
{
    // Fields
    private const int ApproxHighAvgNumHeaders = 16;
    private WebHeaderCollectionType collectionType;
    private static readonly HeaderInfoTable headerTable = new HeaderInfoTable();
    private NameValueFromDictionary innerCollection;

    // Methods
    public XHRWebHeaderCollection() : this(WebHeaderCollectionType.Unknown)
    {
    }

    internal XHRWebHeaderCollection(WebHeaderCollectionType type)
    {
        this.collectionType = type;
    }

    internal void Add(string name, string value)
    {
        this.InnerCollection.Add(name, value);
    }

    internal DataParseStatus ParseHeaders(byte[] byteBuffer, int size, ref int unparsed, ref int totalResponseHeadersLength, int maximumResponseHeadersLength, ref WebParseError parseError)
    {
        char ch;
        string str2;
        if (byteBuffer.Length < size)
        {
            return DataParseStatus.NeedMoreData;
        }
        int index = -1;
        int num2 = -1;
        int num3 = -1;
        int num4 = -1;
        int num5 = -1;
        int num6 = unparsed;
        int num7 = totalResponseHeadersLength;
        WebParseErrorCode generic = WebParseErrorCode.Generic;
        DataParseStatus invalid = DataParseStatus.Invalid;
    Label_0023:
        str2 = string.Empty;
        string str3 = string.Empty;
        bool flag = false;
        string str = null;
        if (this.Count == 0)
        {
            while (num6 < size)
            {
                ch = (char) byteBuffer[num6];
                if ((ch != ' ') && (ch != '\t'))
                {
                    break;
                }
                num6++;
                if ((maximumResponseHeadersLength >= 0) && (++num7 >= maximumResponseHeadersLength))
                {
                    invalid = DataParseStatus.DataTooBig;
                    goto Label_02E5;
                }
            }
            if (num6 == size)
            {
                invalid = DataParseStatus.NeedMoreData;
                goto Label_02E5;
            }
        }
        index = num6;
        while (num6 < size)
        {
            ch = (char) byteBuffer[num6];
            if ((ch != ':') && (ch != '\n'))
            {
                if (ch > ' ')
                {
                    num2 = num6;
                }
                num6++;
                if ((maximumResponseHeadersLength < 0) || (++num7 < maximumResponseHeadersLength))
                {
                    continue;
                }
                invalid = DataParseStatus.DataTooBig;
            }
            else
            {
                if (ch != ':')
                {
                    break;
                }
                num6++;
                if ((maximumResponseHeadersLength < 0) || (++num7 < maximumResponseHeadersLength))
                {
                    break;
                }
                invalid = DataParseStatus.DataTooBig;
            }
            goto Label_02E5;
        }
        if (num6 == size)
        {
            invalid = DataParseStatus.NeedMoreData;
            goto Label_02E5;
        }
    Label_00EF:;
        num5 = ((this.Count == 0) && (num2 < 0)) ? 1 : 0;
        while ((num6 < size) && (num5 < 2))
        {
            ch = (char) byteBuffer[num6];
            if (ch > ' ')
            {
                break;
            }
            if (ch == '\n')
            {
                num5++;
                if (num5 == 1)
                {
                    if ((num6 + 1) == size)
                    {
                        invalid = DataParseStatus.NeedMoreData;
                        goto Label_02E5;
                    }
                    flag = (byteBuffer[num6 + 1] == 32) || (byteBuffer[num6 + 1] == 9);
                }
            }
            num6++;
            if ((maximumResponseHeadersLength >= 0) && (++num7 >= maximumResponseHeadersLength))
            {
                invalid = DataParseStatus.DataTooBig;
                goto Label_02E5;
            }
        }
        if ((num5 != 2) && ((num5 != 1) || flag))
        {
            if (num6 == size)
            {
                invalid = DataParseStatus.NeedMoreData;
                goto Label_02E5;
            }
            num3 = num6;
            while (num6 < size)
            {
                ch = (char) byteBuffer[num6];
                if (ch == '\n')
                {
                    break;
                }
                if (ch > ' ')
                {
                    num4 = num6;
                }
                num6++;
                if ((maximumResponseHeadersLength >= 0) && (++num7 >= maximumResponseHeadersLength))
                {
                    invalid = DataParseStatus.DataTooBig;
                    goto Label_02E5;
                }
            }
            if (num6 == size)
            {
                invalid = DataParseStatus.NeedMoreData;
                goto Label_02E5;
            }
            num5 = 0;
            while ((num6 < size) && (num5 < 2))
            {
                ch = (char) byteBuffer[num6];
                if ((ch != '\r') && (ch != '\n'))
                {
                    break;
                }
                if (ch == '\n')
                {
                    num5++;
                }
                num6++;
                if ((maximumResponseHeadersLength >= 0) && (++num7 >= maximumResponseHeadersLength))
                {
                    invalid = DataParseStatus.DataTooBig;
                    goto Label_02E5;
                }
            }
            if ((num6 == size) && (num5 < 2))
            {
                invalid = DataParseStatus.NeedMoreData;
                goto Label_02E5;
            }
        }
        if (((num3 >= 0) && (num3 > num2)) && (num4 >= num3))
        {
            str3 = Encoding.UTF8.GetString(byteBuffer, num3, (num4 - num3) + 1);
        }
        str = (str == null) ? str3 : (str + " " + str3);
        if ((num6 < size) && (num5 == 1))
        {
            switch (((char) byteBuffer[num6]))
            {
                case ' ':
                case '\t':
                    num6++;
                    if ((maximumResponseHeadersLength < 0) || (++num7 < maximumResponseHeadersLength))
                    {
                        goto Label_00EF;
                    }
                    invalid = DataParseStatus.DataTooBig;
                    goto Label_02E5;
            }
        }
        if ((index >= 0) && (num2 >= index))
        {
            str2 = Encoding.UTF8.GetString(byteBuffer, index, (num2 - index) + 1);
        }
        if (str2.Length > 0)
        {
            this.Add(str2, str);
        }
        totalResponseHeadersLength = num7;
        unparsed = num6;
        if (num5 != 2)
        {
            goto Label_0023;
        }
        invalid = DataParseStatus.Done;
    Label_02E5:
        if (invalid == DataParseStatus.Invalid)
        {
            parseError.Section = WebParseErrorSection.ResponseHeader;
            parseError.Code = generic;
        }
        return invalid;
    }

    internal void SetSpecialHeader(string headerName, string value)
    {
        value = ValidationHelper.CheckBadChars(value, true);
        this.InnerCollection.Remove(headerName);
        if (value.Length != 0)
        {
            this.InnerCollection.Add(headerName, value);
        }
    }

    private void ThrowOnRestrictedHeader(string headerName)
    {
        if ((this.collectionType == WebHeaderCollectionType.HttpWebRequest) && headerTable[headerName].IsRequestRestricted)
        {
            throw new InvalidOperationException("RestrictedHeader");
        }
    }

    // Properties
    public override ICollection<string> AllKeys
    {
        get
        {
            return this.InnerCollection.Keys;
        }
    }

    private bool AllowHttpRequestHeader
    {
        get
        {
            if (this.collectionType == WebHeaderCollectionType.Unknown)
            {
                this.collectionType = WebHeaderCollectionType.WebRequest;
            }
            if (this.collectionType != WebHeaderCollectionType.WebRequest)
            {
                return (this.collectionType == WebHeaderCollectionType.HttpWebRequest);
            }
            return true;
        }
    }

    public override int Count
    {
        get
        {
            return this.InnerCollection.Count;
        }
    }

    private NameValueFromDictionary InnerCollection
    {
        get
        {
            if (this.innerCollection == null)
            {
                this.innerCollection = new NameValueFromDictionary(16, CaseInsensitiveAscii.StaticInstance);
            }
            return this.innerCollection;
        }
    }

    public override string this[HttpRequestHeader header]
    {
        get
        {
            if (!this.AllowHttpRequestHeader)
            {
                throw new InvalidOperationException("WebHeaderCollection.this[HttpRequestHeader].get");
            }
            return this[HttpHeaderToName.RequestHeaderNames[header]];
        }
        set
        {
            if (!this.AllowHttpRequestHeader)
            {
                throw new InvalidOperationException("WebHeaderCollection.this[HttpRequestHeader].set");
            }
            this[HttpHeaderToName.RequestHeaderNames[header]] = value;
        }
    }

    public override string this[string name]
    {
        get
        {
            return this.InnerCollection.Get(name);
        }
        set
        {
            name = ValidationHelper.CheckBadChars(name, false);
            value = ValidationHelper.CheckBadChars(value, true);
            this.ThrowOnRestrictedHeader(name);
            this.InnerCollection.Set(name, value);
        }
    }
}}
