using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Services.Providers;
using Evaluant.Uss.ObjectContext.Contracts;
using Evaluant.Uss.PersistenceEngine.Contracts;
using System.Data.Services;
using System.Diagnostics;
using Evaluant.Uss.Commands;
using Evaluant.Uss.Extensions;
using Evaluant.Uss.Utility;
using Evaluant.Uss.Domain;
using System.Collections;
using Evaluant.Uss.Linq;

namespace Evaluant.Uss.DataServices
{
    public class DataContext : ObjectService, IUpdatable, IExpandProvider
    {
        protected static TraceSource trace = new TraceSource("Evaluant.Uss.DataServices");

        public IQueryable<T> CreateIQueryable<T>()
        {
            return new QueryableUss<T>(oc);
        }

        [Serializable]
        public class Resource
        {
            public string Type { get; set; }
            public string Id { get; set; }

            public Resource(string type, string id)
            {
                this.Id = id;
                this.Type = type;
            }
        }

        List<Command> commands = new List<Command>();
        protected IPersistenceEngineObjectContext oc;
        protected IPersistenceEngineObjectContextAsync asyncOc;

        private string GetEussType(Type type)
        {
            return type.FullName;
        }

        private string ID(object o)
        {
            return oc.GetId(o);
        }

        public DataContext(IPersistenceEngineObjectContextAsync oc)
        {
            this.asyncOc = oc;
        }

        public DataContext(IPersistenceEngineObjectContext oc)
        {
            this.oc = oc;
        }

        #region IUpdatable Members

        /// <summary>
        /// Adds the specified value to the collection.
        /// </summary>
        /// <param name="targetResource">Target object that defines the property.</param>
        /// <param name="propertyName">The name of the collection property to which the resource should be added..</param>
        /// <param name="resourceToBeAdded">The opaque object representing the resource to be added.</param>
        public void AddReferenceToCollection(object targetResource, string propertyName, object resourceToBeAdded)
        {
            trace.TraceInformation(string.Format("AddReferenceToCollection('{0}', '{1}', '{3}')", targetResource, propertyName, resourceToBeAdded));
            try
            {
                CreateReferenceCommand crc = new CreateReferenceCommand(propertyName, ((Resource)targetResource).Id, ((Resource)targetResource).Type, ((Resource)resourceToBeAdded).Id, ((Resource)resourceToBeAdded).Type);
                commands.Add(crc);
            }
            catch (Exception e)
            {
                trace.TraceEvent(TraceEventType.Error, 1, e.Message);
                throw new DataServiceException("An error occured while adding a value to the collection.", e);
            }
        }

        /// <summary>
        /// Cancels a change to the data.
        /// </summary>
        public void ClearChanges()
        {
            trace.TraceInformation(string.Format("ClearChanges()"));

            commands.Clear();
        }

        /// <summary>
        /// Creates the resource of the specified type and that belongs to the specified container.
        /// </summary>
        /// <param name="containerName">The name of the entity set to which the resource belongs.</param>
        /// <param name="fullTypeName">The full namespace-qualified type name of the resource.</param>
        /// <returns>
        /// The object representing a resource of specified type and belonging to the specified container.
        /// </returns>
        public object CreateResource(string containerName, string fullTypeName)
        {
            trace.TraceInformation(string.Format("CreateResource('{0}', '{1}')", containerName, fullTypeName));

            try
            {
                IPersistenceEngineObjectContext peoc = oc as IPersistenceEngineObjectContext;
                var e = new Entity(fullTypeName);
                peoc.PersistenceEngine.CreateId(e);

                CreateEntityCommand cec = new CreateEntityCommand(e);
                commands.Add(cec);

                return new Resource(fullTypeName, e.Id);
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.Message);
                throw new DataServiceException("An error occured while creating the resource.", e);
            }
        }

        /// <summary>
        /// Deletes the specified resource.
        /// </summary>
        /// <param name="targetResource">The resource to be deleted.</param>
        public void DeleteResource(object targetResource)
        {
            trace.TraceInformation(string.Format("DeleteResource('{0}')", targetResource));

            try
            {
                DeleteEntityCommand dec = new DeleteEntityCommand(((Resource)targetResource).Id, ((Resource)targetResource).Type);
                commands.Add(dec);
            }
            catch (Exception e)
            {
                trace.TraceEvent(TraceEventType.Error, 1, e.Message);
                throw new DataServiceException("An error occured while deleting the resource.", e);
            }
        }

        /// <summary>
        /// Gets the resource of the specified type identified by a query and type name.
        /// </summary>
        /// <param name="query">Language integratee query(LINQ) pointing to a particular resource.</param>
        /// <param name="fullTypeName">The fully qualified type name of resource.</param>
        /// <returns>
        /// An opaque object representing a resource of the specified type, referenced by the specified query.
        /// </returns>
        public object GetResource(IQueryable query, string fullTypeName)
        {
            trace.TraceInformation(string.Format("GetResource('{0}', '{1}')", query, fullTypeName));

            object result = null;

            try
            {
                foreach (object o in ((IEnumerable)query))
                {
                    result = o;
                    break;
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.Message);
                throw new DataServiceException("An error occured while getting the resource.", e);
            }

            if (!String.IsNullOrEmpty(fullTypeName) && fullTypeName != result.GetType().FullName)
            {
                throw new DataServiceException("Incorrect type.");
            }

            Entity entity = oc.Resolver.Resolve(result, oc.Factory.PersistenceEngineFactory.Model);

            return new Resource(entity.Type, entity.Id);
        }

