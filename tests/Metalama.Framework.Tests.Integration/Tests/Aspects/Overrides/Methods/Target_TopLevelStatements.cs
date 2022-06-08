#if TEST_OPTIONS
// @Include(__TopLevelStatements.cs)
// @OutputAllSyntaxTrees
// @Executable
#endif

using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;
using Metalama.TestFramework;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Methods.Target_TopLevelStatements
{
    // Tests single OverrideMethod aspect with trivial template on methods with trivial bodies.

    public class OverrideAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine("This is the overriding method.");
            return meta.Proceed(); ;
        }
    }

    public class MyProjectFabric : ProjectFabric
    {
        public override void AmendProject(IProjectAmender amender)
        {
            amender.With(p => p.Types.SelectMany(t => t.Methods)).AddAspect(m => new OverrideAttribute());
        }
    }
}