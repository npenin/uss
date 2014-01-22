using System;

namespace Evaluant.Uss.MetaData
{
	public interface IMetaData
	{
		void Accept(IMetaDataVisitor visitor);
	}
}
