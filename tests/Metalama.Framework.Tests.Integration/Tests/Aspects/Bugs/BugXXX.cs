using System;
using System.Linq;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using  Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.BugXXX;

[assembly: AspectOrder( AspectOrderDirection.CompileTime, typeof(IntroductionAspect), typeof(ReadAspect))]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.BugXXX;

public class IntroductionAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        base.BuildAspect( builder );

        builder.IntroduceConstructor(
            nameof(ConstructorTemplate),
            buildConstructor: c =>
            {
                c.Parameters[0].Type = builder.Target.TypeArguments[0];
            } );
    }

    [Template]
    void ConstructorTemplate( dynamic x ) { }

    [Introduce]
    void IntroducedMethod()
    {
        
    }
}

public class ReadAspect : TypeAspect
{
    [Introduce(WhenExists = OverrideStrategy.New)]
    public void PrintBaseConstructors()
    {
        foreach (var c in meta.Target.Type.BaseType.Constructors.OrderBy( c=>c.ToString() ))
        {
            Console.WriteLine( c.ToDisplayString(  ) );
        }
    }
    
    [Introduce(WhenExists = OverrideStrategy.New)]
    public void PrintBaseMethods()
    {
        foreach (var c in meta.Target.Type.BaseType.Methods.OrderBy( m=>m.ToString() ))
        {
            Console.WriteLine( c.ToDisplayString(  ) );
        }
    }
}

// <target>
[IntroductionAspect,ReadAspect]
internal class C<T>
{
    public C() { }

    public void M() { }
}

// <target>
[ReadAspect]
internal class D : C<int> { }