using System;
using System.Collections;
using System.Collections.Generic;

namespace Evaluant.Uss.Utility
{
    // Summary:
    //     Notifies listeners of dynamic changes to a collection, such as when items
    //     are added and removed, or the entire collection object is reset.
    /// <summary>
    /// Notifies listeners of dynamic changes to a collection, such as when items 
    /// are added and removed, or the entire collection object is reset.
    /// </summary>
    public interface INotifyCollectionChanged<T>
    {
        /// <summary>
        ///  Occurs when the items list of the collection has changed, or the collection is reset.
        /// </summary>
        event NotifyCollectionChangedEventHandler<T> CollectionChanged;
    }

    /// <summary>
    ///     Represents the method that handles events that implement the System.Collections.Specialized.INotifyCollectionChanged.CollectionChanged
    ///     event.
    /// </summary>
    /// <param name="sender">The object that raised the event.</param>
    /// <param name="e">The event data.</param>
    public delegate void NotifyCollectionChangedEventHandler<T>(object sender, NotifyCollectionChangedEventArgs<T> e);

    /// <summary>
    /// Provides event data for the System.Collections.Specialized.INotifyCollectionChanged.CollectionChanged
    /// event.
    /// </summary>
    public sealed class NotifyCollectionChangedEventArgs<T> : EventArgs
    {
        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action)
        {
            Action = action;
        }

        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, T changedItem, int index)
            : this(action)
        {
            switch (action)
            {
                case NotifyCollectionChangedAction.Add:
                    NewItems = new List<T>() { changedItem };
                    NewStartingIndex = index;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    OldItems = new List<T>() { changedItem };
                    OldStartingIndex = index;
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, T newItem, T oldItem, int index)
        {
            switch (action)
            {
                case NotifyCollectionChangedAction.Replace:
                    NewItems = new List<T>() { newItem };
                    NewStartingIndex = index;
                    OldItems = new List<T>() { oldItem };
                    OldStartingIndex = index;
                    break;
                case NotifyCollectionChangedAction.Add:
                    NewItems = new List<T>() { newItem };
                    NewStartingIndex = index;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    OldItems = new List<T>() { oldItem };
                    OldStartingIndex = index;
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction action, IEnumerable<T> newItems, IEnumerable<T> oldItem, int index)
        {
        
        }


        public NotifyCollectionChangedAction Action { get; private set; }

        public IList<T> NewItems { get; private set; }

        public int NewStartingIndex { get; private set; }

        public IList<T> OldItems { get; private set; }

        public int OldStartingIndex { get; private set; }
    }

    // Summary:
    //     Describes the action that caused a System.Collections.Specialized.INotifyCollectionChanged.CollectionChanged
    //     event.
    public enum NotifyCollectionChangedAction
    {
        // Summary:
        //     One or more items were added to the collection.
        Add = 0,
        //
        // Summary:
        //     One or more items were removed from the collection.
        Remove = 1,
        //
        // Summary:
        //     One or more items were replaced in the collection.
        Replace = 2,
        //
        // Summary:
        //     The content of the collection changed dramatically.
        Reset = 4,
    }
}
