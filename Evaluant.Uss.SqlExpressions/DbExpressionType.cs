using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.SqlExpressions
{
    public enum DbExpressionType
    {
        Unknown = 1000,
        Table, // make sure these don't overlap with ExpressionType
        Case,
        ClientJoin,
        Column,
        Select,
        Projection,
        Entity,
        Join,
        Aggregate,
        Scalar,
        Exists,
        In,
        Grouping,
        AggregateSubquery,
        IsNull,
        Between,
        NamedValue,
        OuterJoined,
        Insert,
        Update,
        Upsert,
        Delete,
        If,
        Function,
        Value,
        Parameter,
        Property,
        Drop,
        Create,
        Alter,
        Schema,
        Not,
        HardCoded,
    }

    public enum DbUnaryExpressionType
    {
        Not,
        Function
    }
}
