using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Evaluant.Uss.Utility
{
    public static class Helper
    {
        public static TypeCode GetTypeCode(object value)
        {
            IConvertible cv = value as IConvertible;
            if (cv != null)
                return cv.GetTypeCode();
            return TypeCode.Object;
        }
    }
}
