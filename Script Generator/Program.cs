using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.Uss.SqlMapper.Mapping;
using Evaluant.Uss.SqlMapper;
using Evaluant.Uss.SqlExpressions.Mapping;
using Evaluant.Uss.SqlExpressions;
using System.IO;
using CommandLineParser;
using Evaluant.Uss.SqlExpressions.Statements;
using Evaluant.Uss.SqlMapper.Drivers;

namespace Script_Generator
{
    class Program
    {
        static void Main(string[] args)
        {
            ArgsParser parser = new ArgsParser(
                new Parameter("mapping", "m", true, "the mapping file from which to generate the script"),
                new Parameter("dialect", "d", true, "the dialect to use, eg. SqlServer"),
                new Parameter("output", "o", false, "the script file to be generated"));
            if (parser.Parse(args))
            {
                Mapping m = Mapping.Load(parser["mapping"]);
                m.Initialize(false);
                Type dialectType = Type.GetType(parser["dialect"]);
                if (dialectType == null)
                    dialectType = Type.GetType("Evaluant.Uss.SqlMapper.Dialects." + parser["dialect"] + ", Evaluant.Uss.SqlMapper");
                if (dialectType == null)
                {
                    Console.WriteLine("The dialect named " + parser["dialect"] + " could not be found");
                    return;
                }

                IDialect dialect = (IDialect)Activator.CreateInstance(dialectType);

                SqlMapperProvider provider = new SqlMapperProvider();
                provider.Dialect = dialect;
                provider.Driver = new Script();
                provider.Mapping = m;

                provider.Driver.Initialize(parser["output"] ?? "script.sql");
                provider.MarkAsInitialized();
                provider.CreatePersistenceEngine().InitializeRepository();

                //using (TextWriter file = File.CreateText())
                //{
                //    foreach (Table table in m.Tables.Values)
                //        file.WriteLine(dialect.Render((IDbExpression)new CreateTableStatement(table)));

                //    List<string> list = new List<string>();

                //    foreach (var entity in m.Entities)
                //    {
                //        foreach (var reference in entity.Value.References.Values)
                //        {
                //            bool isFirst = reference.Rules.Count > 1;

                //            foreach (var rule in reference.Rules)
                //            {
                //                if (list.Contains(reference.Name + "_" + rule.ParentTableName))
                //                    continue;

                //                list.Add(reference.Name + "_" + rule.ParentTableName);

                //                if (isFirst)
                //                {
                //                    isFirst = false;
                //                    file.WriteLine(dialect.Render((IDbExpression)new AlterTableAddStatement()
                //                    {
                //                        ConstraintName = "FK_" + rule.ParentTableName + "_" + reference.Name,
                //                        Table = rule.ChildTable,
                //                        Constraint = new ForeignKeyConstraint()
                //                        {
                //                            Fields = rule.ChildFields,
                //                            ReferencesTable = rule.ParentTable,
                //                            References = rule.ParentFields
                //                        }
                //                    }));
                //                }
                //                else
                //                {
                //                    file.WriteLine(dialect.Render((IDbExpression)new AlterTableAddStatement()
                //                    {
                //                        ConstraintName = "FK_" + rule.ParentTableName + "_" + reference.Name,
                //                        Table = rule.ParentTable,
                //                        Constraint = new ForeignKeyConstraint()
                //                        {
                //                            Fields = rule.ParentFields,
                //                            ReferencesTable = rule.ChildTable,
                //                            References = rule.ChildFields
                //                        }
                //                    }));

                //                }
                //            }
                //        }

                //    }
                //    file.Close();
                //}

                Console.WriteLine("The script was successfully generated...");
                Console.Read();
            }
        }
    }
}
