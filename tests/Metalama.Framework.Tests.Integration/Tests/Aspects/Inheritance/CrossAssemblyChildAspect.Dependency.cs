using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Inheritance.CrossAssemblyChildAspect;

[assembly: AspectOrder( AspectOrderDirection.RunTime, typeof(ChildAspect), typeof(ParentAspect) )]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Inheritance.CrossAssemblyChildAspect
{
    public class ParentAspect : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            base.BuildAspect( builder );

            builder.Outbound.SelectMany( t => t.Methods ).AddAspect( _ => new ChildAspect() );
        }
    }

    [Inheritable]
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