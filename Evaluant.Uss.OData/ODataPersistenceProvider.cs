using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.PersistenceEngine.Contracts;
using System.Net;
using System.Xml;
using System.Net.Browser;
using System.Security;
using System.Diagnostics;

namespace Evaluant.Uss.OData
{
    public class ODataPersistenceProvider : PersistenceProviderAsyncImplementation
    {
        static readonly Version requestVersion = new Version(2, 0);

        public override void InitializeConfiguration()
        {
            initialized = true;
            HttpWebRequest request = (HttpWebRequest)WebRequestCreator.ClientHttp.Create(new Uri(ConnectionString, new Uri("$metadata", UriKind.Relative)));
            request.Method = "GET";
            //request.Headers["X-HTTP-Method"] = "GET";
            //request.Headers["DataServiceVersion"] = requestVersion.ToString() + ";NetFx";
            //request.Headers["MaxDataServiceVersion"] = requestVersion.ToString() + ";NetFx";
            request.Accept = "application/atom+xml,application/xml";
            request.BeginGetResponse(res =>
            {
                try
                {
                    HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(res);
                    ReadMetadata(response.GetResponseStream());
                }
                catch (WebException)
                {

                }
                catch (SecurityException)
                {
                    if (!Debugger.IsAttached)
                        throw new SecurityException("You are not allowed to access this OData service. If you are the owner of this service, please ensure there is a client access policy file on the root of this server.");
                }
                catch (UnauthorizedAccessException)
                {

                }
                if (Initialized != null)
                    Initialized();
            }, null);
        }

        public event System.Action Initialized;

