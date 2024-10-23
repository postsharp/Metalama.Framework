#if TEST_OPTIONS
// @TestScenario(CodeFix)
#endif

using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;
using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Tests.AspectTests.CodeFixes.AddAttribute
{
    internal class Aspect : MethodAspect
    {
        private static DiagnosticDefinition _diag = new( "MY001", Severity.Warning, "Add some attribute" );

        public override void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            base.BuildAspect( builder );

            builder.Diagnostics.Report( _diag.WithCodeFixes( CodeFixFactory.AddAttribute( builder.Target, typeof(MyAttribute) ) ) );
        }
    }

    internal class MyAttribute : Attribute { }

    // <target>
    internal class TargetCode
    {
        [Aspect]
        private int Method( int a )
        {
            return a;
        }
    }
}