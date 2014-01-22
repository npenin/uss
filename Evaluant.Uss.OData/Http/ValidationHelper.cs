using System;
namespace Evaluant.Uss.OData.Http
{
    internal static class ValidationHelper
    {
        private static readonly char[] HttpTrimCharacters = new char[] { '\t', '\n', '\v', '\f', '\r', ' ' };
        private static readonly char[] InvalidParamChars = new char[] { 
            '(', ')', '<', '>', '@', ',', ';', ':', '\\', '"', '\'', '/', '[', ']', '?', '=', 
            '{', '}', ' ', '\t', '\r', '\n'
         };

        internal static string CheckBadChars(string name, bool isHeaderValue)
        {
            if (string.IsNullOrEmpty(name))
            {
                if (isHeaderValue)
                {
                    return string.Empty;
                }
                if (name == null)
                {
                    throw new ArgumentNullException("name");
                }
                throw new InvalidOperationException("ValidationHelper.CheckBadChars.7");
            }
            if (isHeaderValue)
            {
                name = name.Trim(HttpTrimCharacters);
                int num = 0;
                for (int i = 0; i < name.Length; i++)
                {
                    char ch = (char) ('\x00ff' & name[i]);
                    switch (num)
                    {
                        case 0:
                        {
                            if (ch != '\r')
                            {
                                goto Label_00B3;
                            }
                            num = 1;
                            continue;
                        }
                        case 1:
                            if (ch != '\n')
                            {
                                throw new InvalidOperationException("ValidationHelper.CheckBadChars");
                            }
                            break;

                        case 2:
                            if ((ch != ' ') && (ch != '\t'))
                            {
                                throw new InvalidOperationException("ValidationHelper.CheckBadChars.2");
                            }
                            goto Label_00AF;

                        default:
                        {
                            continue;
                        }
                    }
                    num = 2;
                    continue;
                Label_00AF:
                    num = 0;
                    continue;
                Label_00B3:
                    if (ch == '\n')
                    {
                        num = 2;
                    }
                    else if ((ch == '\x007f') || ((ch < ' ') && (ch != '\t')))
                    {
                        throw new InvalidOperationException("ValidationHelper.CheckBadChars.3");
                    }
                }
                if (num != 0)
                {
                    throw new InvalidOperationException("ValidationHelper.CheckBadChars.4");
                }
                return name;
            }
            if (name.IndexOfAny(InvalidParamChars) != -1)
            {
                throw new InvalidOperationException("ValidationHelper.CheckBadChars.5");
            }
            if (ContainsNonAsciiChars(name))
            {
                throw new InvalidOperationException("ValidationHelper.CheckBadChars.6");
            }
            return name;
        }

        private static bool ContainsNonAsciiChars(string token)
        {
            for (int i = 0; i < token.Length; i++)
            {
                if ((token[i] < ' ') || (token[i] > '~'))
                {
                    return true;
                }
            }
            return false;
        }
    }
}

