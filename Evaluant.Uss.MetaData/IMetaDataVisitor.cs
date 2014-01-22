using System;
using Evaluant.Uss.MetaData;

namespace Evaluant.Uss.MetaData
{
	public interface IMetaDataVisitor
	{
		void Process(ReferenceMetaData composition);
		void Process(TypeMetaData type);
        void Process(PropertyMetaData property);
        void Process(InterfaceMetadata interf);
    }
}
