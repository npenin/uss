using System;
using System.Collections;

namespace Evaluant.Uss
{

	public class EntityEnumerator : IEnumerator
	{
		private bool _Current;
		private int _CurrentItem;
		private int _NumItems;

		private ArrayList _Entries;
 
		internal EntityEnumerator(ArrayList entries)
		{
			this._Entries = entries;

			this._NumItems = (entries.Count - 1);
			this._CurrentItem = -1;
			this._Current = false;
		}
 
		public bool MoveNext()
		{
			if (this._CurrentItem < this._NumItems)
			{
				++this._CurrentItem;
				this._Current = true;
			}
			else
			{
				this._Current = false;
			}

			return this._Current;
		}
 
		public void Reset()
		{
			this._CurrentItem = -1;
			this._Current = false;
		}
 
		public EntityEntry Current
		{
			get
			{
				if (!this._Current)
				{
					throw new InvalidOperationException("This collection can't be iterated");
				}

				return (EntityEntry)_Entries[_CurrentItem];
			}
		}
 
		object System.Collections.IEnumerator.Current
		{
			get
			{
				if (!this._Current)
				{
					throw new InvalidOperationException("This collection can't be iterated");
				}

				return (EntityEntry)_Entries[_CurrentItem];
			}
		}

	}
}
