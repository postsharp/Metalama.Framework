#if TEST_OPTIONS
// @TestScenario(ApplyCodeFix)
#endif

using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Tests.Integration.CodeFixes.ApplyAspect;

[assembly: AspectOrder( AspectOrderDirection.RunTime, typeof(Aspect1), typeof(Aspect2) )]

namespace Metalama.Framework.Tests.Integration.CodeFixes.ApplyAspect
{
    internal class Aspect1 : MethodAspect
    {
        private static DiagnosticDefinition _diag = new( "MY001", Severity.Warning, "Apply Aspect2" );

        public override void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            base.BuildAspect( builder );

            builder.Diagnostics.Report( _diag.WithCodeFixes( CodeFixFactory.ApplyAspect( builder.Target, new Aspect2(), "Apply" ) ) );
        }
    }

    internal class Aspect2 : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine( "Oops" );

            return meta.Proceed();
        }
    }

    internal class MyAttribute : Attribute { }

    // <target>
    internal class TargetCode
    {
        [Aspect1]
        private int Method( int a )
        {
            return a;
        }
    }
}