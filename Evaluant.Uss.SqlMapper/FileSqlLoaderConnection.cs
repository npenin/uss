using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Evaluant.Uss.Data.FileClient;
using System.IO;

namespace Evaluant.Uss.SqlMapper
{
    public class FileSqlLoaderConnection : FileConnection
    {
        //public override void Open()
        //{
        //    state = ConnectionState.Open;
        //    sw = new StreamWriter(filename, true);
        //}

        protected IDictionary<string, FileConnection> fileConnections = new Dictionary<string, FileConnection>();

        public override void Close()
        {
            base.Close();

            foreach (FileConnection connection in fileConnections.Values)
                connection.Close();
        }

        string path = string.Empty;

        protected override void ParseConnectionString()
        {
            base.ParseConnectionString();
            if (filename.Contains("\\"))
                path = filename.Substring(0, filename.LastIndexOf('\\') + 1);
            else if (filename.Contains("/"))
                path = filename.Substring(0, filename.LastIndexOf('/') + 1);
        }

        internal override void ExecuteCommand(FileCommand command)
        {
            if (command.CommandText.StartsWith("-- TABLE"))
            {
                command.CommandText = command.CommandText.Substring("-- TABLE ".Length);
                StringReader sr = new StringReader(command.CommandText);
                string tableName = sr.ReadLine();
                command.CommandText = sr.ReadToEnd();
                sr.Close();
                FileConnection connection;
                if (!fileConnections.ContainsKey(tableName))
                {
                    connection = new FileConnection();
                    connection.ConnectionString = "filename=" + path + tableName + ".asc";
                    connection.Open();
                    connection.Dialect = Dialect;
                    fileConnections.Add(tableName, connection);
                }
                else
                    connection = fileConnections[tableName];
                connection.ExecuteCommand(command);
            }
            else if (command.CommandText.StartsWith("-- UPDATE"))
            {
                command.CommandText = command.CommandText.Substring("-- UPDATE ".Length);
                TextReader sr = new StringReader(command.CommandText);
                string tableName = sr.ReadLine();
                command.CommandText = sr.ReadToEnd();
                sr.Close();
                command.CommandText = TransformToSql(command);
                FileConnection connection;
                if (fileConnections.ContainsKey(tableName))
                {
                    connection = fileConnections[tableName];
                    connection.Close();
                    fileConnections.Remove(tableName);
                }
                FileStream fs = File.Open(path + tableName + ".asc", FileMode.Open);
                sr = new StreamReader(fs);
                string[] values = command.CommandText.Split('$');
                int position = 0;
                bool found = false;
                while (!found && !((StreamReader)sr).EndOfStream)
                {
                    string line = sr.ReadLine();
                    string[] lineValues = line.Split('$');
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(values[i]) && !string.IsNullOrEmpty(lineValues[i]) && values[i] == lineValues[i])
                        {
                            found = true;
                            for (int j = 0; j < values.Length; j++)
                            {
                                if (string.IsNullOrEmpty(values[j]))
                                    values[j] = lineValues[j];
                            }
                            break;
                        }
                    }
                    if (found)
                    {
                        string toEnd = sr.ReadToEnd();
                        fs.Seek(position, SeekOrigin.Begin);
                        StreamWriter sw = new StreamWriter(fs);
                        sw.WriteLine(string.Join("$", values) + "$");
                        sw.Write(toEnd);
                        sw.Close();
                    }
                    else
                        position += line.Length + StringWriter.Null.NewLine.Length;

                }
            }
            else
            {
                command.CommandText = command.CommandText.Substring("-- LOAD ".Length);
                TextReader sr = new StringReader(command.CommandText);
                string tableName = sr.ReadLine();
                command.CommandText = sr.ReadToEnd();
                sr.Close();
                base.Close();
                filename = path + tableName + ".ctl";
                base.Open();
                base.ExecuteCommand(command);
            }
        }
    }
}
