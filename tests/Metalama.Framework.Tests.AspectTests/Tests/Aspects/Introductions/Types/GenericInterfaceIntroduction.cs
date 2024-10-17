using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.AspectTests.Tests.Aspects.Introductions.Types.GenericInterfaceIntroduction;

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var nestedType = builder.IntroduceClass( "TestNestedType" );
        var equatable = builder.Target.Compilation.Factory.GetNamedTypeByReflectionType( typeof(IEquatable<>) ).MakeGenericInstance( nestedType.Declaration );
        nestedType.ImplementInterface( equatable );
        nestedType.IntroduceMethod( nameof(Equals), args: new { T = nestedType.Declaration } );
    }

    [Template]
    public bool Equals<[CompileTime] T>( T other ) => true;
}

// <target>
[IntroductionAttribute]
public class TargetType { }