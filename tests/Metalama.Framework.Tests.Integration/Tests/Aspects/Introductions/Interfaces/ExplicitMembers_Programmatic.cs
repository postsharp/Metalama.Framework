using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0067 

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Interfaces.ExplicitMembers_Programmatic;

/*
 * Simple case with explicit interface members for a single interface, implemented programmatically.
 */

public interface IInterface
{
    int InterfaceMethod();

    event EventHandler Event;

    event EventHandler? EventField;

    int Property { get; set; }

    int AutoProperty { get; set; }
}

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        var explicitImplementation = builder.Advice.ImplementInterface(builder.Target, typeof(IInterface)).ExplicitImplementation;

        explicitImplementation.IntroduceMethod(nameof(InterfaceMethod));
        explicitImplementation.IntroduceEvent(nameof(Event));
        explicitImplementation.IntroduceEvent(nameof(EventField));
        explicitImplementation.IntroduceProperty(nameof(Property));
        explicitImplementation.IntroduceProperty(nameof(AutoProperty));
    }

    [Template]
    public int InterfaceMethod()
    {
        Console.WriteLine( "This is introduced interface member." );

        return meta.Proceed();
    }

    [Template]
    public event EventHandler? Event
    {
        add
        {
            Console.WriteLine( "This is introduced interface member." );
            meta.Proceed();
        }

        remove
        {
            Console.WriteLine( "This is introduced interface member." );
            meta.Proceed();
        }
    }

    [Template]
    public event EventHandler? EventField;

    [Template]
    public int Property
    {
        get
        {
            Console.WriteLine( "This is introduced interface member." );

            return meta.Proceed();
        }

        set
        {
            Console.WriteLine( "This is introduced interface member." );
            meta.Proceed();
        }
    }

    [Template]
    public int AutoProperty { get; set; }
}

// <target>
[Introduction]
public class TargetClass { }