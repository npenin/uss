using System;
using System.Data;
using System.Xml.Serialization;

namespace Evaluant.Uss.SqlMapper
{
	/// <summary>
	/// Description résumée de GeneratorMapping.
	/// </summary>
	public class GeneratorMapping : ITagMapping
	{
		public enum GeneratorType {guid, native, assigned, inherited, business};

		private GeneratorType _Name;
		private ParamMapping[] _Params;

       
		public GeneratorMapping()
		{
		}

		[XmlAttribute("name")]
		public GeneratorType Name
		{
			get { return _Name; }
			set { _Name = value; }
		}

		[XmlElement("Property")]
		public ParamMapping[] Params
		{
			get{return _Params;}
			set { _Params = value; }
		}

		public ParamMapping GetParam(string name)
		{
			if (_Params != null)
			{
				foreach(ParamMapping param in _Params)
				{
					if (param.Name == name)
						return param;
				}
			}
			return null;
		}

	}
}
