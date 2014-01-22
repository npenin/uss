using System;
using System.Collections;
using Evaluant.Uss.Domain;
using System.Collections.Generic;

namespace Evaluant.Uss.Commands
{
    public enum CommandTypes
    {
        CompoundCreate,
        CompoundUpdate,
        CreateAttribute,
        CreateEntity,
        CreateReference,
        DeleteAttribute,
        DeleteEntity,
        DeleteReference,
        UpdateAttribute,

    }

#if !SILVERLIGHT
	[Serializable]
#endif
    public abstract class Command : IComparable
    {
        [Obsolete]
        public Command(string parentId)
        {
            _ParentId = parentId;
        }

        public Command(Entity parentEntity)
            : this(parentEntity.Id)
        {
            ParentEntity = parentEntity;
        }

        public Entity ParentEntity { get; set; }

        protected string _ParentId;

        public string ParentId
        {
            get
            {
                if (string.IsNullOrEmpty(_ParentId))
                    _ParentId = ParentEntity.Id;
                return _ParentId;
            }
        }

        protected string clientId;

        public string ClientId
        {
            get { return clientId; }
            set { clientId = value; }
        }


        public virtual void UpdateIds(IDictionary<string, string> newIds)
        {
            if (newIds.ContainsKey(ParentId))
                _ParentId = newIds[ParentId];
        }

        protected bool ignoreMetadata = false;

        /// <summary>
        /// Gets or sets a value indicating whether to ignore metadata.
        /// </summary>
        /// <value><c>true</c> to ignore metadata; otherwise, <c>false</c>.</value>
        public bool IgnoreMetadata
        {
            get { return ignoreMetadata; }
            set { ignoreMetadata = value; }
        }

        protected int _ProcessOrder;

        /// <summary>
        /// Gets the process order.
        /// </summary>
        /// <value>The process order.</value>
        public int ProcessOrder
        {
            get { return _ProcessOrder; }
        }

        #region Membres de IComparable

        public int CompareTo(object obj)
        {
            Command c = obj as Command;

            if (c != null)
                return ProcessOrder.CompareTo(((Command)obj).ProcessOrder);
            else
                throw new ArgumentException("The object must be a Command");
        }

        #endregion

        public abstract CommandTypes CommandType { get; }
    }
}
