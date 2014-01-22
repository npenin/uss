using System;
using System.Collections.Generic;
using Evaluant.NLinq;
using Evaluant.Uss.Services;
using Evaluant.Uss.PersistenceEngine.Contracts;
using Evaluant.Uss.Domain;
using Evaluant.Uss.Commands;

namespace Evaluant.Uss.ObjectContext.Contracts
{
    public interface IObjectContextAsync : IObjectContextTransactionalBase
    {
        /// <summary>
        /// Initializes the repository.
        /// </summary>
        void BeginInitializeRepository();

        /// <summary>
        /// Initializes the repository.
        /// </summary>
        void BeginInitializeRepository(AsyncCallback callback);

        /// <summary>
        /// Initializes the repository.
        /// </summary>
        void BeginInitializeRepository(AsyncCallback callback, object asyncState);


        /// <summary>
        /// Initializes the repository.
        /// </summary>
        void EndInitializeRepository(IAsyncResult result);

        /// <summary>
        /// Loads a set of IPersistable objects using a <cref>Query</cref> instance
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The collection of the objects found</returns>
        void BeginLoad<T>(AsyncCallback callback, NLinqQuery query);

        /// <summary>
        /// Loads a set of IPersistable objects using a <cref>Query</cref> instance
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The collection of the objects found</returns>
        void BeginLoad<T>(AsyncCallback callback, NLinq.Expressions.Expression query);

        /// <summary>
        /// Loads only one IPersistable object using a <cref>Query</cref> instance
        /// If more than one objects match the query, return the first.
        /// </summary>
        /// <returns>the object found</returns>
        void BeginLoadSingle<T>(AsyncCallback callback);

        /// <summary>
        /// Loads only one IPersistable object using a <cref>Query</cref> instance
        /// If more than one objects match the query, return the first.
        /// </summary>
        /// <returns>the object found</returns>
        void BeginLoadSingle<T>(AsyncCallback callback, object asyncState);


        /// <summary>
        /// Loads only one IPersistable object using a <cref>Query</cref> instance
        /// If more than one objects match the query, return the first.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>the object found</returns>
        void BeginLoadSingle<T>(AsyncCallback callback, Type type);

        /// <summary>
        /// Loads only one IPersistable object using a <cref>Query</cref> instance
        /// If more than one objects match the query, return the first.
        /// </summary>
        /// <param name="constraint">criterias that object must match (nlinq syntax)</param>
        /// <returns>the object found</returns>
        void BeginLoadSingle<T>(AsyncCallback callback, string constraint);

        /// <summary>
        /// Loads only one IPersistable object using a <cref>Query</cref> instance
        /// If more than one objects match the query, return the first.
        /// </summary>
        /// <param name="constraint">criterias that object must match (nlinq syntax)</param>
        /// <returns>the object found</returns>
        void BeginLoadSingle<T>(AsyncCallback callback, NLinq.Expressions.Expression constraint);

        /**/
        /// <summary>
        /// Loads a set of IPersistable objects using a <cref>Query</cref> instance
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The collection of the objects found</returns>
        void BeginLoad<T>(AsyncCallback callback, NLinqQuery query, object asyncState);

        /// <summary>
        /// Loads a set of IPersistable objects using a <cref>Query</cref> instance
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The collection of the objects found</returns>
        void BeginLoad<T>(AsyncCallback callback, NLinq.Expressions.Expression query, object asyncState);

        /// <summary>
        /// Loads only one IPersistable object using a <cref>Query</cref> instance
        /// If more than one objects match the query, return the first.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>the object found</returns>
        void BeginLoadSingle<T>(AsyncCallback callback, Type type, object asyncState);

        /// <summary>
        /// Loads only one IPersistable object using a <cref>Query</cref> instance
        /// If more than one objects match the query, return the first.
        /// </summary>
        /// <param name="constraint">criterias that object must match (nlinq syntax)</param>
        /// <returns>the object found</returns>
        void BeginLoadSingle<T>(AsyncCallback callback, string constraint, object asyncState);

        /// <summary>
        /// Loads only one IPersistable object using a <cref>Query</cref> instance
        /// If more than one objects match the query, return the first.
        /// </summary>
        /// <param name="constraint">criterias that object must match (nlinq syntax)</param>
        /// <returns>the object found</returns>
        void BeginLoadSingle<T>(AsyncCallback callback, NLinq.Expressions.Expression constraint, object asyncState);

        /**/
        /// <summary>
        /// Loads a set of IPersistable objects using a <cref>Query</cref> instance
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The collection of the objects found</returns>
        void BeginLoad<T>(Action<IList<T>> callback, NLinqQuery query);

        /// <summary>
        /// Loads a set of IPersistable objects using a <cref>Query</cref> instance
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The collection of the objects found</returns>
        void BeginLoad<T>(Action<IList<T>> callback, NLinq.Expressions.Expression query);

