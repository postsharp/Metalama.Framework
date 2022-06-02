using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Formatting.InnerBlock
{
    public class TestAspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            if (BoolSource.Value)
            {
                return default;
            }
            else
            {
                var result = meta.Proceed();
                return result;
            }
        }
    }

    internal static class BoolSource
    {
        public static bool Value;
    }

    // <target>
    public class Target
    {
        [TestAspect]
        private static int Add(int a, int b)
        {
            Console.WriteLine("Thinking...");
            return a + b;
        }
    }
}
