using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Inheritance.ChildAspect_;

[assembly: AspectOrder( typeof(ChildAspect), typeof(ParentAspect) )]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Inheritance.ChildAspect_;

public class ParentAspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        base.BuildAspect( builder );

        builder.With( t => t.Methods ).AddAspect( _ => new ChildAspect() );
    }
}

[Inherited]
public class ChildAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine( "From ChildAspect" );

        return meta.Proceed();
    }
}

// <target>
internal class Targets
{
    [ParentAspect]
    public class BaseTarget
    {
        public virtual void M() { }
    }

    public class DerivedTarget : BaseTarget
    {
        public override void M()
        {
            Console.WriteLine( "Hello, world." );
        }
    }
}