        /// <summary>
        /// Loads only one IPersistable object using a <cref>Query</cref> instance
        /// If more than one objects match the query, return the first.
        /// </summary>
        /// <returns>the object found</returns>
        void BeginLoadSingle<T>(Action<T> callback);

        /// <summary>
        /// Loads only one IPersistable object using a <cref>Query</cref> instance
        /// If more than one objects match the query, return the first.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>the object found</returns>
        void BeginLoadSingle<T>(Action<T> callback, Type type);

        /// <summary>
        /// Loads only one IPersistable object using a <cref>Query</cref> instance
        /// If more than one objects match the query, return the first.
        /// </summary>
        /// <param name="constraint">criterias that object must match (nlinq syntax)</param>
        /// <returns>the object found</returns>
        void BeginLoadSingle<T>(Action<T> callback, string constraint);

        /// <summary>
        /// Loads only one IPersistable object using a <cref>Query</cref> instance
        /// If more than one objects match the query, return the first.
        /// </summary>
        /// <param name="constraint">criterias that object must match (nlinq syntax)</param>
        /// <returns>the object found</returns>
        void BeginLoadSingle<T>(Action<T> callback, NLinq.Expressions.Expression constraint);
        /**/

        IList<T> EndLoad<T>(IAsyncResult result);
        T EndLoadSingle<T>(IAsyncResult result);
        T EndLoadWithId<T>(IAsyncResult result);

        /// <summary>
        /// Loads only one IPersistable object using a <cref>Query</cref> instance
        /// If more than one objects match the query, return the first.
        /// </summary>
        /// <param name="constraint">criterias that object must match (opath syntax)</param>
        /// <returns>the object found</returns>
        void BeginLoadSingle<T>(AsyncCallback callback, NLinqQuery constraint);

        /// <summary>
        /// Loads only one IPersistable object using a <cref>Query</cref> instance
        /// If more than one objects match the query, return the first.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="constraint">criterias that object must match (opath syntax)</param>
        /// <returns>the object found</returns>
        void BeginLoadSingle<T>(AsyncCallback callback, Type type, string constraint);

        /// <summary>
        /// Loads the entities with the specified type.
        /// </summary>
        /// <returns>The collection of the objects found</returns>
        void BeginLoad<T>(AsyncCallback callback);

        /// <summary>
        /// Loads the entities with the specified type.
        /// </summary>
        /// <param name="constraint">criterias that object must match (opath syntax)</param>
        /// <returns>The collection of the objects found</returns>
        void BeginLoad<T>(AsyncCallback callback, string constraint);

        /// <summary>
        /// Loads the entities with the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The collection of the objects found</returns>
        void BeginLoad<T>(AsyncCallback callback, Type type);

        /// <summary>
        /// Loads the entities with the specified type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="constraint">criterias that object must match (opath syntax)</param>
        /// <returns>The collection of the objects found</returns>
        void BeginLoad<T>(AsyncCallback callback, Type type, string constraint);

        /// <summary>
        /// Loads the parent objects for a given relationship
        /// </summary>
        /// <param name="fromObject">The serialized child object whose parent must be loaded</param>
        /// <param name="role">The role between the parent and the child object</param>
        /// <returns></returns>
        //IList<T> LoadParents<T>(object fromObject, string role);

        /// <summary>
        /// Loads an object from an existing id
        /// </summary>
        /// <param name="type">The type of the object to retrieve</param>
        /// <param name="id">The id of the object to retrieve</param>
        /// <returns>The objects with the corresponding ids</returns>
        void BeginLoadWithId<T>(AsyncCallback callback, string id);

        /// <summary>
        /// Loads some objects given their ids
        /// </summary>
        /// <param name="type">The type of the object to retrieve</param>
        /// <param name="ids">The ids of the objects to retrieve</param>
        /// <returns>The objects with the corresponding ids</returns>
        void BeginLoadWithId<T>(AsyncCallback callback, string[] ids);

        /// <summary>
        /// Commits the transaction.
        /// </summary>
        void BeginCommitTransaction();

        /// <summary>
        /// Commits the transaction.
        /// </summary>
        void BeginCommitTransaction(AsyncCallback callback);

        /// <summary>
        /// Commits the transaction.
        /// </summary>
        void BeginCommitTransaction(AsyncCallback callback, object asyncState);

        void EndCommitTransaction(IAsyncResult result);

        /// <summary>
        /// Imports the specified entity and its related.
        /// </summary>
        /// <param name="entity">Entity to import.</param>
        void BeginImport(object entity);

        /// <summary>
        /// Imports the specified entities and their related.
        /// </summary>
        /// <param name="entities">Entities to import.</param>
        /// <remarks>
        /// Only the related objects which are already loaded will be imported. 
        /// Thus it alows you to import only one node of the object graph.
        /// </remarks>
        void BeginImport<T>(IEnumerable<T> entities);

    }
}
