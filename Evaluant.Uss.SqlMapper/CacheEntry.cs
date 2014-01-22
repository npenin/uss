
namespace Evaluant.Uss.SqlMapper
{
	/// <summary>
	/// Description r�sum�e de CacheEntry.
	/// </summary>
	abstract class CacheEntry
	{
		private ITagMapping _Mapping;
		
		public CacheEntry(ITagMapping mapping)
		{
			_Mapping = mapping;
		}

		public ITagMapping Mapping
		{
			get { return _Mapping; }
		}
	}
}
