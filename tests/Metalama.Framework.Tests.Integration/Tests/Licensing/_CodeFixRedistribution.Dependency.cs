using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;
using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Tests.Integration.Tests.Licensing.CodeFixRedistribution.Dependency;

public class SuggestMyAttributeRedistributableAttribute : MethodAspect
{
    private static DiagnosticDefinition _diag = new( "MY001", Severity.Warning, "Add some attribute" );

    public override void BuildAspect( IAspectBuilder<IMethod> builder )
    {
        base.BuildAspect( builder );

        builder.Diagnostics.Report( _diag.WithCodeFixes( CodeFixFactory.AddAttribute( builder.Target, typeof(MyAttribute) ) ) );
    }
}

public class MyAttribute : Attribute { }