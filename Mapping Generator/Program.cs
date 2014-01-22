using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.SqlMapper.Mapper;
using Evaluant.Uss.SqlMapper;
using Evaluant.Uss.MetaData;
using Evaluant.Uss.Model;
using Evaluant.Uss.SqlMapper.Mapping;
using System.Xml.Serialization;
using System.IO;
using CommandLineParser;
using System.Reflection;

namespace MappingGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            ArgsParser parser = new ArgsParser(
                new Parameter("assembly", "a", "the assembly from which the mapping should be generated"),
                new Parameter("model", "m", "the model from which the mapping should be generated"),
                new Parameter("namespace", "n", "maps only the namespace(s) you specify"),
                new Parameter("driver", "d", true, "the SQL driver to use, eg. SqlServer"),
                new Parameter("output", "o", true, "the file where the mapping should be generated")
            );
            parser.Description = "This tool helps you to generate a default mapping. Here are the available commands :";
            if (parser.Parse(args))
            {
                Model model = new Model();
                var visitor = new ModelMetaDataVisitor(model);
                string assembly = parser["assembly"];
                if (assembly != null)
                {
                    string @namespace = parser["namespace"];
                    string[] namespaces = null;
                    if (@namespace != null)
                        namespaces = @namespace.Split(' ');
                    foreach (IMetaData metadata in MetaDataFactory.FromAssembly(Assembly.LoadFile(assembly), namespaces))
                        metadata.Accept(visitor);
                }

                Mapping mapping = new Mapping();
                Type driver = Type.GetType(parser["driver"]);
                if (driver == null)
                    driver = Type.GetType("Evaluant.Uss.SqlMapper.Drivers." + parser["driver"] + ", Evaluant.Uss.SqlMapper");
                if (driver == null)
                {
                    Console.WriteLine("The driver named " + parser["driver"] + " could not be found");
                    return;
                }
                IMapper mapper = new DefaultMapper((IDriver)Activator.CreateInstance(driver), mapping);
                mapper.Map(model);
                XmlSerializer serializer = new XmlSerializer(typeof(Mapping));
                using (Stream s = File.OpenWrite(parser["output"]))
                {
                    serializer.Serialize(s, mapping);
                    s.Close();
                }
                Console.WriteLine("The mapping was generated successfully...");
                Console.ReadLine();
            }
        }
    }
}
