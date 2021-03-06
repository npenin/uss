﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using DslModeling = global::Microsoft.VisualStudio.Modeling;
using DslDesign = global::Microsoft.VisualStudio.Modeling.Design;
namespace Evaluant.Uss.SqlMapper.Mapping
{
	/// <summary>
	/// DomainClass Mapping
	/// The root in which all other elements are embedded. Appears as a diagram.
	/// </summary>
	[DslDesign::DisplayNameResource("Evaluant.Uss.SqlMapper.Mapping.Mapping.DisplayName", typeof(global::Evaluant.Uss.SqlMapper.Mapping.SqlMapperMappingDomainModel), "Evaluant.Uss.SqlMapper.Mapping.GeneratedCode.DomainModelResx")]
	[DslDesign::DescriptionResource("Evaluant.Uss.SqlMapper.Mapping.Mapping.Description", typeof(global::Evaluant.Uss.SqlMapper.Mapping.SqlMapperMappingDomainModel), "Evaluant.Uss.SqlMapper.Mapping.GeneratedCode.DomainModelResx")]
	[DslModeling::DomainModelOwner(typeof(global::Evaluant.Uss.SqlMapper.Mapping.SqlMapperMappingDomainModel))]
	[global::System.CLSCompliant(true)]
	[DslModeling::DomainObjectId("b781fed7-e0c2-4f43-8877-d0f6b0c1d02f")]
	public partial class Mapping : DslModeling::ModelElement
	{
		#region Constructors, domain class Id
	
		/// <summary>
		/// Mapping domain class Id.
		/// </summary>
		public static readonly new global::System.Guid DomainClassId = new global::System.Guid(0xb781fed7, 0xe0c2, 0x4f43, 0x88, 0x77, 0xd0, 0xf6, 0xb0, 0xc1, 0xd0, 0x2f);
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="store">Store where new element is to be created.</param>
		/// <param name="propertyAssignments">List of domain property id/value pairs to set once the element is created.</param>
		public Mapping(DslModeling::Store store, params DslModeling::PropertyAssignment[] propertyAssignments)
			: this(store != null ? store.DefaultPartitionForClass(DomainClassId) : null, propertyAssignments)
		{
		}
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="partition">Partition where new element is to be created.</param>
		/// <param name="propertyAssignments">List of domain property id/value pairs to set once the element is created.</param>
		public Mapping(DslModeling::Partition partition, params DslModeling::PropertyAssignment[] propertyAssignments)
			: base(partition, propertyAssignments)
		{
		}
		#endregion
		#region Entities opposite domain role accessor
		
		/// <summary>
		/// Gets a list of Entities.
		/// Description for Evaluant.Uss.SqlMapper.Mapping.MappingHasEntities.Mapping
		/// </summary>
		public virtual DslModeling::LinkedElementCollection<Entity> Entities
		{
			[global::System.Diagnostics.DebuggerStepThrough]
			get
			{
				return GetRoleCollection<DslModeling::LinkedElementCollection<Entity>, Entity>(global::Evaluant.Uss.SqlMapper.Mapping.MappingHasEntities.MappingDomainRoleId);
			}
		}
		#endregion
		#region ElementGroupPrototype Merge methods
		/// <summary>
		/// Returns a value indicating whether the source element represented by the
		/// specified root ProtoElement can be added to this element.
		/// </summary>
		/// <param name="rootElement">
		/// The root ProtoElement representing a source element.  This can be null, 
		/// in which case the ElementGroupPrototype does not contain an ProtoElements
		/// and the code should inspect the ElementGroupPrototype context information.
		/// </param>
		/// <param name="elementGroupPrototype">The ElementGroupPrototype that contains the root ProtoElement.</param>
		/// <returns>true if the source element represented by the ProtoElement can be added to this target element.</returns>
		protected override bool CanMerge(DslModeling::ProtoElementBase rootElement, DslModeling::ElementGroupPrototype elementGroupPrototype)
		{
			if ( elementGroupPrototype == null ) throw new global::System.ArgumentNullException("elementGroupPrototype");
			
			if (rootElement != null)
			{
				DslModeling::DomainClassInfo rootElementDomainInfo = this.Partition.DomainDataDirectory.GetDomainClass(rootElement.DomainClassId);
				
				if (rootElementDomainInfo.IsDerivedFrom(global::Evaluant.Uss.SqlMapper.Mapping.Entity.DomainClassId)) 
				{
					return true;
				}
			}
			return base.CanMerge(rootElement, elementGroupPrototype);
		}
		
