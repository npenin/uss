using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.Era
{
    public abstract class Cardinality
    {
        CardinalityValues value;

        protected Cardinality(CardinalityValues value)
        {
            this.value = value;
        }

        public bool IsToMany { get { return (value & CardinalityValues.ToMany) == CardinalityValues.ToMany; } }
        public bool IsToOne { get { return !IsToMany; } }

        public bool IsMany { get { return (value & CardinalityValues.FromMany) == CardinalityValues.FromMany; } }
        public bool IsOne { get { return !IsMany; } }

        public override int GetHashCode()
        {
            return (int)value;
        }

        public class ToOne : Cardinality
        {
            private ToOne(CardinalityValues value) : base(value) { }

            public static readonly ToOne One = new ToOne(CardinalityValues.OneToOne);
            public static readonly ToOne Many = new ToOne(CardinalityValues.ManyToOne);
        }
        public class ToMany : Cardinality
        {
            private ToMany(CardinalityValues value) : base(value) { }

            public static readonly ToMany One = new ToMany(CardinalityValues.OneToMany);
            public static readonly ToMany Many = new ToMany(CardinalityValues.ManyToMany);
        }
    }

    [Flags]
    public enum CardinalityValues
    {
        None = 0,
        OneToOne = 1,
        OneToMany = 2,
        ManyToOne = 5,
        ManyToMany = 6,

        ToOne = 1,
        ToMany = 2,

        FromMany = 4,
    }
}
