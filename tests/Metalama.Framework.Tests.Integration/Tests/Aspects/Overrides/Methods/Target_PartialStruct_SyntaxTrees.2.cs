using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Methods.Target_PartialStruct_SyntaxTrees
{
    // <target>
    internal partial struct TargetStruct
    {
        public void TargetMethod3()
        {
            Console.WriteLine("This is TargetMethod3.");
        }
    }
}