		/// <summary>
		/// Called by the Merge process to create a relationship between 
		/// this target element and the specified source element. 
		/// Typically, a parent-child relationship is established
		/// between the target element (the parent) and the source element 
		/// (the child), but any relationship can be established.
		/// </summary>
		/// <param name="sourceElement">The element that is to be related to this model element.</param>
		/// <param name="elementGroup">The group of source ModelElements that have been rehydrated into the target store.</param>
		/// <remarks>
		/// This method is overriden to create the relationship between the target element and the specified source element.
		/// The base method does nothing.
		/// </remarks>
		protected override void MergeRelate(DslModeling::ModelElement sourceElement, DslModeling::ElementGroup elementGroup)
		{
			// In general, sourceElement is allowed to be null, meaning that the elementGroup must be parsed for special cases.
			// However this is not supported in generated code.  Use double-deriving on this class and then override MergeRelate completely if you 
			// need to support this case.
			if ( sourceElement == null ) throw new global::System.ArgumentNullException("sourceElement");
		
				
			global::Evaluant.Uss.SqlMapper.Mapping.Entity sourceEntity1 = sourceElement as global::Evaluant.Uss.SqlMapper.Mapping.Entity;
			if (sourceEntity1 != null)
			{
				// Create link for path MappingHasEntities.Entities
				this.Entities.Add(sourceEntity1);

				return;
			}
		
			// Sdk workaround to runtime bug #879350 (DSL: can't copy and paste a MEL that has a MEX). Avoid MergeRelate on ModelElementExtension
			// during a "Paste".
			if (sourceElement is DslModeling::ExtensionElement
				&& sourceElement.Store.TransactionManager.CurrentTransaction.TopLevelTransaction.Context.ContextInfo.ContainsKey("{9DAFD42A-DC0E-4d78-8C3F-8266B2CF8B33}"))
			{
				return;
			}
		
			// Fall through to base class if this class hasn't handled the merge.
			base.MergeRelate(sourceElement, elementGroup);
		}
		
		/// <summary>
		/// Performs operation opposite to MergeRelate - i.e. disconnects a given
		/// element from the current one (removes links created by MergeRelate).
		/// </summary>
		/// <param name="sourceElement">Element to be unmerged/disconnected.</param>
		[global::System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		protected override void MergeDisconnect(DslModeling::ModelElement sourceElement)
		{
			if (sourceElement == null) throw new global::System.ArgumentNullException("sourceElement");
				
			global::Evaluant.Uss.SqlMapper.Mapping.Entity sourceEntity1 = sourceElement as global::Evaluant.Uss.SqlMapper.Mapping.Entity;
			if (sourceEntity1 != null)
			{
				// Delete link for path MappingHasEntities.Entities
				
				foreach (DslModeling::ElementLink link in global::Evaluant.Uss.SqlMapper.Mapping.MappingHasEntities.GetLinks((global::Evaluant.Uss.SqlMapper.Mapping.Mapping)this, sourceEntity1))
				{
					// Delete the link, but without possible delete propagation to the element since it's moving to a new location.
					link.Delete(global::Evaluant.Uss.SqlMapper.Mapping.MappingHasEntities.MappingDomainRoleId, global::Evaluant.Uss.SqlMapper.Mapping.MappingHasEntities.EntityDomainRoleId);
				}

				return;
			}
			// Fall through to base class if this class hasn't handled the unmerge.
			base.MergeDisconnect(sourceElement);
		}
		#endregion
	}
}
namespace Evaluant.Uss.SqlMapper.Mapping
{
	/// <summary>
	/// DomainClass Entity
	/// Description for Evaluant.Uss.SqlMapper.Mapping.Entity
	/// </summary>
	[DslDesign::DisplayNameResource("Evaluant.Uss.SqlMapper.Mapping.Entity.DisplayName", typeof(global::Evaluant.Uss.SqlMapper.Mapping.SqlMapperMappingDomainModel), "Evaluant.Uss.SqlMapper.Mapping.GeneratedCode.DomainModelResx")]
	[DslDesign::DescriptionResource("Evaluant.Uss.SqlMapper.Mapping.Entity.Description", typeof(global::Evaluant.Uss.SqlMapper.Mapping.SqlMapperMappingDomainModel), "Evaluant.Uss.SqlMapper.Mapping.GeneratedCode.DomainModelResx")]
	[DslModeling::DomainModelOwner(typeof(global::Evaluant.Uss.SqlMapper.Mapping.SqlMapperMappingDomainModel))]
	[global::System.CLSCompliant(true)]
	[DslModeling::DomainObjectId("26f9fe6b-d5c8-4e3b-a4cc-91e64c3f81e2")]
	public partial class Entity : DslModeling::ModelElement
	{
		#region Constructors, domain class Id
	
