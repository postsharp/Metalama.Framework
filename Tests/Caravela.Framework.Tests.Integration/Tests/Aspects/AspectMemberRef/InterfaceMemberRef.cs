using System;
using System.ComponentModel;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using Caravela.Framework.Code;

#pragma warning disable CS0067

namespace Caravela.Framework.IntegrationTests.Aspects.AspectMemberRef.InterfaceMemberRef
{

    public class IntroduceAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.AdviceFactory.ImplementInterface( builder.TargetDeclaration, typeof(IInterface) );
        }

        [Framework.Aspects.Introduce]
        public void SomeMethod()
        {
            this.Method();
            this.Property = this.Property + 1;
            this.Event += this.EventHandler;
        }

        [Framework.Aspects.Introduce]
        void EventHandler( object? sender, EventArgs a ) { }

        [InterfaceMember]
        void Method()
        {
            
        }

        [InterfaceMember]
        private int Property { get; set; }

        [InterfaceMember]
        private event EventHandler? Event;

    }

    interface IInterface
    {
        void Method();
        int Property { get; set; }

        event EventHandler Event;
    }
    
    // <target>
    [Introduce]
    class Program
    {

    }
}