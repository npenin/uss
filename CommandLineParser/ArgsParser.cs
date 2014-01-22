using System;
using System.Collections.Generic;
using System.Text;

namespace CommandLineParser
{
    public class ArgsParser
    {
        Dictionary<Parameter, string> arguments;
        private List<Parameter> parameters;

        public ArgsParser(params Parameter[] parameters)
        {
            this.parameters = new List<Parameter>(parameters);
            this.parameters.Add(new Parameter("help", "h", "Displays this help"));
        }

        public ArgsParser(IEnumerable<Parameter> parameters)
        {
            this.parameters = new List<Parameter>(parameters);
            this.parameters.Add(new Parameter("help", "h", "Displays this help"));
        }

        public string this[Parameter key]
        {
            get
            {
                string result;
                if (arguments.TryGetValue(key, out result))
                    return result;
                return null;
            }
        }

        public string this[string key]
        {
            get
            {
                foreach (var param in arguments)
                {
                    if (param.Key.FullName == key || param.Key.Name == key)
                        return param.Value;
                }
                return null;
            }
        }

        public IEnumerable<Parameter> Keys
        {
            get
            {
                return arguments.Keys;
            }
        }

        public IEnumerable<Parameter> Parameters
        {
            get
            {
                return parameters;
            }
        }

        public bool HasFlag(string key)
        {
            return this[key] == "#Flag";
        }

        public bool Parse(string[] args)
        {
            try
            {
                string key = null;

                if (args == null || args.Length == 0)
                {
                    AddFlag("help");
                    Help();
                    return false;
                }

                foreach (string arg in args)
                {
                    if (arg.Length > 0 && (arg[0] == '/' || arg[0] == '-'))
                    {
                        if (!string.IsNullOrEmpty(key))
                            AddFlag(key);

                        key = arg;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(key))
                            throw new ArgumentException("You have not specified any parameter for " + arg);
                        AddArgument(key, arg);
                        key = null;
                    }
                }

                if (!string.IsNullOrEmpty(key))
                {
                    AddFlag(key);
                    if (HasFlag("help"))
                    {
                        Help();
                        return false;
                    }
                }

                foreach (Parameter parameter in parameters)
                {
                    if (parameter.Mandatory && !arguments.ContainsKey(parameter))
                        throw new ArgumentNullException("You have not specified the mandatory parameter " + parameter.FullName);
                }

                return true;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);
                Help();
                return false;
            }
        }

        private void Help()
        {
            Console.WriteLine(Description);
            Console.WriteLine();
            foreach (Parameter param in parameters)
            {
                Console.WriteLine("-" + param.Name + "\t--" + param.FullName);
                Console.WriteLine("\t\t" + param.Help);
                Console.WriteLine();
            }
            return;
        }

        private void AddArgument(string key, string arg)
        {
            foreach (Parameter p in parameters)
            {
                if ("/" + p.FullName == key ||
                    "--" + p.FullName == key ||
                    "/" + p.Name == key ||
                    "-" + p.Name == key)
                {
                    if (arguments == null)
                        arguments = new Dictionary<Parameter, string>();
                    arguments.Add(p, arg);
                    return;
                }
            }
            throw new ArgumentOutOfRangeException("The parameter named " + key + " does not exist.");
        }

        private void AddFlag(string key)
        {
            AddArgument(key, "#Flag");
        }

        public string Description { get; set; }
    }
}
