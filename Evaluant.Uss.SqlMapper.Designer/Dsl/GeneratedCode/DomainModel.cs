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
using DslDiagrams = global::Microsoft.VisualStudio.Modeling.Diagrams;
namespace Evaluant.Uss.SqlMapper.Mapping
{
	/// <summary>
	/// DomainModel SqlMapperMappingDomainModel
	/// Description for Evaluant.Uss.SqlMapper.Mapping.SqlMapperMapping
	/// </summary>
	[DslDesign::DisplayNameResource("Evaluant.Uss.SqlMapper.Mapping.SqlMapperMappingDomainModel.DisplayName", typeof(global::Evaluant.Uss.SqlMapper.Mapping.SqlMapperMappingDomainModel), "Evaluant.Uss.SqlMapper.Mapping.GeneratedCode.DomainModelResx")]
	[DslDesign::DescriptionResource("Evaluant.Uss.SqlMapper.Mapping.SqlMapperMappingDomainModel.Description", typeof(global::Evaluant.Uss.SqlMapper.Mapping.SqlMapperMappingDomainModel), "Evaluant.Uss.SqlMapper.Mapping.GeneratedCode.DomainModelResx")]
	[global::System.CLSCompliant(true)]
	[DslModeling::DependsOnDomainModel(typeof(global::Microsoft.VisualStudio.Modeling.CoreDomainModel))]
	[DslModeling::DependsOnDomainModel(typeof(global::Microsoft.VisualStudio.Modeling.Diagrams.CoreDesignSurfaceDomainModel))]
	[global::System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Generated code.")]
	[DslModeling::DomainObjectId("3a8d209f-f0eb-4205-8a82-b01f252eff10")]
	public partial class SqlMapperMappingDomainModel : DslModeling::DomainModel
	{
		#region Constructor, domain model Id
	
		/// <summary>
		/// SqlMapperMappingDomainModel domain model Id.
		/// </summary>
		public static readonly global::System.Guid DomainModelId = new global::System.Guid(0x3a8d209f, 0xf0eb, 0x4205, 0x8a, 0x82, 0xb0, 0x1f, 0x25, 0x2e, 0xff, 0x10);
	
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="store">Store containing the domain model.</param>
		public SqlMapperMappingDomainModel(DslModeling::Store store)
			: base(store, DomainModelId)
		{
			// Call the partial method to allow any required serialization setup to be done.
			this.InitializeSerialization(store);		
		}
		
	
		///<Summary>
		/// Defines a partial method that will be called from the constructor to
		/// allow any necessary serialization setup to be done.
		///</Summary>
		///<remarks>
		/// For a DSL created with the DSL Designer wizard, an implementation of this 
		/// method will be generated in the GeneratedCode\SerializationHelper.cs class.
		///</remarks>
		partial void InitializeSerialization(DslModeling::Store store);
	
	
		#endregion
		#region Domain model reflection
			
		/// <summary>
		/// Gets the list of generated domain model types (classes, rules, relationships).
		/// </summary>
		/// <returns>List of types.</returns>
		[global::System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Generated code.")]	
		protected sealed override global::System.Type[] GetGeneratedDomainModelTypes()
		{
			return new global::System.Type[]
			{
				typeof(Mapping),
				typeof(Entity),
				typeof(Reference),
				typeof(Attribute),
				typeof(EntityHasReferences),
				typeof(EntityHasAttributes),
				typeof(SqlMapperMappingDiagramReferencesEntityShapes),
				typeof(EntityShapeReferencesRelationshipshape),
				typeof(MappingHasEntities),
				typeof(SqlMapperMappingDiagram),
				typeof(Relationshipshape),
				typeof(EntityShape),
				typeof(global::Evaluant.Uss.SqlMapper.Mapping.FixUpDiagram),
			};
		}
		/// <summary>
		/// Gets the list of generated domain properties.
		/// </summary>
		/// <returns>List of property data.</returns>
		[global::System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Generated code.")]	
		protected sealed override DomainMemberInfo[] GetGeneratedDomainProperties()
		{
			return new DomainMemberInfo[]
			{
				new DomainMemberInfo(typeof(Reference), "Name", Reference.NameDomainPropertyId, typeof(Reference.NamePropertyHandler)),
				new DomainMemberInfo(typeof(Attribute), "Name", Attribute.NameDomainPropertyId, typeof(Attribute.NamePropertyHandler)),
			};
		}
		/// <summary>
		/// Gets the list of generated domain roles.
		/// </summary>
		/// <returns>List of role data.</returns>
		protected sealed override DomainRolePlayerInfo[] GetGeneratedDomainRoles()
		{
			return new DomainRolePlayerInfo[]
			{
				new DomainRolePlayerInfo(typeof(EntityHasReferences), "Entity", EntityHasReferences.EntityDomainRoleId),
				new DomainRolePlayerInfo(typeof(EntityHasReferences), "Reference", EntityHasReferences.ReferenceDomainRoleId),
				new DomainRolePlayerInfo(typeof(EntityHasAttributes), "Entity", EntityHasAttributes.EntityDomainRoleId),
				new DomainRolePlayerInfo(typeof(EntityHasAttributes), "Attribute", EntityHasAttributes.AttributeDomainRoleId),
				new DomainRolePlayerInfo(typeof(SqlMapperMappingDiagramReferencesEntityShapes), "SqlMapperMappingDiagram", SqlMapperMappingDiagramReferencesEntityShapes.SqlMapperMappingDiagramDomainRoleId),
				new DomainRolePlayerInfo(typeof(SqlMapperMappingDiagramReferencesEntityShapes), "EntityShape", SqlMapperMappingDiagramReferencesEntityShapes.EntityShapeDomainRoleId),
				new DomainRolePlayerInfo(typeof(EntityShapeReferencesRelationshipshape), "EntityShape", EntityShapeReferencesRelationshipshape.EntityShapeDomainRoleId),
				new DomainRolePlayerInfo(typeof(EntityShapeReferencesRelationshipshape), "Relationshipshape", EntityShapeReferencesRelationshipshape.RelationshipshapeDomainRoleId),
				new DomainRolePlayerInfo(typeof(MappingHasEntities), "Mapping", MappingHasEntities.MappingDomainRoleId),
				new DomainRolePlayerInfo(typeof(MappingHasEntities), "Entity", MappingHasEntities.EntityDomainRoleId),
			};
		}
		#endregion
		#region Factory methods
		private static global::System.Collections.Generic.Dictionary<global::System.Type, int> createElementMap;
	
		/// <summary>
		/// Creates an element of specified type.
		/// </summary>
		/// <param name="partition">Partition where element is to be created.</param>
		/// <param name="elementType">Element type which belongs to this domain model.</param>
		/// <param name="propertyAssignments">New element property assignments.</param>
		/// <returns>Created element.</returns>
		[global::System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[global::System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "Generated code.")]	
		public sealed override DslModeling::ModelElement CreateElement(DslModeling::Partition partition, global::System.Type elementType, DslModeling::PropertyAssignment[] propertyAssignments)
		{
			if (elementType == null) throw new global::System.ArgumentNullException("elementType");
	
			if (createElementMap == null)
			{
				createElementMap = new global::System.Collections.Generic.Dictionary<global::System.Type, int>(7);
				createElementMap.Add(typeof(Mapping), 0);
				createElementMap.Add(typeof(Entity), 1);
				createElementMap.Add(typeof(Reference), 2);
				createElementMap.Add(typeof(Attribute), 3);
				createElementMap.Add(typeof(SqlMapperMappingDiagram), 4);
				createElementMap.Add(typeof(Relationshipshape), 5);
				createElementMap.Add(typeof(EntityShape), 6);
			}
			int index;
			if (!createElementMap.TryGetValue(elementType, out index))
			{
				// construct exception error message		
				string exceptionError = string.Format(
								global::System.Globalization.CultureInfo.CurrentCulture,
								global::Evaluant.Uss.SqlMapper.Mapping.SqlMapperMappingDomainModel.SingletonResourceManager.GetString("UnrecognizedElementType"),
								elementType.Name);
				throw new global::System.ArgumentException(exceptionError, "elementType");
			}
			switch (index)
			{
				case 0: return new Mapping(partition, propertyAssignments);
				case 1: return new Entity(partition, propertyAssignments);
				case 2: return new Reference(partition, propertyAssignments);
				case 3: return new Attribute(partition, propertyAssignments);
				case 4: return new SqlMapperMappingDiagram(partition, propertyAssignments);
				case 5: return new Relationshipshape(partition, propertyAssignments);
				case 6: return new EntityShape(partition, propertyAssignments);
				default: return null;
			}
		}
	
		private static global::System.Collections.Generic.Dictionary<global::System.Type, int> createElementLinkMap;
	
		/// <summary>
		/// Creates an element link of specified type.
		/// </summary>
		/// <param name="partition">Partition where element is to be created.</param>
		/// <param name="elementLinkType">Element link type which belongs to this domain model.</param>
		/// <param name="roleAssignments">List of relationship role assignments for the new link.</param>
		/// <param name="propertyAssignments">New element property assignments.</param>
		/// <returns>Created element link.</returns>
		[global::System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public sealed override DslModeling::ElementLink CreateElementLink(DslModeling::Partition partition, global::System.Type elementLinkType, DslModeling::RoleAssignment[] roleAssignments, DslModeling::PropertyAssignment[] propertyAssignments)
		{
			if (elementLinkType == null) throw new global::System.ArgumentNullException("elementLinkType");
			if (roleAssignments == null) throw new global::System.ArgumentNullException("roleAssignments");
	
			if (createElementLinkMap == null)
			{
				createElementLinkMap = new global::System.Collections.Generic.Dictionary<global::System.Type, int>(5);
				createElementLinkMap.Add(typeof(EntityHasReferences), 0);
				createElementLinkMap.Add(typeof(EntityHasAttributes), 1);
				createElementLinkMap.Add(typeof(SqlMapperMappingDiagramReferencesEntityShapes), 2);
				createElementLinkMap.Add(typeof(EntityShapeReferencesRelationshipshape), 3);
				createElementLinkMap.Add(typeof(MappingHasEntities), 4);
			}
			int index;
			if (!createElementLinkMap.TryGetValue(elementLinkType, out index))
			{
				// construct exception error message
				string exceptionError = string.Format(
								global::System.Globalization.CultureInfo.CurrentCulture,
								global::Evaluant.Uss.SqlMapper.Mapping.SqlMapperMappingDomainModel.SingletonResourceManager.GetString("UnrecognizedElementLinkType"),
								elementLinkType.Name);
				throw new global::System.ArgumentException(exceptionError, "elementLinkType");
			
			}
			switch (index)
			{
				case 0: return new EntityHasReferences(partition, roleAssignments, propertyAssignments);
				case 1: return new EntityHasAttributes(partition, roleAssignments, propertyAssignments);
				case 2: return new SqlMapperMappingDiagramReferencesEntityShapes(partition, roleAssignments, propertyAssignments);
				case 3: return new EntityShapeReferencesRelationshipshape(partition, roleAssignments, propertyAssignments);
				case 4: return new MappingHasEntities(partition, roleAssignments, propertyAssignments);
				default: return null;
			}
		}
		#endregion
		#region Resource manager
		
		private static global::System.Resources.ResourceManager resourceManager;
		
		/// <summary>
		/// The base name of this model's resources.
		/// </summary>
		public const string ResourceBaseName = "Evaluant.Uss.SqlMapper.Mapping.GeneratedCode.DomainModelResx";
		
		/// <summary>
		/// Gets the DomainModel's ResourceManager. If the ResourceManager does not already exist, then it is created.
		/// </summary>
		public override global::System.Resources.ResourceManager ResourceManager
		{
			[global::System.Diagnostics.DebuggerStepThrough]
			get
			{
				return SqlMapperMappingDomainModel.SingletonResourceManager;
			}
		}
	
		/// <summary>
		/// Gets the Singleton ResourceManager for this domain model.
		/// </summary>
		public static global::System.Resources.ResourceManager SingletonResourceManager
		{
			[global::System.Diagnostics.DebuggerStepThrough]
			get
			{
				if (SqlMapperMappingDomainModel.resourceManager == null)
				{
					SqlMapperMappingDomainModel.resourceManager = new global::System.Resources.ResourceManager(ResourceBaseName, typeof(SqlMapperMappingDomainModel).Assembly);
				}
				return SqlMapperMappingDomainModel.resourceManager;
			}
		}
		#endregion
		#region Copy/Remove closures
		/// <summary>
		/// CopyClosure cache
		/// </summary>
		private static DslModeling::IElementVisitorFilter copyClosure;
		/// <summary>
		/// DeleteClosure cache
		/// </summary>
		private static DslModeling::IElementVisitorFilter removeClosure;
		/// <summary>
		/// Returns an IElementVisitorFilter that corresponds to the ClosureType.
		/// </summary>
		/// <param name="type">closure type</param>
		/// <param name="rootElements">collection of root elements</param>
		/// <returns>IElementVisitorFilter or null</returns>
		public override DslModeling::IElementVisitorFilter GetClosureFilter(DslModeling::ClosureType type, global::System.Collections.Generic.ICollection<DslModeling::ModelElement> rootElements)
		{
			switch (type)
			{
				case DslModeling::ClosureType.CopyClosure:
					return SqlMapperMappingDomainModel.CopyClosure;
				case DslModeling::ClosureType.DeleteClosure:
					return SqlMapperMappingDomainModel.DeleteClosure;
			}
			return base.GetClosureFilter(type, rootElements);
		}
		/// <summary>
		/// CopyClosure cache
		/// </summary>
		private static DslModeling::IElementVisitorFilter CopyClosure
		{
			get
			{
				// Incorporate all of the closures from the models we extend
				if (SqlMapperMappingDomainModel.copyClosure == null)
				{
					DslModeling::ChainingElementVisitorFilter copyFilter = new DslModeling::ChainingElementVisitorFilter();
					copyFilter.AddFilter(new SqlMapperMappingCopyClosure());
					copyFilter.AddFilter(new DslModeling::CoreCopyClosure());
					copyFilter.AddFilter(new DslDiagrams::CoreDesignSurfaceCopyClosure());
					
					SqlMapperMappingDomainModel.copyClosure = copyFilter;
				}
				return SqlMapperMappingDomainModel.copyClosure;
			}
		}
		/// <summary>
		/// DeleteClosure cache
		/// </summary>
		private static DslModeling::IElementVisitorFilter DeleteClosure
		{
			get
			{
				// Incorporate all of the closures from the models we extend
				if (SqlMapperMappingDomainModel.removeClosure == null)
				{
					DslModeling::ChainingElementVisitorFilter removeFilter = new DslModeling::ChainingElementVisitorFilter();
					removeFilter.AddFilter(new SqlMapperMappingDeleteClosure());
					removeFilter.AddFilter(new DslModeling::CoreDeleteClosure());
					removeFilter.AddFilter(new DslDiagrams::CoreDesignSurfaceDeleteClosure());
		
					SqlMapperMappingDomainModel.removeClosure = removeFilter;
				}
				return SqlMapperMappingDomainModel.removeClosure;
			}
		}
		#endregion
		#region Diagram rule helpers
		/// <summary>
		/// Enables rules in this domain model related to diagram fixup for the given store.
		/// If diagram data will be loaded into the store, this method should be called first to ensure
		/// that the diagram behaves properly.
		/// </summary>
		public static void EnableDiagramRules(DslModeling::Store store)
		{
			if(store == null) throw new global::System.ArgumentNullException("store");
			
			DslModeling::RuleManager ruleManager = store.RuleManager;
			ruleManager.EnableRule(typeof(global::Evaluant.Uss.SqlMapper.Mapping.FixUpDiagram));
		}
		
		/// <summary>
		/// Disables rules in this domain model related to diagram fixup for the given store.
		/// </summary>
		public static void DisableDiagramRules(DslModeling::Store store)
		{
			if(store == null) throw new global::System.ArgumentNullException("store");
			
			DslModeling::RuleManager ruleManager = store.RuleManager;
			ruleManager.DisableRule(typeof(global::Evaluant.Uss.SqlMapper.Mapping.FixUpDiagram));
		}
		#endregion
	}
		
	#region Copy/Remove closure classes
	/// <summary>
	/// Remove closure visitor filter
	/// </summary>
	[global::System.CLSCompliant(true)]
	public partial class SqlMapperMappingDeleteClosure : SqlMapperMappingDeleteClosureBase, DslModeling::IElementVisitorFilter
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public SqlMapperMappingDeleteClosure() : base()
		{
		}
	}
	
	/// <summary>
	/// Base class for remove closure visitor filter
	/// </summary>
	[global::System.CLSCompliant(true)]
	public partial class SqlMapperMappingDeleteClosureBase : DslModeling::IElementVisitorFilter
	{
		/// <summary>
		/// DomainRoles
		/// </summary>
		private global::System.Collections.Specialized.HybridDictionary domainRoles;
		/// <summary>
		/// Constructor
		/// </summary>
		public SqlMapperMappingDeleteClosureBase()
		{
			#region Initialize DomainData Table
			DomainRoles.Add(global::Evaluant.Uss.SqlMapper.Mapping.EntityHasReferences.ReferenceDomainRoleId, true);
			DomainRoles.Add(global::Evaluant.Uss.SqlMapper.Mapping.EntityHasAttributes.AttributeDomainRoleId, true);
			DomainRoles.Add(global::Evaluant.Uss.SqlMapper.Mapping.MappingHasEntities.EntityDomainRoleId, true);
			#endregion
		}
		/// <summary>
		/// Called to ask the filter if a particular relationship from a source element should be included in the traversal
		/// </summary>
		/// <param name="walker">ElementWalker that is traversing the model</param>
		/// <param name="sourceElement">Model Element playing the source role</param>
		/// <param name="sourceRoleInfo">DomainRoleInfo of the role that the source element is playing in the relationship</param>
		/// <param name="domainRelationshipInfo">DomainRelationshipInfo for the ElementLink in question</param>
		/// <param name="targetRelationship">Relationship in question</param>
		/// <returns>Yes if the relationship should be traversed</returns>
		public virtual DslModeling::VisitorFilterResult ShouldVisitRelationship(DslModeling::ElementWalker walker, DslModeling::ModelElement sourceElement, DslModeling::DomainRoleInfo sourceRoleInfo, DslModeling::DomainRelationshipInfo domainRelationshipInfo, DslModeling::ElementLink targetRelationship)
		{
			return DslModeling::VisitorFilterResult.Yes;
		}
		/// <summary>
		/// Called to ask the filter if a particular role player should be Visited during traversal
		/// </summary>
		/// <param name="walker">ElementWalker that is traversing the model</param>
		/// <param name="sourceElement">Model Element playing the source role</param>
		/// <param name="elementLink">Element Link that forms the relationship to the role player in question</param>
		/// <param name="targetDomainRole">DomainRoleInfo of the target role</param>
		/// <param name="targetRolePlayer">Model Element that plays the target role in the relationship</param>
		/// <returns></returns>
		public virtual DslModeling::VisitorFilterResult ShouldVisitRolePlayer(DslModeling::ElementWalker walker, DslModeling::ModelElement sourceElement, DslModeling::ElementLink elementLink, DslModeling::DomainRoleInfo targetDomainRole, DslModeling::ModelElement targetRolePlayer)
		{
			if (targetDomainRole == null) throw new global::System.ArgumentNullException("targetDomainRole");
			return this.DomainRoles.Contains(targetDomainRole.Id) ? DslModeling::VisitorFilterResult.Yes : DslModeling::VisitorFilterResult.DoNotCare;
		}
		/// <summary>
		/// DomainRoles
		/// </summary>
		private global::System.Collections.Specialized.HybridDictionary DomainRoles
		{
			get
			{
				if (this.domainRoles == null)
				{
					this.domainRoles = new global::System.Collections.Specialized.HybridDictionary();
				}
				return this.domainRoles;
			}
		}
	
	}
	/// <summary>
	/// Copy closure visitor filter
	/// </summary>
	[global::System.CLSCompliant(true)]
	public partial class SqlMapperMappingCopyClosure : SqlMapperMappingCopyClosureBase, DslModeling::IElementVisitorFilter
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public SqlMapperMappingCopyClosure() : base()
		{
		}
	}
	/// <summary>
	/// Base class for copy closure visitor filter
	/// </summary>
	[global::System.CLSCompliant(true)]
	public partial class SqlMapperMappingCopyClosureBase : DslModeling::CopyClosureFilter, DslModeling::IElementVisitorFilter
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public SqlMapperMappingCopyClosureBase():base()
		{
		}
	}
	#endregion
		
}
