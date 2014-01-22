<?xml version="1.0" encoding="utf-8"?>
<Dsl xmlns:dm0="http://schemas.microsoft.com/VisualStudio/2008/DslTools/Core" dslVersion="1.0.0.0" Id="3a8d209f-f0eb-4205-8a82-b01f252eff10" Description="Description for Evaluant.Uss.SqlMapper.Mapping.SqlMapperMapping" Name="SqlMapperMapping" DisplayName="SqlMapperMapping" Namespace="Evaluant.Uss.SqlMapper.Mapping" ProductName="Evaluant.Uss" CompanyName="Evaluant" PackageGuid="dc2ca4a9-94ff-457f-ab1b-44975f1e1e1d" PackageNamespace="Evaluant.Uss.SqlMapper.Mapping" xmlns="http://schemas.microsoft.com/VisualStudio/2005/DslTools/DslDefinitionModel">
  <Classes>
    <DomainClass Id="b781fed7-e0c2-4f43-8877-d0f6b0c1d02f" Description="The root in which all other elements are embedded. Appears as a diagram." Name="Mapping" DisplayName="Mapping" Namespace="Evaluant.Uss.SqlMapper.Mapping">
      <ElementMergeDirectives>
        <ElementMergeDirective>
          <Index>
            <DomainClassMoniker Name="Entity" />
          </Index>
          <LinkCreationPaths>
            <DomainPath>MappingHasEntities.Entities</DomainPath>
          </LinkCreationPaths>
        </ElementMergeDirective>
      </ElementMergeDirectives>
    </DomainClass>
    <DomainClass Id="26f9fe6b-d5c8-4e3b-a4cc-91e64c3f81e2" Description="Description for Evaluant.Uss.SqlMapper.Mapping.Entity" Name="Entity" DisplayName="Entity" Namespace="Evaluant.Uss.SqlMapper.Mapping">
      <ElementMergeDirectives>
        <ElementMergeDirective>
          <Index>
            <DomainClassMoniker Name="Reference" />
          </Index>
          <LinkCreationPaths>
            <DomainPath>EntityHasReferences.References</DomainPath>
          </LinkCreationPaths>
        </ElementMergeDirective>
        <ElementMergeDirective>
          <Index>
            <DomainClassMoniker Name="Attribute" />
          </Index>
          <LinkCreationPaths>
            <DomainPath>EntityHasAttributes.Attributes</DomainPath>
          </LinkCreationPaths>
        </ElementMergeDirective>
      </ElementMergeDirectives>
    </DomainClass>
    <DomainClass Id="82e5253f-a09f-4ebd-b1a0-932581beb50b" Description="Description for Evaluant.Uss.SqlMapper.Mapping.Reference" Name="Reference" DisplayName="Reference" Namespace="Evaluant.Uss.SqlMapper.Mapping">
      <Properties>
        <DomainProperty Id="ed0e7ca9-1c3a-4e52-be16-366d07709901" Description="Description for Evaluant.Uss.SqlMapper.Mapping.Reference.Name" Name="Name" DisplayName="Name" IsElementName="true">
          <Type>
            <ExternalTypeMoniker Name="/System/String" />
          </Type>
        </DomainProperty>
      </Properties>
    </DomainClass>
    <DomainClass Id="167ef8d9-ab65-4133-ba0a-abeb1c22f56f" Description="Description for Evaluant.Uss.SqlMapper.Mapping.Attribute" Name="Attribute" DisplayName="Attribute" Namespace="Evaluant.Uss.SqlMapper.Mapping">
      <Properties>
        <DomainProperty Id="3fe95160-b033-4d6e-80b7-5de2d0bd288b" Description="Description for Evaluant.Uss.SqlMapper.Mapping.Attribute.Name" Name="Name" DisplayName="Name" IsElementName="true">
          <Type>
            <ExternalTypeMoniker Name="/System/String" />
          </Type>
        </DomainProperty>
      </Properties>
    </DomainClass>
  </Classes>
  <Relationships>
    <DomainRelationship Id="88f57ad0-da31-42f1-97e7-ebe6b1a38108" Description="Description for Evaluant.Uss.SqlMapper.Mapping.EntityHasReferences" Name="EntityHasReferences" DisplayName="Entity Has References" Namespace="Evaluant.Uss.SqlMapper.Mapping" IsEmbedding="true">
      <Source>
        <DomainRole Id="a9f00092-cb69-4c95-b85c-faf150f8037d" Description="Description for Evaluant.Uss.SqlMapper.Mapping.EntityHasReferences.Entity" Name="Entity" DisplayName="Entity" PropertyName="References" PropagatesCopy="PropagatesCopyToLinkAndOppositeRolePlayer" PropertyDisplayName="References">
          <RolePlayer>
            <DomainClassMoniker Name="Entity" />
          </RolePlayer>
        </DomainRole>
      </Source>
      <Target>
        <DomainRole Id="b530ced2-4d52-41bd-8427-75781c7a4f1b" Description="Description for Evaluant.Uss.SqlMapper.Mapping.EntityHasReferences.Reference" Name="Reference" DisplayName="Reference" PropertyName="Entity" Multiplicity="One" PropagatesDelete="true" PropertyDisplayName="Entity">
          <RolePlayer>
            <DomainClassMoniker Name="Reference" />
          </RolePlayer>
        </DomainRole>
      </Target>
    </DomainRelationship>
    <DomainRelationship Id="6b580c85-518a-40ab-9d62-fdfb1b23a12b" Description="Description for Evaluant.Uss.SqlMapper.Mapping.EntityHasAttributes" Name="EntityHasAttributes" DisplayName="Entity Has Attributes" Namespace="Evaluant.Uss.SqlMapper.Mapping" IsEmbedding="true">
      <Source>
        <DomainRole Id="33b7dc22-7eee-4ac7-88d2-9b037caa6189" Description="Description for Evaluant.Uss.SqlMapper.Mapping.EntityHasAttributes.Entity" Name="Entity" DisplayName="Entity" PropertyName="Attributes" PropagatesCopy="PropagatesCopyToLinkAndOppositeRolePlayer" PropertyDisplayName="Attributes">
          <RolePlayer>
            <DomainClassMoniker Name="Entity" />
          </RolePlayer>
        </DomainRole>
      </Source>
      <Target>
        <DomainRole Id="ac8b78a4-955d-4b1a-9b54-1a59c4474ec3" Description="Description for Evaluant.Uss.SqlMapper.Mapping.EntityHasAttributes.Attribute" Name="Attribute" DisplayName="Attribute" PropertyName="Entity" Multiplicity="One" PropagatesDelete="true" PropertyDisplayName="Entity">
          <RolePlayer>
            <DomainClassMoniker Name="Attribute" />
          </RolePlayer>
        </DomainRole>
      </Target>
    </DomainRelationship>
    <DomainRelationship Id="e730d212-6a3d-4b4d-b71f-a7405d015e88" Description="Description for Evaluant.Uss.SqlMapper.Mapping.SqlMapperMappingDiagramReferencesEntityShapes" Name="SqlMapperMappingDiagramReferencesEntityShapes" DisplayName="Sql Mapper Mapping Diagram References Entity Shapes" Namespace="Evaluant.Uss.SqlMapper.Mapping">
      <Source>
        <DomainRole Id="eee17f73-8ffa-469f-a8c3-583619cadcda" Description="Description for Evaluant.Uss.SqlMapper.Mapping.SqlMapperMappingDiagramReferencesEntityShapes.SqlMapperMappingDiagram" Name="SqlMapperMappingDiagram" DisplayName="Sql Mapper Mapping Diagram" PropertyName="EntityShapes" PropertyDisplayName="Entity Shapes">
          <RolePlayer>
            <DiagramMoniker Name="SqlMapperMappingDiagram" />
          </RolePlayer>
        </DomainRole>
      </Source>
      <Target>
        <DomainRole Id="4f0b9d5c-2f78-4880-98ef-4178942d5473" Description="Description for Evaluant.Uss.SqlMapper.Mapping.SqlMapperMappingDiagramReferencesEntityShapes.EntityShape" Name="EntityShape" DisplayName="Entity Shape" PropertyName="SqlMapperMappingDiagrams" PropertyDisplayName="Sql Mapper Mapping Diagrams">
          <RolePlayer>
            <GeometryShapeMoniker Name="EntityShape" />
          </RolePlayer>
        </DomainRole>
      </Target>
    </DomainRelationship>
    <DomainRelationship Id="fb4ce25f-a11c-4257-81ee-004b1274be42" Description="Description for Evaluant.Uss.SqlMapper.Mapping.EntityShapeReferencesRelationshipshape" Name="EntityShapeReferencesRelationshipshape" DisplayName="Entity Shape References Relationshipshape" Namespace="Evaluant.Uss.SqlMapper.Mapping">
      <Source>
        <DomainRole Id="fa7a81ab-75a5-4f8a-b1b2-735501a8df5c" Description="Description for Evaluant.Uss.SqlMapper.Mapping.EntityShapeReferencesRelationshipshape.EntityShape" Name="EntityShape" DisplayName="Entity Shape" PropertyName="Relationshipshape" PropertyDisplayName="Relationshipshape">
          <RolePlayer>
            <GeometryShapeMoniker Name="EntityShape" />
          </RolePlayer>
        </DomainRole>
      </Source>
      <Target>
        <DomainRole Id="d51e6899-24f2-4604-b846-dd5f1b461c64" Description="Description for Evaluant.Uss.SqlMapper.Mapping.EntityShapeReferencesRelationshipshape.Relationshipshape" Name="Relationshipshape" DisplayName="Relationshipshape" PropertyName="EntityShapes" PropertyDisplayName="Entity Shapes">
          <RolePlayer>
            <ConnectorMoniker Name="Relationshipshape" />
          </RolePlayer>
        </DomainRole>
      </Target>
    </DomainRelationship>
    <DomainRelationship Id="86b38a3b-0e4e-4853-9195-c8702ad8fa33" Description="Description for Evaluant.Uss.SqlMapper.Mapping.MappingHasEntities" Name="MappingHasEntities" DisplayName="Mapping Has Entities" Namespace="Evaluant.Uss.SqlMapper.Mapping" IsEmbedding="true">
      <Source>
        <DomainRole Id="29a1f4b2-48be-4d6d-b4fd-c701a3b55723" Description="Description for Evaluant.Uss.SqlMapper.Mapping.MappingHasEntities.Mapping" Name="Mapping" DisplayName="Mapping" PropertyName="Entities" PropagatesCopy="PropagatesCopyToLinkAndOppositeRolePlayer" PropertyDisplayName="Entities">
          <RolePlayer>
            <DomainClassMoniker Name="Mapping" />
          </RolePlayer>
        </DomainRole>
      </Source>
      <Target>
        <DomainRole Id="11e725e9-fe80-4476-a90b-784b8133a18f" Description="Description for Evaluant.Uss.SqlMapper.Mapping.MappingHasEntities.Entity" Name="Entity" DisplayName="Entity" PropertyName="Mapping" Multiplicity="One" PropagatesDelete="true" PropertyDisplayName="Mapping">
          <RolePlayer>
            <DomainClassMoniker Name="Entity" />
          </RolePlayer>
        </DomainRole>
      </Target>
    </DomainRelationship>
  </Relationships>
  <Types>
    <ExternalType Name="DateTime" Namespace="System" />
    <ExternalType Name="String" Namespace="System" />
    <ExternalType Name="Int16" Namespace="System" />
    <ExternalType Name="Int32" Namespace="System" />
    <ExternalType Name="Int64" Namespace="System" />
    <ExternalType Name="UInt16" Namespace="System" />
    <ExternalType Name="UInt32" Namespace="System" />
    <ExternalType Name="UInt64" Namespace="System" />
    <ExternalType Name="SByte" Namespace="System" />
    <ExternalType Name="Byte" Namespace="System" />
    <ExternalType Name="Double" Namespace="System" />
    <ExternalType Name="Single" Namespace="System" />
    <ExternalType Name="Guid" Namespace="System" />
    <ExternalType Name="Boolean" Namespace="System" />
    <ExternalType Name="Char" Namespace="System" />
  </Types>
  <Shapes>
    <GeometryShape Id="110c58af-9c6f-4355-8fdc-14c2abb37733" Description="Shape used to represent ExampleElements on a Diagram." Name="EntityShape" DisplayName="Entity Shape" Namespace="Evaluant.Uss.SqlMapper.Mapping" FixedTooltipText="Entity Shape" FillColor="242, 239, 229" OutlineColor="113, 111, 110" InitialWidth="2" InitialHeight="0.75" OutlineThickness="0.01" Geometry="Rectangle">
      <Notes>The shape has a text decorator used to display the Name property of the mapped ExampleElement.</Notes>
      <ShapeHasDecorators Position="Center" HorizontalOffset="0" VerticalOffset="0">
        <TextDecorator Name="NameDecorator" DisplayName="Name Decorator" DefaultText="NameDecorator" />
      </ShapeHasDecorators>
      <ShapeHasDecorators Position="InnerTopLeft" HorizontalOffset="0" VerticalOffset="0">
        <ExpandCollapseDecorator Name="ExpandCollapseDecorator" DisplayName="Expand Collapse Decorator" />
      </ShapeHasDecorators>
    </GeometryShape>
  </Shapes>
  <Connectors>
    <Connector Id="bccdc380-8e1b-45a6-9042-402b71ed3aaa" Description="Connector between the ExampleShapes. Represents ExampleRelationships on the Diagram." Name="Relationshipshape" DisplayName="Relationshipshape" Namespace="Evaluant.Uss.SqlMapper.Mapping" FixedTooltipText="Relationshipshape" Color="113, 111, 110" TargetEndStyle="EmptyArrow" Thickness="0.01">
      <ConnectorHasDecorators Position="TargetTop" OffsetFromShape="0" OffsetFromLine="0" isMoveable="true">
        <TextDecorator Name="Name" DisplayName="Name" DefaultText="Name" />
      </ConnectorHasDecorators>
      <ConnectorHasDecorators Position="SourceBottom" OffsetFromShape="0" OffsetFromLine="0" isMoveable="true">
        <TextDecorator Name="SourceCardinality" DisplayName="Source Cardinality" DefaultText="SourceCardinality" />
      </ConnectorHasDecorators>
      <ConnectorHasDecorators Position="TargetBottom" OffsetFromShape="0" OffsetFromLine="0" isMoveable="true">
        <TextDecorator Name="TargetCardinality" DisplayName="Target Cardinality" DefaultText="TargetCardinality" />
      </ConnectorHasDecorators>
    </Connector>
  </Connectors>
  <XmlSerializationBehavior Name="SqlMapperMappingSerializationBehavior" Namespace="Evaluant.Uss.SqlMapper.Mapping">
    <ClassData>
      <XmlClassData TypeName="Mapping" MonikerAttributeName="" SerializeId="true" MonikerElementName="mappingMoniker" ElementName="mapping" MonikerTypeName="MappingMoniker">
        <DomainClassMoniker Name="Mapping" />
        <ElementData>
          <XmlRelationshipData UseFullForm="true" RoleElementName="entities">
            <DomainRelationshipMoniker Name="MappingHasEntities" />
          </XmlRelationshipData>
        </ElementData>
      </XmlClassData>
      <XmlClassData TypeName="EntityShape" MonikerAttributeName="" SerializeId="true" MonikerElementName="entityShapeMoniker" ElementName="entityShape" MonikerTypeName="EntityShapeMoniker">
        <GeometryShapeMoniker Name="EntityShape" />
        <ElementData>
          <XmlRelationshipData UseFullForm="true" RoleElementName="relationshipshape">
            <DomainRelationshipMoniker Name="EntityShapeReferencesRelationshipshape" />
          </XmlRelationshipData>
        </ElementData>
      </XmlClassData>
      <XmlClassData TypeName="Relationshipshape" MonikerAttributeName="" SerializeId="true" MonikerElementName="relationshipshapeMoniker" ElementName="relationshipshape" MonikerTypeName="RelationshipshapeMoniker">
        <ConnectorMoniker Name="Relationshipshape" />
      </XmlClassData>
      <XmlClassData TypeName="SqlMapperMappingDiagram" MonikerAttributeName="" SerializeId="true" MonikerElementName="sqlMapperMappingDiagramMoniker" ElementName="sqlMapperMappingDiagram" MonikerTypeName="SqlMapperMappingDiagramMoniker">
        <DiagramMoniker Name="SqlMapperMappingDiagram" />
        <ElementData>
          <XmlRelationshipData UseFullForm="true" RoleElementName="entityShapes">
            <DomainRelationshipMoniker Name="SqlMapperMappingDiagramReferencesEntityShapes" />
          </XmlRelationshipData>
        </ElementData>
      </XmlClassData>
      <XmlClassData TypeName="Entity" MonikerAttributeName="" SerializeId="true" MonikerElementName="entityMoniker" ElementName="entity" MonikerTypeName="EntityMoniker">
        <DomainClassMoniker Name="Entity" />
        <ElementData>
          <XmlRelationshipData UseFullForm="true" RoleElementName="references">
            <DomainRelationshipMoniker Name="EntityHasReferences" />
          </XmlRelationshipData>
          <XmlRelationshipData UseFullForm="true" RoleElementName="attributes">
            <DomainRelationshipMoniker Name="EntityHasAttributes" />
          </XmlRelationshipData>
        </ElementData>
      </XmlClassData>
      <XmlClassData TypeName="Reference" MonikerAttributeName="name" SerializeId="true" MonikerElementName="referenceMoniker" ElementName="reference" MonikerTypeName="ReferenceMoniker">
        <DomainClassMoniker Name="Reference" />
        <ElementData>
          <XmlPropertyData XmlName="name" IsMonikerKey="true">
            <DomainPropertyMoniker Name="Reference/Name" />
          </XmlPropertyData>
        </ElementData>
      </XmlClassData>
      <XmlClassData TypeName="EntityHasReferences" MonikerAttributeName="" SerializeId="true" MonikerElementName="entityHasReferencesMoniker" ElementName="entityHasReferences" MonikerTypeName="EntityHasReferencesMoniker">
        <DomainRelationshipMoniker Name="EntityHasReferences" />
      </XmlClassData>
      <XmlClassData TypeName="Attribute" MonikerAttributeName="name" SerializeId="true" MonikerElementName="attributeMoniker" ElementName="attribute" MonikerTypeName="AttributeMoniker">
        <DomainClassMoniker Name="Attribute" />
        <ElementData>
          <XmlPropertyData XmlName="name" IsMonikerKey="true">
            <DomainPropertyMoniker Name="Attribute/Name" />
          </XmlPropertyData>
        </ElementData>
      </XmlClassData>
      <XmlClassData TypeName="EntityHasAttributes" MonikerAttributeName="" SerializeId="true" MonikerElementName="entityHasAttributesMoniker" ElementName="entityHasAttributes" MonikerTypeName="EntityHasAttributesMoniker">
        <DomainRelationshipMoniker Name="EntityHasAttributes" />
      </XmlClassData>
      <XmlClassData TypeName="SqlMapperMappingDiagramReferencesEntityShapes" MonikerAttributeName="" SerializeId="true" MonikerElementName="sqlMapperMappingDiagramReferencesEntityShapesMoniker" ElementName="sqlMapperMappingDiagramReferencesEntityShapes" MonikerTypeName="SqlMapperMappingDiagramReferencesEntityShapesMoniker">
        <DomainRelationshipMoniker Name="SqlMapperMappingDiagramReferencesEntityShapes" />
      </XmlClassData>
      <XmlClassData TypeName="EntityShapeReferencesRelationshipshape" MonikerAttributeName="" SerializeId="true" MonikerElementName="entityShapeReferencesRelationshipshapeMoniker" ElementName="entityShapeReferencesRelationshipshape" MonikerTypeName="EntityShapeReferencesRelationshipshapeMoniker">
        <DomainRelationshipMoniker Name="EntityShapeReferencesRelationshipshape" />
      </XmlClassData>
      <XmlClassData TypeName="MappingHasEntities" MonikerAttributeName="" SerializeId="true" MonikerElementName="mappingHasEntitiesMoniker" ElementName="mappingHasEntities" MonikerTypeName="MappingHasEntitiesMoniker">
        <DomainRelationshipMoniker Name="MappingHasEntities" />
      </XmlClassData>
    </ClassData>
  </XmlSerializationBehavior>
  <ExplorerBehavior Name="SqlMapperMappingExplorer" />
  <ConnectionBuilders>
    <ConnectionBuilder Name="SqlMapperMappingDiagramReferencesEntityShapesBuilder">
      <LinkConnectDirective>
        <DomainRelationshipMoniker Name="SqlMapperMappingDiagramReferencesEntityShapes" />
        <SourceDirectives>
          <RolePlayerConnectDirective>
            <AcceptingClass>
              <DiagramMoniker Name="SqlMapperMappingDiagram" />
            </AcceptingClass>
          </RolePlayerConnectDirective>
        </SourceDirectives>
        <TargetDirectives>
          <RolePlayerConnectDirective>
            <AcceptingClass>
              <GeometryShapeMoniker Name="EntityShape" />
            </AcceptingClass>
          </RolePlayerConnectDirective>
        </TargetDirectives>
      </LinkConnectDirective>
    </ConnectionBuilder>
    <ConnectionBuilder Name="EntityShapeReferencesRelationshipshapeBuilder">
      <LinkConnectDirective>
        <DomainRelationshipMoniker Name="EntityShapeReferencesRelationshipshape" />
        <SourceDirectives>
          <RolePlayerConnectDirective>
            <AcceptingClass>
              <GeometryShapeMoniker Name="EntityShape" />
            </AcceptingClass>
          </RolePlayerConnectDirective>
        </SourceDirectives>
        <TargetDirectives>
          <RolePlayerConnectDirective>
            <AcceptingClass>
              <ConnectorMoniker Name="Relationshipshape" />
            </AcceptingClass>
          </RolePlayerConnectDirective>
        </TargetDirectives>
      </LinkConnectDirective>
    </ConnectionBuilder>
  </ConnectionBuilders>
  <Diagram Id="8f33c714-9930-407d-8ac9-6e2fedcb7457" Description="Description for Evaluant.Uss.SqlMapper.Mapping.SqlMapperMappingDiagram" Name="SqlMapperMappingDiagram" DisplayName="Minimal Language Diagram" Namespace="Evaluant.Uss.SqlMapper.Mapping">
    <Class>
      <DomainClassMoniker Name="Mapping" />
    </Class>
    <ShapeMaps>
      <ShapeMap HasCustomParentElement="true">
        <DomainClassMoniker Name="Entity" />
        <GeometryShapeMoniker Name="EntityShape" />
      </ShapeMap>
    </ShapeMaps>
  </Diagram>
  <Designer CopyPasteGeneration="CopyPasteOnly" FileExtension="sqlm" EditorGuid="656a4d1c-4e0d-4a6c-b278-1335cef46f5e">
    <RootClass>
      <DomainClassMoniker Name="Mapping" />
    </RootClass>
    <XmlSerializationDefinition CustomPostLoad="false">
      <XmlSerializationBehaviorMoniker Name="SqlMapperMappingSerializationBehavior" />
    </XmlSerializationDefinition>
    <ToolboxTab TabText="SqlMapperMapping">
      <ElementTool Name="ExampleElement" ToolboxIcon="resources\exampleshapetoolbitmap.bmp" Caption="ExampleElement" Tooltip="Create an ExampleElement" HelpKeyword="CreateExampleClassF1Keyword">
        <DomainClassMoniker Name="Entity" />
      </ElementTool>
      <ConnectionTool Name="ExampleRelationship" ToolboxIcon="resources\exampleconnectortoolbitmap.bmp" Caption="ExampleRelationship" Tooltip="Drag between ExampleElements to create an ExampleRelationship" HelpKeyword="ConnectExampleRelationF1Keyword">
        <ConnectionBuilderMoniker Name="SqlMapperMapping/EntityShapeReferencesRelationshipshapeBuilder" />
      </ConnectionTool>
    </ToolboxTab>
    <Validation UsesMenu="false" UsesOpen="false" UsesSave="false" UsesLoad="false" />
    <DiagramMoniker Name="SqlMapperMappingDiagram" />
  </Designer>
  <Explorer ExplorerGuid="40bd32a7-3ce9-4e55-9dc8-23a2add2cefc" Title="SqlMapperMapping Explorer">
    <ExplorerBehaviorMoniker Name="SqlMapperMapping/SqlMapperMappingExplorer" />
  </Explorer>
</Dsl>