		/// <summary>
		/// Entity domain class Id.
		/// </summary>
		public static readonly new global::System.Guid DomainClassId = new global::System.Guid(0x26f9fe6b, 0xd5c8, 0x4e3b, 0xa4, 0xcc, 0x91, 0xe6, 0x4c, 0x3f, 0x81, 0xe2);
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="store">Store where new element is to be created.</param>
		/// <param name="propertyAssignments">List of domain property id/value pairs to set once the element is created.</param>
		public Entity(DslModeling::Store store, params DslModeling::PropertyAssignment[] propertyAssignments)
			: this(store != null ? store.DefaultPartitionForClass(DomainClassId) : null, propertyAssignments)
		{
		}
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="partition">Partition where new element is to be created.</param>
		/// <param name="propertyAssignments">List of domain property id/value pairs to set once the element is created.</param>
		public Entity(DslModeling::Partition partition, params DslModeling::PropertyAssignment[] propertyAssignments)
			: base(partition, propertyAssignments)
		{
		}
		#endregion
		#region References opposite domain role accessor
		
		/// <summary>
		/// Gets a list of References.
		/// Description for Evaluant.Uss.SqlMapper.Mapping.EntityHasReferences.Entity
		/// </summary>
		public virtual DslModeling::LinkedElementCollection<Reference> References
		{
			[global::System.Diagnostics.DebuggerStepThrough]
			get
			{
				return GetRoleCollection<DslModeling::LinkedElementCollection<Reference>, Reference>(global::Evaluant.Uss.SqlMapper.Mapping.EntityHasReferences.EntityDomainRoleId);
			}
		}
		#endregion
		#region Attributes opposite domain role accessor
		
