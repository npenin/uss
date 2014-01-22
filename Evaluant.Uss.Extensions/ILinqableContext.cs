using System;
using System.Linq;
using System.Linq.Expressions;

namespace Evaluant.Uss.ObjectContext.Contracts
{
    public interface ILinqableContext
    {
        IQueryable<T> Cast<T>() where T : class;
    }

    public interface IInferrable : IQueryable
    {
        IInferrable Infer(string reference);
    }


    public interface IInferrable<T> : IQueryable<T>
    {
        IInferrable<T> Infer(string reference);
    }
}
