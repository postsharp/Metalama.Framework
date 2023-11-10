using Metalama.Framework.Aspects;
using System;
using System.Linq;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

#pragma warning disable CS0169
#pragma warning disable CS0414

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.PrimaryConstructorParameter
{
    /*
     * Tests that overriding of fields with initializers that reference primary constructor parameter correctly retains the initializer.
     */

    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            foreach(var field in builder.Target.Fields)
            {
                builder.Advice.Override(field, nameof(OverrideTemplate));
            }
        }

        [Template]
        public dynamic? OverrideTemplate
        {
            get
            {
                Console.WriteLine("This is the overridden getter.");

                return meta.Proceed();
            }

            set
            {
                Console.WriteLine("This is the overridden setter.");
                meta.Proceed();
            }
        }
    }

    // <target>
    internal class TargetClass(int x)
    {
        public int Foo()
        {
            return x++;
        }
    }
}