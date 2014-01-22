using Evaluant.Uss.Model;
using Evaluant.Uss.PersistenceEngine.Contracts;

namespace Evaluant.Uss.Xml
{
	/// <summary>
	/// Description résumée de XmlProvider.
	/// </summary>
	public class XmlProvider : PersistenceProviderImplementation
	{
		protected string _Filename;

		public override void InitializeConfiguration()
		{
		}

		public override IPersistenceEngine CreatePersistenceEngine()
		{
			XmlPersistenceEngine engine = new XmlPersistenceEngine(this);
            engine.Culture = _Culture;

			return engine;
		}

		public string FileName
		{
			get { return _Filename; }
			set 
			{
				_Filename = value;
			}
		}
	}
}