		/// <summary>
		/// Gets a list of Attributes.
		/// Description for Evaluant.Uss.SqlMapper.Mapping.EntityHasAttributes.Entity
		/// </summary>
		public virtual DslModeling::LinkedElementCollection<Attribute> Attributes
		{
			[global::System.Diagnostics.DebuggerStepThrough]
			get
			{
				return GetRoleCollection<DslModeling::LinkedElementCollection<Attribute>, Attribute>(global::Evaluant.Uss.SqlMapper.Mapping.EntityHasAttributes.EntityDomainRoleId);
			}
		}
		#endregion
		#region Mapping opposite domain role accessor
		/// <summary>
		/// Gets or sets Mapping.
		/// Description for Evaluant.Uss.SqlMapper.Mapping.MappingHasEntities.Entity
		/// </summary>
		public virtual Mapping Mapping
		{
			[global::System.Diagnostics.DebuggerStepThrough]
			get
			{
				return DslModeling::DomainRoleInfo.GetLinkedElement(this, global::Evaluant.Uss.SqlMapper.Mapping.MappingHasEntities.EntityDomainRoleId) as Mapping;
			}
			[global::System.Diagnostics.DebuggerStepThrough]
			set
			{
				DslModeling::DomainRoleInfo.SetLinkedElement(this, global::Evaluant.Uss.SqlMapper.Mapping.MappingHasEntities.EntityDomainRoleId, value);
			}
		}
		#endregion
		#region ElementGroupPrototype Merge methods
		/// <summary>
		/// Returns a value indicating whether the source element represented by the
		/// specified root ProtoElement can be added to this element.
		/// </summary>
		/// <param name="rootElement">
		/// The root ProtoElement representing a source element.  This can be null, 
		/// in which case the ElementGroupPrototype does not contain an ProtoElements
		/// and the code should inspect the ElementGroupPrototype context information.
		/// </param>
		/// <param name="elementGroupPrototype">The ElementGroupPrototype that contains the root ProtoElement.</param>
		/// <returns>true if the source element represented by the ProtoElement can be added to this target element.</returns>
		protected override bool CanMerge(DslModeling::ProtoElementBase rootElement, DslModeling::ElementGroupPrototype elementGroupPrototype)
		{
			if ( elementGroupPrototype == null ) throw new global::System.ArgumentNullException("elementGroupPrototype");
			
			if (rootElement != null)
			{
				DslModeling::DomainClassInfo rootElementDomainInfo = this.Partition.DomainDataDirectory.GetDomainClass(rootElement.DomainClassId);
				
				if (rootElementDomainInfo.IsDerivedFrom(global::Evaluant.Uss.SqlMapper.Mapping.Reference.DomainClassId)) 
				{
					return true;
				}
				
				if (rootElementDomainInfo.IsDerivedFrom(global::Evaluant.Uss.SqlMapper.Mapping.Attribute.DomainClassId)) 
				{
					return true;
				}
			}
			return base.CanMerge(rootElement, elementGroupPrototype);
		}
		
		/// <summary>
		/// Called by the Merge process to create a relationship between 
		/// this target element and the specified source element. 
		/// Typically, a parent-child relationship is established
		/// between the target element (the parent) and the source element 
		/// (the child), but any relationship can be established.
		/// </summary>
		/// <param name="sourceElement">The element that is to be related to this model element.</param>
		/// <param name="elementGroup">The group of source ModelElements that have been rehydrated into the target store.</param>
		/// <remarks>
		/// This method is overriden to create the relationship between the target element and the specified source element.
		/// The base method does nothing.
		/// </remarks>
		protected override void MergeRelate(DslModeling::ModelElement sourceElement, DslModeling::ElementGroup elementGroup)
		{
			// In general, sourceElement is allowed to be null, meaning that the elementGroup must be parsed for special cases.
			// However this is not supported in generated code.  Use double-deriving on this class and then override MergeRelate completely if you 
			// need to support this case.
			if ( sourceElement == null ) throw new global::System.ArgumentNullException("sourceElement");
		
				
			global::Evaluant.Uss.SqlMapper.Mapping.Reference sourceReference1 = sourceElement as global::Evaluant.Uss.SqlMapper.Mapping.Reference;
			if (sourceReference1 != null)
			{
				// Create link for path EntityHasReferences.References
				this.References.Add(sourceReference1);

				return;
			}
				
			global::Evaluant.Uss.SqlMapper.Mapping.Attribute sourceAttribute2 = sourceElement as global::Evaluant.Uss.SqlMapper.Mapping.Attribute;
			if (sourceAttribute2 != null)
			{
				// Create link for path EntityHasAttributes.Attributes
				this.Attributes.Add(sourceAttribute2);

				return;
			}
		
			// Sdk workaround to runtime bug #879350 (DSL: can't copy and paste a MEL that has a MEX). Avoid MergeRelate on ModelElementExtension
			// during a "Paste".
			if (sourceElement is DslModeling::ExtensionElement
				&& sourceElement.Store.TransactionManager.CurrentTransaction.TopLevelTransaction.Context.ContextInfo.ContainsKey("{9DAFD42A-DC0E-4d78-8C3F-8266B2CF8B33}"))
			{
				return;
			}
		
			// Fall through to base class if this class hasn't handled the merge.
			base.MergeRelate(sourceElement, elementGroup);
		}
		
