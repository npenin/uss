using Evaluant.Uss.Models;
using Evaluant.Uss.Common;

namespace Evaluant.Uss.Xml
{
	/// <summary>
	/// Description résumée de XmlProvider.
	/// </summary>
	public class XmlProvider : PersistenceEngineFactoryImplementation
	{
		protected string _Filename;

		public override void InitializeConfiguration()
		{
		}

		public override IPersistenceEngine CreatePersistenceEngine()
		{
			XmlPersistenceEngine engine = new XmlPersistenceEngine(_Filename, _Model);
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
