// @OutputAllSyntaxTrees

using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Methods.PartialType_SyntaxTrees
{
    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            foreach (var method in builder.Target.Methods)
            {
                builder.Advices.OverrideMethod(method, nameof(Template), tags: new TagDictionary() { ["name"] = method.Name });
            }
        }

        [Template]
        public dynamic? Template()
        {
            Console.WriteLine($"This is the override of {meta.Tags["name"]}.");

            return meta.Proceed();
        }
    }

    // <target>
    [Override]
    internal partial class TargetClass
    {
        public void TargetMethod1()
        {
            Console.WriteLine("This is TargetMethod1.");
        }
    }
}