		/// <summary>
		/// Performs operation opposite to MergeRelate - i.e. disconnects a given
		/// element from the current one (removes links created by MergeRelate).
		/// </summary>
		/// <param name="sourceElement">Element to be unmerged/disconnected.</param>
		[global::System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
		protected override void MergeDisconnect(DslModeling::ModelElement sourceElement)
		{
			if (sourceElement == null) throw new global::System.ArgumentNullException("sourceElement");
				
			global::Evaluant.Uss.SqlMapper.Mapping.Reference sourceReference1 = sourceElement as global::Evaluant.Uss.SqlMapper.Mapping.Reference;
			if (sourceReference1 != null)
			{
				// Delete link for path EntityHasReferences.References
				
				foreach (DslModeling::ElementLink link in global::Evaluant.Uss.SqlMapper.Mapping.EntityHasReferences.GetLinks((global::Evaluant.Uss.SqlMapper.Mapping.Entity)this, sourceReference1))
				{
					// Delete the link, but without possible delete propagation to the element since it's moving to a new location.
					link.Delete(global::Evaluant.Uss.SqlMapper.Mapping.EntityHasReferences.EntityDomainRoleId, global::Evaluant.Uss.SqlMapper.Mapping.EntityHasReferences.ReferenceDomainRoleId);
				}

				return;
			}
				
			global::Evaluant.Uss.SqlMapper.Mapping.Attribute sourceAttribute2 = sourceElement as global::Evaluant.Uss.SqlMapper.Mapping.Attribute;
			if (sourceAttribute2 != null)
			{
				// Delete link for path EntityHasAttributes.Attributes
				
				foreach (DslModeling::ElementLink link in global::Evaluant.Uss.SqlMapper.Mapping.EntityHasAttributes.GetLinks((global::Evaluant.Uss.SqlMapper.Mapping.Entity)this, sourceAttribute2))
				{
					// Delete the link, but without possible delete propagation to the element since it's moving to a new location.
					link.Delete(global::Evaluant.Uss.SqlMapper.Mapping.EntityHasAttributes.EntityDomainRoleId, global::Evaluant.Uss.SqlMapper.Mapping.EntityHasAttributes.AttributeDomainRoleId);
				}

				return;
			}
			// Fall through to base class if this class hasn't handled the unmerge.
			base.MergeDisconnect(sourceElement);
		}
		#endregion
	}
}
namespace Evaluant.Uss.SqlMapper.Mapping
{
	/// <summary>
	/// DomainClass Reference
	/// Description for Evaluant.Uss.SqlMapper.Mapping.Reference
	/// </summary>
	[DslDesign::DisplayNameResource("Evaluant.Uss.SqlMapper.Mapping.Reference.DisplayName", typeof(global::Evaluant.Uss.SqlMapper.Mapping.SqlMapperMappingDomainModel), "Evaluant.Uss.SqlMapper.Mapping.GeneratedCode.DomainModelResx")]
	[DslDesign::DescriptionResource("Evaluant.Uss.SqlMapper.Mapping.Reference.Description", typeof(global::Evaluant.Uss.SqlMapper.Mapping.SqlMapperMappingDomainModel), "Evaluant.Uss.SqlMapper.Mapping.GeneratedCode.DomainModelResx")]
	[DslModeling::DomainModelOwner(typeof(global::Evaluant.Uss.SqlMapper.Mapping.SqlMapperMappingDomainModel))]
	[global::System.CLSCompliant(true)]
	[global::System.Diagnostics.DebuggerDisplay("{GetType().Name,nq} (Name = {namePropertyStorage})")]
	[DslModeling::DomainObjectId("82e5253f-a09f-4ebd-b1a0-932581beb50b")]
	public partial class Reference : DslModeling::ModelElement
	{
		#region Constructors, domain class Id
	
