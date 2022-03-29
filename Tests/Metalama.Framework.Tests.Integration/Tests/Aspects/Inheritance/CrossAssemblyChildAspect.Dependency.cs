using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Inheritance.CrossAssemblyChildAspect;

[assembly: AspectOrder( typeof(ChildAspect), typeof(ParentAspect) )]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Inheritance.CrossAssemblyChildAspect
{
    public class ParentAspect : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            base.BuildAspect( builder );

            builder.WithTargetMembers( t => t.Methods ).AddAspect( _ => new ChildAspect() );
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

    [ParentAspect]
    public interface I
    {
        void M();
    }
}