        /// <summary>
        /// Gets the value of the specified property on the target object.
        /// </summary>
        /// <param name="targetResource">An opaque object that represents a resource.</param>
        /// <param name="propertyName">The name of the property whose value needs to be retrieved.</param>
        /// <returns></returns>
        public object GetValue(object targetResource, string propertyName)
        {
            trace.TraceInformation(string.Format("GetValue('{0}', '{1}')", targetResource, propertyName));

            try
            {
                object propertyValue = null;

                Entity entity = oc.PersistenceEngine.LoadWithId(((Resource)targetResource).Type, ((Resource)targetResource).Id);

                if (entity == null)
                {
                    throw new ArgumentException("The resource could not be found");
                }

                propertyValue = entity[propertyName];

                return propertyValue;
            }
            catch (Exception e)
            {
                trace.TraceEvent(TraceEventType.Error, 1, e.Message);
                throw new DataServiceException("An error occured while getting the property.", e);
            }
        }

        /// <summary>
        /// Removes the specified value from the collection.
        /// </summary>
        /// <param name="targetResource">The target object that defines the property.</param>
        /// <param name="propertyName">The name of the property whose value needs to be updated.</param>
        /// <param name="resourceToBeRemoved">The property value that needs to be removed.</param>
        public void RemoveReferenceFromCollection(object targetResource, string propertyName, object resourceToBeRemoved)
        {
            trace.TraceInformation(string.Format("RemoveReferenceFromCollection('{0}', '{1}', '{2}')", targetResource, propertyName, resourceToBeRemoved));

            try
            {
                DeleteReferenceCommand drc = new DeleteReferenceCommand(propertyName, ID(targetResource), GetEussType(targetResource.GetType()), ID(resourceToBeRemoved), GetEussType(resourceToBeRemoved.GetType()));
                commands.Add(drc);
            }
            catch (Exception e)
            {
                System.Diagnostics.Trace.WriteLine(e.Message);
                throw new DataServiceException("An error occured while removing a value from the collection.", e);
            }
        }

        /// <summary>
        /// Updates the resource identified by the parameter <paramref name="resource"/>.
        /// </summary>
        /// <param name="resource">The resource to be updated.</param>
        /// <returns></returns>
        public object ResetResource(object resource)
        {
            trace.TraceInformation(string.Format("ResetResource('{0}')", resource));

            // throw new NotImplementedException();
            return null;
        }

        /// <summary>
        /// Returns the instance of the resource represented by the specified resource object.
        /// </summary>
        /// <param name="resource">The object representing the resource whose instance needs to be retrieved.</param>
        /// <returns>
        /// Returns the instance of the resource represented by the specified resource object.
        /// </returns>
        public object ResolveResource(object resource)
        {
            trace.TraceInformation(string.Format("ResolveResource('{0}')", resource));

            return oc.LoadWithEntities<object>(new EntitySet() { new Entity(((Resource)resource).Type) { Id = ((Resource)resource).Id } })[0];
        }

        /// <summary>
        /// Saves all the changes that have been made by using the <see cref="T:System.Data.Services.IUpdatable"/> APIs.
        /// </summary>
        public void SaveChanges()
        {
            trace.TraceInformation("SaveChanges()");

            try
            {
                oc.BeginTransaction();
                oc.ExecuteCommand(commands.ToArray());
                oc.CommitTransaction();
            }
            catch (Exception e)
            {
                trace.TraceEvent(TraceEventType.Error, 1, e.Message);
                throw new DataServiceException("An error occured while saving changes.", e);
            }
        }

        /// <summary>
        /// Sets the value of the specified reference property on the target object.
        /// </summary>
        /// <param name="targetResource">The target object that defines the property.</param>
        /// <param name="propertyName">The name of the property whose value needs to be updated.</param>
        /// <param name="propertyValue">The property value to be updated.</param>
        public void SetReference(object targetResource, string propertyName, object propertyValue)
        {
            trace.TraceInformation(string.Format("SetReference('{0}', '{1}', '{2}')", targetResource, propertyName, propertyValue));

            RemoveReferenceFromCollection(targetResource, propertyName, propertyValue);
            AddReferenceToCollection(targetResource, propertyName, propertyValue);
        }

        /// <summary>
        /// Sets the value of the property with the specified name on the target resource to the specified property value.
        /// </summary>
        /// <param name="targetResource">The target object that defines the property.</param>
        /// <param name="propertyName">The name of the property whose value needs to be updated.</param>
        /// <param name="propertyValue">The property value for update.</param>
        public void SetValue(object targetResource, string propertyName, object propertyValue)
        {
            trace.TraceInformation(string.Format("SetValue('{0}', '{1}', '{2}')", targetResource, propertyName, propertyValue));

            try
            {
                UpdateAttributeCommand uac = new UpdateAttributeCommand(((Resource)targetResource).Id, ((Resource)targetResource).Type, propertyName, propertyValue.GetType(), propertyValue);
                commands.Add(uac);
            }
            catch (Exception e)
            {
                trace.TraceEvent(TraceEventType.Error, 1, e.Message);
                throw new DataServiceException("An error occured while setting the value.", e);
            }
        }

        #endregion

        #region IExpandProvider Members

        public IEnumerable ApplyExpansions(IQueryable queryable, ICollection<ExpandSegmentCollection> expandPaths)
        {
            trace.TraceInformation(string.Format("ApplyExpansions('{0}', '{1}')", queryable, expandPaths));

            foreach (ExpandSegmentCollection esc in expandPaths)
            {
                foreach (ExpandSegment es in esc)
                {
                    ((IInferrable)queryable).Infer(es.Name);
                }
            }

            return null;
        }

        #endregion
    }
}