		/// <summary>
		/// Reference domain class Id.
		/// </summary>
		public static readonly new global::System.Guid DomainClassId = new global::System.Guid(0x82e5253f, 0xa09f, 0x4ebd, 0xb1, 0xa0, 0x93, 0x25, 0x81, 0xbe, 0xb5, 0x0b);
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="store">Store where new element is to be created.</param>
		/// <param name="propertyAssignments">List of domain property id/value pairs to set once the element is created.</param>
		public Reference(DslModeling::Store store, params DslModeling::PropertyAssignment[] propertyAssignments)
			: this(store != null ? store.DefaultPartitionForClass(DomainClassId) : null, propertyAssignments)
		{
		}
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="partition">Partition where new element is to be created.</param>
		/// <param name="propertyAssignments">List of domain property id/value pairs to set once the element is created.</param>
		public Reference(DslModeling::Partition partition, params DslModeling::PropertyAssignment[] propertyAssignments)
			: base(partition, propertyAssignments)
		{
		}
		#endregion
		#region Name domain property code
		
		/// <summary>
		/// Name domain property Id.
		/// </summary>
		public static readonly global::System.Guid NameDomainPropertyId = new global::System.Guid(0xed0e7ca9, 0x1c3a, 0x4e52, 0xbe, 0x16, 0x36, 0x6d, 0x07, 0x70, 0x99, 0x01);
		
		/// <summary>
		/// Storage for Name
		/// </summary>
		private global::System.String namePropertyStorage = string.Empty;
		
		/// <summary>
		/// Gets or sets the value of Name domain property.
		/// Description for Evaluant.Uss.SqlMapper.Mapping.Reference.Name
		/// </summary>
		[DslDesign::DisplayNameResource("Evaluant.Uss.SqlMapper.Mapping.Reference/Name.DisplayName", typeof(global::Evaluant.Uss.SqlMapper.Mapping.SqlMapperMappingDomainModel), "Evaluant.Uss.SqlMapper.Mapping.GeneratedCode.DomainModelResx")]
		[DslDesign::DescriptionResource("Evaluant.Uss.SqlMapper.Mapping.Reference/Name.Description", typeof(global::Evaluant.Uss.SqlMapper.Mapping.SqlMapperMappingDomainModel), "Evaluant.Uss.SqlMapper.Mapping.GeneratedCode.DomainModelResx")]
		[DslModeling::ElementName]
		[DslModeling::DomainObjectId("ed0e7ca9-1c3a-4e52-be16-366d07709901")]
		public global::System.String Name
		{
			[global::System.Diagnostics.DebuggerStepThrough]
			get
			{
				return namePropertyStorage;
			}
			[global::System.Diagnostics.DebuggerStepThrough]
			set
			{
				NamePropertyHandler.Instance.SetValue(this, value);
			}
		}
		/// <summary>
		/// Value handler for the Reference.Name domain property.
		/// </summary>
		internal sealed partial class NamePropertyHandler : DslModeling::DomainPropertyValueHandler<Reference, global::System.String>
		{
			private NamePropertyHandler() { }
		
			/// <summary>
			/// Gets the singleton instance of the Reference.Name domain property value handler.
			/// </summary>
			public static readonly NamePropertyHandler Instance = new NamePropertyHandler();
		
			/// <summary>
			/// Gets the Id of the Reference.Name domain property.
			/// </summary>
			public sealed override global::System.Guid DomainPropertyId
			{
				[global::System.Diagnostics.DebuggerStepThrough]
				get
				{
					return NameDomainPropertyId;
				}
			}
			
			/// <summary>
			/// Gets a strongly-typed value of the property on specified element.
			/// </summary>
			/// <param name="element">Element which owns the property.</param>
			/// <returns>Property value.</returns>
			public override sealed global::System.String GetValue(Reference element)
			{
				if (element == null) throw new global::System.ArgumentNullException("element");
				return element.namePropertyStorage;
			}
		
