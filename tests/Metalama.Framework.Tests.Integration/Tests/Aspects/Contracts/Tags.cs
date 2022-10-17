using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS8618, CS0169

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Contracts.Tags;

internal class MyAspect : FieldAspect
{
    public override void BuildAspect( IAspectBuilder<IField> builder )
    {
        builder.Advice.AddContract( builder.Target, nameof(Filter),  tags: new { tag = "tag"} );
    }

    [Template]
    public void Filter( dynamic? value )
    {
        Console.WriteLine( (string?) meta.Tags["tag"] );
    }
}

// <target>
internal class Target
{
    [MyAspect]
    private string q;
}