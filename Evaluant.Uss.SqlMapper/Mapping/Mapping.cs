using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using Evaluant.Uss.SqlExpressions.Mapping;
using Evaluant.Uss.SqlMapper.Mapping;
using Evaluant.Uss.SqlMapper.Mapper;

namespace Evaluant.Uss.SqlMapper.Mapping
{
    [XmlRoot("Mapping", Namespace = "http://www.evaluant.com/euss/sqlMapper/mapping")]
    public class Mapping : Era.Model<Entity, Attribute, Reference>
    {
        public Mapping()
        {
            Tables = new Dictionary<string, Table>();
            Entities = new Dictionary<string, Entity>();
        }

        [XmlIgnore]
        public Model.Model Model { get; set; }

        public void Initialize(bool needsModel)
        {
            foreach (Entity e in Entities.Values)
            {
                e.Mapping = this;
                e.Initialize(needsModel);
            }
        }

        [XmlIgnore]
        public IMapper Mapper { get; set; }

        [XmlIgnore]
        public Dictionary<string, Table> Tables { get; private set; }

        public void Save(Stream stream)
        {
            XmlSerializer s = new XmlSerializer(GetType());
            s.Serialize(stream, this);
        }


        public static Mapping Load(string path)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(Mapping));
            Mapping mapping = null;
            try
            {
                using (FileStream fileStream = new FileStream(path, FileMode.Open))
                {
                    mapping = xmlSerializer.Deserialize(fileStream) as Mapping;
                }
            }
            catch (Exception e)
            {
                throw new MappingException("An error occured while loading the mapping file: " + path, e);
            }
            return mapping;
        }
    }
}
