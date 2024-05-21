using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.TemplateProvider
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
            var factory = builder.Advice.WithTemplateProvider(new TemplateProvider());

            factory.ImplementInterface( builder.Target, typeof(IInterface) );

            factory.IntroduceMethod(builder.Target, nameof(TemplateProviderBase.Foo));
            factory.IntroduceMethod(builder.Target, nameof(TemplateProvider.Bar));
        }
    }

    public class TemplateProviderBase : ITemplateProvider
    {
        [Template]
        public void Foo()
        {
            Console.WriteLine("Introduced interface member");
        }
    }

    public class TemplateProvider : TemplateProviderBase
    { 
        [Template]
        public void Bar()
        {
            Console.WriteLine("Introduced interface member");
        }
    }

    // <target>
    [Introduction]
    public class TargetClass { }
}