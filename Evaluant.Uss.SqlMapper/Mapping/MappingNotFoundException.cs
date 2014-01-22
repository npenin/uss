using System;
using System.Text;

namespace Evaluant.Uss.SqlMapper
{
    public class MappingNotFoundException : ApplicationException
    {
        public MappingNotFoundException() : base()
        {
        }

        public MappingNotFoundException(string message) : base(message)
        {
        }
    }
}
