using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateTypeParameters.Filter;

#pragma warning disable CS8618, CS0169

internal class MyAspect : FieldAspect
{
    public override void BuildAspect( IAspectBuilder<IField> builder )
    {
        builder.Advice.AddFilter( builder.Target, nameof(Filter),  args: new { T = builder.Target.Type } );
    }

    [Template]
    public void Filter<[CompileTime] T>( T? value )
    {
        Console.WriteLine( typeof(T).Name );
    }
}

// <target>
internal class Target
{
    [MyAspect]
    private string q;
}