using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.TemplateProvider_Explicit
{
    /*
     * Tests a simple case with implicit interface member defined in a template provider.
     */

    public interface IInterface
    {
        void Foo();
        void Bar();
    }

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advice.WithTemplateProvider(new TemplateProvider()).ImplementInterface( builder.Target, typeof(IInterface) );
        }
    }

    public class TemplateProviderBase : ITemplateProvider
    {
        [ExplicitInterfaceMember]
        public void Foo()
        {
            Console.WriteLine("Introduced interface member");
        }
    }

    public class TemplateProvider : TemplateProviderBase
    { 
        [ExplicitInterfaceMember]
        public void Bar()
        {
            Console.WriteLine("Introduced interface member");
        }
    }

    // <target>
    [Introduction]
    public class TargetClass { }
}