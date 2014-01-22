using System;
//using Evaluant.Uss.Common;
//using Evaluant.Uss.Metadata;

namespace Evaluant.Uss.MetaData
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class TypeMetaData : IMetaData
	{
		private string _Name;
		private string _ParentName;
		private bool _Ignore;
		private bool _IsInterface;

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeMetaData"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="parentName">Name of the parent.</param>
		public TypeMetaData(string name, string parentName, bool isInterface)
		{
			_Name = name;
			_ParentName = parentName;
			_IsInterface = isInterface;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeMetaData"/> class.
		/// </summary>
		/// <param name="type">The type.</param>
		/// <param name="parent">The parent.</param>
		public TypeMetaData(Type type, Type parent, bool isInterface)
		{
			_Name = TypeResolver.ConvertNamespaceDomainToEuss(type);
			_ParentName = TypeResolver.ConvertNamespaceDomainToEuss(parent);
			_IsInterface = isInterface;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TypeMetaData"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		public TypeMetaData(string name) : this(name, String.Empty, false)
		{
		}

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get { return _Name; }
			set { _Name = value; }
		}

		/// <summary>
		/// Gets or sets the name of the parent.
		/// </summary>
		/// <value>The name of the parent.</value>
		public string ParentName
		{
			get { return _ParentName; }
			set { _ParentName = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="TypeMetadata"/> is ignored.
		/// </summary>
		/// <value><c>true</c> if ignored; otherwise, <c>false</c>.</value>
		public bool Ignore
		{
			get { return _Ignore; }
			set { _Ignore = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this instance is interface.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is interface; otherwise, <c>false</c>.
		/// </value>
		public bool IsInterface
		{
			get { return _IsInterface; }
			set { _IsInterface = value; }
		}

		/// <summary>
		/// Accepts the specified visitor.
		/// </summary>
		/// <param name="visitor">The visitor.</param>
		void IMetaData.Accept(IMetaDataVisitor visitor)
		{
			visitor.Process(this);
		}

	}
}
