using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Evaluant.Uss.Tests
{
    public class UnitTestTraceListener : TraceListener
    {
        TextWriter writer;

        public UnitTestTraceListener()
        {
            writer = Console.Out;
        }

        public UnitTestTraceListener(TestContext context)
            : this()
        {
            this.context = context;
            //writer=context.Tra
        }

        StringBuilder sb = new StringBuilder();
        private TestContext context;

        public override void Write(string message)
        {
            if (context != null)
                context.WriteLine(message);
            else
            {

                try
                {
                    writer.Write(message);
                }
                catch (Exception)
                {
                }

            }
            //sb.Append(message);
        }

        public override void WriteLine(string message)
        {
            //Write(message);
            //Debug.WriteLine(sb.ToString());
            if (context != null)
                context.WriteLine(message);
            else
            {

                try
                {
                    writer.WriteLine(message);
                }
                catch (Exception)
                {
                }

            }

            //sb = new StringBuilder();
        }
    }
}
