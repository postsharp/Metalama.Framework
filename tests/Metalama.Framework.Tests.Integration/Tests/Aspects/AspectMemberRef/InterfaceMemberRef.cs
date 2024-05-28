using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0067

namespace Metalama.Framework.IntegrationTests.Aspects.AspectMemberRef.InterfaceMemberRef
{
    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advice.ImplementInterface( builder.Target, typeof(IInterface) );
        }

        [Introduce]
        public void SomeMethod()
        {
            Method();
            Property = Property + 1;
            Event += EventHandler;
        }

        [Introduce]
        private void EventHandler( object? sender, EventArgs a ) { }

        [Introduce]
        public void Method() { }

        [Introduce]
        public int Property { get; set; }

        [Introduce]
        public event EventHandler? Event;
    }

    internal interface IInterface
    {
        void Method();

        int Property { get; set; }

        event EventHandler? Event;
    }

    // <target>
    [Introduction]
    internal class Program { }
}