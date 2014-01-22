using System;
using System.Collections.Generic;
using System.Text;

namespace Evaluant.Uss.Domain
{
    public interface IVisitor<T, TResult>
    {
        TResult Visit(T item);
    }

    public interface IVisitor<T>
    {
        T Visit(T item);
    }
}
