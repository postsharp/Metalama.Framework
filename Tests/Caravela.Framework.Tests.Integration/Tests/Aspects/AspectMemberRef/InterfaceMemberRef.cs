using System;
using System.ComponentModel;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using Caravela.Framework.Code;

namespace Caravela.Framework.IntegrationTests.Aspects.AspectMemberRef.InterfaceMemberRef
{

    public class DisposeAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.AdviceFactory.ImplementInterface( builder.TargetDeclaration, typeof(IDisposable) );
        }

        [Introduce]
        public void SomeMethod()
        {
            this.Dispose();
        }

        [InterfaceMember]
        void Dispose()
        {
            
        }
        
    }
    
    // <target>
    [Dispose]
    class Program
    {

    }
}