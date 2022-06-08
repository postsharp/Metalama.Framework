#if TEST_OPTIONS
// @Include(Include\__TopLevelStatements.cs)
// @OutputAllSyntaxTrees
// @ExecuteProgram
#endif

using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;
using Metalama.TestFramework;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Methods.Target_TopLevelStatements
{
    /*
     * Tests that applying an override on all methods does not target the method containing top-level statements.
     */

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