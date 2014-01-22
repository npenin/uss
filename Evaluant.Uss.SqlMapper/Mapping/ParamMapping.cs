using System;
using System.Xml.Serialization;

namespace Evaluant.Uss.SqlMapper
{
	/// <summary>
	/// Description résumée de PK_Attribute.
	/// </summary>
	public class ParamMapping : ITagMapping
	{
		private string _Name;
		private string _Value;
		
		public ParamMapping()
		{
		}

		public ParamMapping(string name, string value)
		{
			_Name = name;
			_Value = value;
		}

		[XmlAttribute("name", DataType="string")]
		public string Name
		{
			get { return _Name;}
			set { _Name = value;}
		}

		[XmlText]
		public string Value
		{
			get { return _Value;}
			set { _Value = value;}
		}
	}
}
