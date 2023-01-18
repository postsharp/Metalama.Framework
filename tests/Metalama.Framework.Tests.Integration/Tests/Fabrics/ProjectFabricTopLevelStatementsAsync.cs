﻿#if TEST_OPTIONS
// @Include(Include\__TopLevelStatementsAsync.cs)
// @OutputAllSyntaxTrees
// @OutputAssemblyType(Exe)
#endif

using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Fabrics;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.IntegrationTests.Aspects.Fabrics.ProjectFabricTopLevelStatementsAsync
{
    /*
     * Tests that applying an override on all methods does not target the async Main method containing top-level statements.
     */

    public class OverrideAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine( "This is the overriding method." );

            return meta.Proceed();

            ;
        }
    }

    public class MyProjectFabric : ProjectFabric
    {
        public override void AmendProject( IProjectAmender amender )
        {
            amender.Outbound.SelectMany( p => p.Types.SelectMany( t => t.Methods ) ).AddAspectIfEligible( m => new OverrideAttribute() );
        }
    }
}