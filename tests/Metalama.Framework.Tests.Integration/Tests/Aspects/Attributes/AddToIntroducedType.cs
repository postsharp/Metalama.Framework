using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.AddToIntroducedType;

#pragma warning disable CS0067, CS0169

[assembly: AspectOrder( AspectOrderDirection.RunTime, typeof(AddAttributeAspect), typeof(IntroducingAspect), typeof(TypeIntroducingAspect) )]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.AddToIntroducedType;

internal class TypeIntroducingAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var result = builder.IntroduceClass( "TestType" );
        builder.With( result.Declaration ).Outbound.AddAspect<IntroducingAspect>();
        builder.With( result.Declaration ).Outbound.AddAspect<AddAttributeAspect>();
    }
}

internal class IntroducingAspect : TypeAspect
{
    [Introduce]
    private int _field;

    [Introduce]
    private string? Property { get; set; }

    [Introduce]
    private long Method( string p ) => 0;

    [Introduce]
    public event EventHandler? Event;
}

internal class AddAttributeAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var attribute = AttributeConstruction.Create( typeof(MyAttribute) );

        builder.IntroduceAttribute( attribute );

        foreach (var field in builder.Target.Fields)
        {
            builder.With( field ).IntroduceAttribute( attribute );
        }

        foreach (var property in builder.Target.Properties)
        {
            builder.With( property ).IntroduceAttribute( attribute );
        }

        foreach (var @event in builder.Target.Events)
        {
            builder.With( @event ).IntroduceAttribute( attribute );
        }

        foreach (var method in builder.Target.Methods)
        {
            builder.With( method ).IntroduceAttribute( attribute );
            builder.With( method.ReturnParameter ).IntroduceAttribute( attribute );

            foreach (var parameter in method.Parameters)
            {
                builder.With( parameter ).IntroduceAttribute( attribute );
            }
        }
    }
}

internal class MyAttribute : Attribute { }

// <target>
[TypeIntroducingAspect]
internal class C { }