        private void GotResponse(IAsyncResult result)
        {
            try
            {
                HttpWebResponse response = (HttpWebResponse)((WebRequest)result.AsyncState).EndGetResponse(result);
                ReadMetadata(response.GetResponseStream());
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }

        private void ReadMetadata(System.IO.Stream stream)
        {
            XmlReader reader = XmlReader.Create(stream);
            Edm.Metadata metadata = new Edm.Metadata();
            if (GoToElement(reader, "Edmx"))
            {
                if (GoToElement(reader, "DataServices"))
                {
                    if (GoToElement(reader, "Schema"))
                    {
                        string @namespace = reader.GetAttribute("Namespace") + ".";
                        GoToElement(reader, null);
                        do
                        {
                            if (reader.LocalName == "EntityType")
                            {
                                string type = @namespace + reader.GetAttribute("Name");
                                Edm.EntityType entity = new Edm.EntityType() { Type = type, BaseType = reader.GetAttribute("BaseType") };
                                metadata.EntityTypes.Add(entity.Type, entity);
                                GoToElement(reader, null);
                                do
                                {
                                    if (reader.LocalName == "Key")
                                    {
                                        GoToElement(reader, "PropertyRef");
                                        string propertyName = reader.GetAttribute("Name");

                                        Edm.Property property;
                                        if (!entity.Properties.TryGetValue(propertyName, out property))
                                        {
                                            property = new Edm.Property();
                                            entity.Properties.Add(propertyName, property);
                                        }
                                        property.IsId = true;
                                        property.Name = propertyName;
                                        GoAfterElement(reader, "Key");
                                    }
                                    else if (reader.LocalName == "Property")
                                    {
                                        string propertyName = reader.GetAttribute("Name");

                                        Edm.Property property;
                                        if (!entity.Properties.TryGetValue(propertyName, out property))
                                        {
                                            property = new Edm.Property();
                                            property.Name = propertyName;
                                            entity.Properties.Add(propertyName, property);
                                        }
                                        string propertyTypeName = reader.GetAttribute("Type");
                                        property.EdmType = propertyTypeName;
                                        switch (propertyTypeName)
                                        {
                                            case "Edm.Binary":
                                                property.Type = "byte[]";
                                                break;
                                            case "Edm.Boolean":
                                                property.Type = "bool";
                                                break;
                                            case "Edm.Byte":
                                                property.Type = "byte";
                                                break;
                                            case "Edm.DateTime":
                                                property.Type = "DateTime";
                                                break;
                                            case "Edm.Decimal":
                                                property.Type = "Decimal";
                                                break;
                                            case "Edm.Double":
                                                property.Type = "Double";
                                                break;
                                            case "Edm.Guid":
                                                property.Type = "Guid";
                                                break;
                                            case "Edm.Int16":
                                                property.Type = "short";
                                                break;
                                            case "Edm.Int32":
                                                property.Type = "int";
                                                break;
                                            case "Edm.Int64":
                                                property.Type = "long";
                                                break;
                                            case "Edm.SByte":
                                                property.Type = "sbyte";
                                                break;
                                            case "Edm.Single":
                                                property.Type = "single";
                                                break;
                                            case "Edm.Time":
                                                property.Type = "TimeSpan";
                                                break;
                                            case "Edm.DateTimeOffset":
                                                property.Type = "TimeSpan";
                                                break;
                                            case "Edm.String":
                                            default:
                                                property.Type = "string";
                                                break;

                                        }
                                        GoToElement(reader, null);
                                        //GoAfterElement(reader, "Property");
                                    }
                                    else if (reader.LocalName == "NavigationProperty")
                                    {
                                        string propertyName = reader.GetAttribute("Name");

                                        Edm.Property property;
                                        if (!entity.Properties.TryGetValue(propertyName, out property))
                                        {
                                            property = new Edm.Property();
                                            property.Name = propertyName;
                                            entity.Properties.Add(propertyName, property);
                                        }

                                        property.Type = reader.GetAttribute("Relationship");
                                        property.IsNavigationProperty = true;
                                        GoToElement(reader, null);
                                    }
                                }
                                while (reader.LocalName == "Key" || reader.LocalName == "Property" || reader.LocalName == "NavigationProperty");
                            }
                            else if (reader.LocalName == "ComplexType")
                            {
                                GoAfterElement(reader, "ComplexType");
                            }
                            else if (reader.LocalName == "Association")
                            {
                                string assocName = @namespace + reader.GetAttribute("Name");

                                Edm.Association assoc;
                                if (!metadata.Associations.TryGetValue(assocName, out assoc))
                                {
                                    assoc = new Edm.Association();
                                    assoc.Name = assocName;
                                    metadata.Associations.Add(assoc.Name, assoc);
                                }
                                GoToElement(reader, null);
                                if (reader.LocalName == "End")
                                {
                                    Edm.AssociationEnd end = new Edm.AssociationEnd();
                                    end.Type = reader.GetAttribute("Type");
                                    assoc.FirstEnd = end;
                                }
                                GoToElement(reader, null);
                                if (reader.LocalName == "End")
                                {
                                    Edm.AssociationEnd end = new Edm.AssociationEnd();
                                    end.Type = reader.GetAttribute("Type");
                                    assoc.LastEnd = end;
                                }
                                GoAfterElement(reader, "Association");
                            }
                            else if (reader.LocalName == "FunctionImport")
                            {
                                GoAfterElement(reader, "FunctionImport");
                            }
                            else if (reader.LocalName == "Schema")
                            {
                                GoToElement(reader, "EntityContainer");
                                metadata.Name = reader.GetAttribute("Name");
                                GoToElement(reader, null);
                                while (reader.LocalName == "EntitySet" || reader.LocalName == "AssociationSet")
                                {
                                    if (reader.LocalName == "EntitySet")
                                    {
                                        metadata.EntityTypes[reader.GetAttribute("EntityType")].EntitySetName = reader.GetAttribute("Name");
                                        GoToElement(reader, null);
                                    }
                                    else if (reader.LocalName == "AssociationSet")
                                    {
                                        string assocName = reader.GetAttribute("Association");

                                        Edm.Association assoc;
                                        if (!metadata.Associations.TryGetValue(assocName, out assoc))
                                        {
                                            assoc = new Edm.Association();
                                            assoc.Name = assocName;
                                            metadata.Associations.Add(assocName, assoc);
                                        }
                                        GoToElement(reader, null);
                                        if (reader.LocalName == "End")
                                        {
                                            Edm.AssociationEnd end = new Edm.AssociationEnd();
                                            assoc.FirstEnd.EntitySet = reader.GetAttribute("EntitySet");
                                        }
                                        GoToElement(reader, null);
                                        if (reader.LocalName == "End")
                                        {
                                            Edm.AssociationEnd end = new Edm.AssociationEnd();
                                            assoc.LastEnd.EntitySet = reader.GetAttribute("EntitySet");
                                        }
                                        GoAfterElement(reader, "AssociationSet");
                                    }
                                }
                            }
                            //GoToElement(reader, null);
                        }
                        while (reader.LocalName == "EntityType" || reader.LocalName == "ComplexType" || reader.LocalName == "Association" || reader.LocalName == "FunctionImport" || reader.LocalName == "Schema");
                    }
                }
            }

            //Clean inheritance tree
            foreach (Edm.EntityType entity in metadata.EntityTypes.Values)
            {
                var parentEntity = entity;
                while (string.IsNullOrEmpty(parentEntity.EntitySetName) && !string.IsNullOrEmpty(parentEntity.BaseType))
                {
                    parentEntity = metadata.EntityTypes[parentEntity.BaseType];
                }
                entity.EntitySetName = parentEntity.EntitySetName;
            }

            foreach (Edm.EntityType entity in metadata.EntityTypes.Values)
            {
                if (string.IsNullOrEmpty(entity.BaseType))
                    RegisterMetaData(new MetaData.TypeMetaData(entity.Type));
                else
                    RegisterMetaData(new MetaData.TypeMetaData(entity.Type, entity.BaseType, false));
                foreach (Edm.Property property in entity.Properties.Values)
                {
                    if (property.IsNavigationProperty)
                    {
                        Edm.Association assoc = metadata.Associations[property.Type];
                        RegisterMetaData(new MetaData.ReferenceMetaData(assoc.FirstEnd.Type, property.Name, assoc.LastEnd.Type));
                    }
                    else
                    {
                        RegisterMetaData(new MetaData.PropertyMetaData(entity.Type, property.Name, MetaData.TypeResolver.GetType(property.Type), property.IsId));
                    }
                }
            }

            Metadata = metadata;

#if !SILVERLIGHT
            XmlDocument doc = new XmlDocument();
            doc.Load(stream);
            XmlNamespaceManager xmlnsm = new XmlNamespaceManager(doc.NameTable);
            xmlnsm.AddNamespace("edm", "http://schemas.microsoft.com/ado/2008/09/edm");
            xmlnsm.AddNamespace("edmx", "http://schemas.microsoft.com/ado/2007/06/edmx");
            xmlnsm.AddNamespace("m", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            xmlnsm.AddNamespace("d", "http://schemas.microsoft.com/ado/2007/08/dataservices");
            XmlNodeList nodes = doc.SelectNodes("/edmx:Edmx/edmx:DataServices/*[local-name()='Schema']/*[local-name()='EntityContainer' and @m:IsDefaultEntityContainer='true']/*[local-name()='EntitySet']", xmlnsm);
            if (nodes.Count == 0)
                        nodes = doc.SelectNodes("/edmx:Edmx/edmx:DataServices/*[local-name()='Schema']/*[local-name()='EntityContainer' ans @m:IsDefaultEntityContainer='true']/*[local-name()='EntitySet']", xmlnsm);

            foreach (XmlNode node in nodes)
            {
                string name = node.SelectSingleNode("@Name", xmlnsm).Value;
                string type = node.SelectSingleNode("@EntityType", xmlnsm).Value;
                RegisterMetaData(new MetaData.TypeMetaData(type));
                foreach (XmlNode idNode in doc.SelectNodes("/edmx:Edmx/edmx:DataServices/*[local-name()='Schema']/*[local-name()='EntityType' and substring('" + type + "', string-length('" + type + "') - string-length(@Name) +1)=@Name]/*[local-name()='Key']/*[local-name()='PropertyRef']/@Name", xmlnsm))
                {
                    new MetaData.PropertyMetaData(type, idNode.Value, typeof(string), true);
                }
                Metadata.Add(type, name);
            }
#endif
        }

        private bool GoAfterElement(XmlReader reader, string p)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement && (p == null || reader.LocalName == p))
                {
                    GoToElement(reader, null);
                    return true;
                }
            }
            return false;
        }

        private bool GoToElement(XmlReader reader, string p)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && (p == null || reader.LocalName == p))
                    return true;
            }
            return false;
        }

        public Uri ConnectionString { get; set; }

        public override IPersistenceEngineAsync CreatePersistenceEngineAsync()
        {
            EnsureConfigurationInitialized();
            return new ODataPersistenceEngineAsync(this, ConnectionString);
        }

        public Edm.Metadata Metadata { get; set; }
    }
}
