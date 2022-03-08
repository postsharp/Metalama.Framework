// @ExpectedEndOfLine(CR)

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Formatting.EndOfLines_CR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: AspectOrder(typeof(TestAspect1), typeof(TestAspect2))]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Formatting.EndOfLines_CR
{
    public class TestAspect1 : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            meta.InsertStatement("Console.WriteLine(\"Hello!\");\n");
            return meta.Proceed();
        }
    }

    public class TestAspect2 : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            meta.InsertStatement("Console.WriteLine(\"Hello!\");\r\n");
            return meta.Proceed();
        }
    }

    internal static class BoolSource
    {
        public static bool Value;
    }

    // <target>
    public class Target
    {
        [TestAspect1]
        [TestAspect2]
        private static int Add(int a, int b)
        {
            Console.WriteLine("Thinking...");
            return a + b;
        }
    }
}
