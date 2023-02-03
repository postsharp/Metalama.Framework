using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("test")]

namespace Dependency;

public class IntroducePrivateMembersAttribute : OverrideMethodAspect
{
    [Introduce]
    private readonly RunTimeOnlyClass _field;

    [Introduce]
    private void Method(RunTimeOnlyClass arg) {}

    [Introduce]
    private RunTimeOnlyClass AutoProperty { get; set; }

    [Introduce]
    internal RunTimeOnlyClass Property { get => new(); private set {} }

    [Introduce]
    private event EventHandler<RunTimeOnlyClass> FieldLikeEvent;

    [Introduce]
    private event EventHandler<RunTimeOnlyClass> Event { add {} remove {} }

    public override dynamic OverrideMethod()
    {
        Method(_field);

        Console.WriteLine(AutoProperty);

        Console.WriteLine(Property);

        FieldLikeEvent += (s, e) => Console.WriteLine(e);

        Event += (s, e) => Console.WriteLine(e);

        return meta.Proceed();
    }
}

public class OverrideWithPrivateTemplatesAttribute : TypeAspect
{
    
    [Template]
    private dynamic? MethodTemplate() 
    {
        Console.WriteLine( "Overridden." );
        return meta.Proceed();
    }

    [Template]
    private dynamic? PropertyTemplate 
    {
        get
        {
            Console.WriteLine( "Overridden." );
            return meta.Proceed();
        }
        set
        {
            Console.WriteLine( "Overridden." );
            meta.Proceed();
        }
    }

    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        foreach ( var method in builder.Target.Methods )
        {
            builder.Advice.Override( method, nameof( MethodTemplate ) );
        }
        foreach ( var property in builder.Target.Properties )
        {
            builder.Advice.Override( property, nameof( PropertyTemplate ) );
        }
    }



}

internal class RunTimeOnlyClass : EventArgs { }