			/// <summary>
			/// Sets property value on an element.
			/// </summary>
			/// <param name="element">Element which owns the property.</param>
			/// <param name="newValue">New property value.</param>
			public override sealed void SetValue(Reference element, global::System.String newValue)
			{
				if (element == null) throw new global::System.ArgumentNullException("element");
		
				global::System.String oldValue = GetValue(element);
				if (newValue != oldValue)
				{
					ValueChanging(element, oldValue, newValue);
					element.namePropertyStorage = newValue;
					ValueChanged(element, oldValue, newValue);
				}
			}
		}
		
		#endregion
		#region Entity opposite domain role accessor
		/// <summary>
		/// Gets or sets Entity.
		/// Description for Evaluant.Uss.SqlMapper.Mapping.EntityHasReferences.Reference
		/// </summary>
		public virtual Entity Entity
		{
			[global::System.Diagnostics.DebuggerStepThrough]
			get
			{
				return DslModeling::DomainRoleInfo.GetLinkedElement(this, global::Evaluant.Uss.SqlMapper.Mapping.EntityHasReferences.ReferenceDomainRoleId) as Entity;
			}
			[global::System.Diagnostics.DebuggerStepThrough]
			set
			{
				DslModeling::DomainRoleInfo.SetLinkedElement(this, global::Evaluant.Uss.SqlMapper.Mapping.EntityHasReferences.ReferenceDomainRoleId, value);
			}
		}
		#endregion
	}
}
namespace Evaluant.Uss.SqlMapper.Mapping
{
	/// <summary>
	/// DomainClass Attribute
	/// Description for Evaluant.Uss.SqlMapper.Mapping.Attribute
	/// </summary>
	[DslDesign::DisplayNameResource("Evaluant.Uss.SqlMapper.Mapping.Attribute.DisplayName", typeof(global::Evaluant.Uss.SqlMapper.Mapping.SqlMapperMappingDomainModel), "Evaluant.Uss.SqlMapper.Mapping.GeneratedCode.DomainModelResx")]
	[DslDesign::DescriptionResource("Evaluant.Uss.SqlMapper.Mapping.Attribute.Description", typeof(global::Evaluant.Uss.SqlMapper.Mapping.SqlMapperMappingDomainModel), "Evaluant.Uss.SqlMapper.Mapping.GeneratedCode.DomainModelResx")]
	[DslModeling::DomainModelOwner(typeof(global::Evaluant.Uss.SqlMapper.Mapping.SqlMapperMappingDomainModel))]
	[global::System.CLSCompliant(true)]
	[global::System.Diagnostics.DebuggerDisplay("{GetType().Name,nq} (Name = {namePropertyStorage})")]
	[DslModeling::DomainObjectId("167ef8d9-ab65-4133-ba0a-abeb1c22f56f")]
	public partial class Attribute : DslModeling::ModelElement
	{
		#region Constructors, domain class Id
	
		/// <summary>
		/// Attribute domain class Id.
		/// </summary>
		public static readonly new global::System.Guid DomainClassId = new global::System.Guid(0x167ef8d9, 0xab65, 0x4133, 0xba, 0x0a, 0xab, 0xeb, 0x1c, 0x22, 0xf5, 0x6f);
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="store">Store where new element is to be created.</param>
		/// <param name="propertyAssignments">List of domain property id/value pairs to set once the element is created.</param>
		public Attribute(DslModeling::Store store, params DslModeling::PropertyAssignment[] propertyAssignments)
			: this(store != null ? store.DefaultPartitionForClass(DomainClassId) : null, propertyAssignments)
		{
		}
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="partition">Partition where new element is to be created.</param>
		/// <param name="propertyAssignments">List of domain property id/value pairs to set once the element is created.</param>
		public Attribute(DslModeling::Partition partition, params DslModeling::PropertyAssignment[] propertyAssignments)
			: base(partition, propertyAssignments)
		{
		}
		#endregion
		#region Name domain property code
		
		/// <summary>
		/// Name domain property Id.
		/// </summary>
		public static readonly global::System.Guid NameDomainPropertyId = new global::System.Guid(0x3fe95160, 0xb033, 0x4d6e, 0x80, 0xb7, 0x5d, 0xe2, 0xd0, 0xbd, 0x28, 0x8b);
		
