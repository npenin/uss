using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Evaluant.Uss.SqlExpressions;

namespace Evaluant.Uss.SqlMapper.Mapper
{
    class LazyParameter : Evaluant.Uss.SqlExpressions.ValuedParameter
    {
        private ValuedParameter innerParameter;
        public override object Value
        {
            get
            {
                return innerParameter.Value;
            }
            set
            {
                innerParameter.Value = value;
            }
        }

        public LazyParameter(ValuedParameter innerParameter)
            : base(innerParameter.Name, null)
        {
            this.innerParameter = innerParameter;
        }

        public LazyParameter(string name, ValuedParameter idParameter, ParameterDirection parameterDirection)
            : base(name, null, idParameter.Type, parameterDirection)
        {
            this.innerParameter = idParameter;
        }

        public LazyParameter(ValuedParameter idParameter, ParameterDirection parameterDirection)
            : base(idParameter.Name, null, idParameter.Type, parameterDirection)
        {
            this.innerParameter = idParameter;
        }
    }
}
