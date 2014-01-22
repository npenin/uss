using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.Models
{
    public abstract class Cardinality
    {
        protected Cardinality(bool isMany)
        {
            IsMany = isMany;
        }

        public bool IsMany { get; private set; }
        public bool IsOne { get { return !IsMany;}}

        public class ToOne : Cardinality
        {
            public ToOne(bool isMany) : base(isMany) { }

            public static readonly ToOne One=new ToOne(false);
            public static readonly ToOne Many=new ToOne(true);
        }
        public class ToMany : Cardinality
        {
            public ToMany(bool isMany) : base(isMany) { }

            public static readonly ToMany One = new ToMany(false);
            public static readonly ToMany Many = new ToMany(true);
        }
    }

    //[Flags]
    //public enum Cardinality
    //{
    //    OneToOne=1,
    //    OneToMany=4,
    //    ManyToOne=2,
    //    ManyToMany = 5,

    //    ToOne = 3,
    //    ToMany=6
    //}
}
