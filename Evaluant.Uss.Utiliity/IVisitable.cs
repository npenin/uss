using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.Domain
{
    public interface IVisitable<T>
    {
        void Accept(IVisitor<T> visitor);
    }

    public interface IVisitable<T, TResult>
    {
        TResult Accept(IVisitor<T> visitor);
    }
}
