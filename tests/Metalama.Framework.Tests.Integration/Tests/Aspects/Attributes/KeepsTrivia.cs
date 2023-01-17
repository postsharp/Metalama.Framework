using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.DeclarationBuilders;
using Metalama.Framework.Fabrics;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.KeepsTrivia;

// This test adds an aspect to the class C and tests that the [target] comment is not removed.

public class MyAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advise.IntroduceAttribute( builder.Target, AttributeConstruction.Create( typeof(SerializableAttribute) ) );
    }
}

public class MyFabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender )
    {
        amender.Amend.SelectMany( c => c.Types.OfName( "C" ) ).AddAspect<MyAspect>();
    }
}

// <target>
internal class C { }