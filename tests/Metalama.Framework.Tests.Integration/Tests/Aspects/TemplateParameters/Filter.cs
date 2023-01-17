using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateParameters.Filter;

#pragma warning disable CS8618, CS0169

internal class MyAspect : FieldAspect
{
    public override void BuildAspect( IAspectBuilder<IField> builder )
    {
        builder.Advise.AddContract( builder.Target, nameof(Filter),  args: new { p = "hey"} );
    }

    [Template]
    public void Filter( dynamic? value, [CompileTime] string p )
    {
        Console.WriteLine( p );
    }
}

// <target>
internal class Target
{
    [MyAspect]
    private string q;
}