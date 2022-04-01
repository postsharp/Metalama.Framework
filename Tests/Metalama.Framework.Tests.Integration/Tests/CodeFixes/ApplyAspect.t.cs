// Warning MY001 on `Method`: `Apply Aspect2`
//    CodeFix: Apply`
using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Tests.Integration.CodeFixes.ApplyAspect;

namespace Metalama.Framework.Tests.Integration.CodeFixes.ApplyAspect
{
#pragma warning disable CS0067
    internal class Aspect1 : MethodAspect
    {
        private static DiagnosticDefinition _diag = new( "MY001", Severity.Warning, "Apply Aspect2" );

        public override void BuildAspect(IAspectBuilder<IMethod> builder) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");

    }
#pragma warning restore CS0067
#pragma warning disable CS0067

    internal class Aspect2 : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");

    }
#pragma warning restore CS0067

    internal class MyAttribute : Attribute { }

    internal class TargetCode
    {
        [Aspect1]
        private int Method( int a )
        {
            return a;
        }
    }
}