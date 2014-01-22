using System;
using System.Collections;
#if !SILVERLIGHT
using System.Collections.Specialized;
#endif
using Evaluant.Uss.Commands;
using System.Collections.Generic;
using Evaluant.Uss.Collections;

namespace Evaluant.Uss.Commands
{
	/// <summary>
	/// Description résumée de HashedArray.
	/// </summary>
#if !SILVERLIGHT
	[Serializable]
#endif
    public class CommandCollection : ICollection<Command>
	{

		private static int MAX = 8;

		// Cuts all commands into predefined sections, using their ProcessOrder
		private HashedList<Command>[] _InnerLists = new HashedList<Command>[MAX];

		public CommandCollection()
		{
			for(int i=0; i<MAX; i++)
				_InnerLists[i] = new HashedList<Command>();
		}

		public bool Contains(Command command)
		{
			return _InnerLists[command.ProcessOrder].Contains(command);
		}

		public CommandCollection(IEnumerable<Command> commands) : this()
		{
            AddRange(commands);
		}

		public void AddRange(IEnumerable<Command> commands)
		{
			foreach(Command c in commands)
			{
				this.Add(c);
			}
		}

		public void Add(Command c)
		{
			if(c == null)
				throw new ArgumentNullException("A Command cannot be null");

			_InnerLists[c.ProcessOrder].Add(c);
		}

		#region Membres de IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

		public IEnumerator<Command> GetEnumerator()
		{
            foreach (HashedList<Command> commands in _InnerLists)
                foreach (Command command in commands)
                    yield return command;
		}

		#endregion

		#region Membres de ICollection

		public bool IsSynchronized
		{
			get
			{
				return false;
			}
		}

		public int Count
		{
			get
			{
				int count = 0;
				for(int i=0; i<_InnerLists.Length; i++)
					count += _InnerLists[i].Count;

				return count;
			}
		}

		public void CopyTo(Array array, int index)
		{
			throw new NotImplementedException();
		}

		public object SyncRoot
		{
			get
			{
				return _InnerLists;
			}
		}

		#endregion

        #region ICollection<Command> Members


        public void Clear()
        {
            foreach (HashedList<Command> commands in _InnerLists)
                commands.Clear();
        }

        public void CopyTo(Command[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(Command item)
        {
            return _InnerLists[item.ProcessOrder].Remove(item);
        }

        #endregion
    }
}