		/// <summary>
		/// Storage for Name
		/// </summary>
		private global::System.String namePropertyStorage = string.Empty;
		
		/// <summary>
		/// Gets or sets the value of Name domain property.
		/// Description for Evaluant.Uss.SqlMapper.Mapping.Attribute.Name
		/// </summary>
		[DslDesign::DisplayNameResource("Evaluant.Uss.SqlMapper.Mapping.Attribute/Name.DisplayName", typeof(global::Evaluant.Uss.SqlMapper.Mapping.SqlMapperMappingDomainModel), "Evaluant.Uss.SqlMapper.Mapping.GeneratedCode.DomainModelResx")]
		[DslDesign::DescriptionResource("Evaluant.Uss.SqlMapper.Mapping.Attribute/Name.Description", typeof(global::Evaluant.Uss.SqlMapper.Mapping.SqlMapperMappingDomainModel), "Evaluant.Uss.SqlMapper.Mapping.GeneratedCode.DomainModelResx")]
		[DslModeling::ElementName]
		[DslModeling::DomainObjectId("3fe95160-b033-4d6e-80b7-5de2d0bd288b")]
		public global::System.String Name
		{
			[global::System.Diagnostics.DebuggerStepThrough]
			get
			{
				return namePropertyStorage;
			}
			[global::System.Diagnostics.DebuggerStepThrough]
			set
			{
				NamePropertyHandler.Instance.SetValue(this, value);
			}
		}
		/// <summary>
		/// Value handler for the Attribute.Name domain property.
		/// </summary>
		internal sealed partial class NamePropertyHandler : DslModeling::DomainPropertyValueHandler<Attribute, global::System.String>
		{
			private NamePropertyHandler() { }
		
			/// <summary>
			/// Gets the singleton instance of the Attribute.Name domain property value handler.
			/// </summary>
			public static readonly NamePropertyHandler Instance = new NamePropertyHandler();
		
			/// <summary>
			/// Gets the Id of the Attribute.Name domain property.
			/// </summary>
			public sealed override global::System.Guid DomainPropertyId
			{
				[global::System.Diagnostics.DebuggerStepThrough]
				get
				{
					return NameDomainPropertyId;
				}
			}
			
			/// <summary>
			/// Gets a strongly-typed value of the property on specified element.
			/// </summary>
			/// <param name="element">Element which owns the property.</param>
			/// <returns>Property value.</returns>
			public override sealed global::System.String GetValue(Attribute element)
			{
				if (element == null) throw new global::System.ArgumentNullException("element");
				return element.namePropertyStorage;
			}
		
			/// <summary>
			/// Sets property value on an element.
			/// </summary>
			/// <param name="element">Element which owns the property.</param>
			/// <param name="newValue">New property value.</param>
			public override sealed void SetValue(Attribute element, global::System.String newValue)
			{
				if (element == null) throw new global::System.ArgumentNullException("element");
		
				global::System.String oldValue = GetValue(element);
				if (newValue != oldValue)
				{
					ValueChanging(element, oldValue, newValue);
					element.namePropertyStorage = newValue;
					ValueChanged(element, oldValue, newValue);
				}
			}
		}
		
		#endregion
		#region Entity opposite domain role accessor
		/// <summary>
		/// Gets or sets Entity.
		/// Description for Evaluant.Uss.SqlMapper.Mapping.EntityHasAttributes.Attribute
		/// </summary>
		public virtual Entity Entity
		{
			[global::System.Diagnostics.DebuggerStepThrough]
			get
			{
				return DslModeling::DomainRoleInfo.GetLinkedElement(this, global::Evaluant.Uss.SqlMapper.Mapping.EntityHasAttributes.AttributeDomainRoleId) as Entity;
			}
			[global::System.Diagnostics.DebuggerStepThrough]
			set
			{
				DslModeling::DomainRoleInfo.SetLinkedElement(this, global::Evaluant.Uss.SqlMapper.Mapping.EntityHasAttributes.AttributeDomainRoleId, value);
			}
		}
		#endregion
	}
}
