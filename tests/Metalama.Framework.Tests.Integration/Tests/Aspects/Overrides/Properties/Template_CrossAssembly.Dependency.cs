using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Properties.Template_CrossAssembly
{
    public class TestAspect : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach (var property in builder.Target.Properties)
            {
                builder.Advice.Override( property, nameof(Override) );
            }
        }

        [Template]
#pragma warning disable CA1822 // Mark members as static
        public string? Override
#pragma warning restore CA1822 // Mark members as static
        {
            get
            {
                Console.WriteLine( "Aspect code" );

                return meta.Proceed();
            }

            set
            {
                Console.WriteLine( "Aspect code" );
                meta.Proceed();
            }
        }
    }
}