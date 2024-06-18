using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;
using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Tests.Integration.Tests.Licensing.CodeFixApplyAspect;

internal class SuggestManualImplementationAttribute : MethodAspect
{
    private static DiagnosticDefinition _diag = new( "MY001", Severity.Warning, "Implement manually" );

    public override void BuildAspect( IAspectBuilder<IMethod> builder )
    {
        base.BuildAspect( builder );

        builder.Diagnostics.Report( _diag.WithCodeFixes( CodeFixFactory.ApplyAspect( builder.Target, new MyAspect() ) ) );
    }
}

internal class MyAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine( "This line was implemented using application of and aspect using a code fix." );

        return meta.Proceed();
    }
}

// <target>
internal class TargetCode
{
    [SuggestManualImplementationAttribute]
    private int Method( int a )
    {
        return a;
    }
}