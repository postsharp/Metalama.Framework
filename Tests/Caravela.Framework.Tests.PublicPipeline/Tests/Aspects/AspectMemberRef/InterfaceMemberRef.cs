using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

#pragma warning disable CS0067

namespace Caravela.Framework.IntegrationTests.Aspects.AspectMemberRef.InterfaceMemberRef
{
    public class IntroduceAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advices.ImplementInterface( builder.Target, typeof(IInterface) );
        }

        [Framework.Aspects.Introduce]
        public void SomeMethod()
        {
            Method();
            Property = Property + 1;
            Event += EventHandler;
        }

        [Framework.Aspects.Introduce]
        private void EventHandler( object? sender, EventArgs a ) { }

        [InterfaceMember]
        private void Method() { }

        [InterfaceMember]
        private int Property { get; set; }

        [InterfaceMember]
        private event EventHandler? Event;
    }

    internal interface IInterface
    {
        void Method();

        int Property { get; set; }

        event EventHandler Event;
    }

    // <target>
    [Introduce]
    internal class Program { }
}