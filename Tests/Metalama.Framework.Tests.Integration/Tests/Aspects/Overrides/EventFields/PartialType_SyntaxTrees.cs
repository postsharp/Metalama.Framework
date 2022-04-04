// @OutputAllSyntaxTrees

using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0067

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.EventFields.PartialType_SyntaxTrees
{
    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            foreach (var @event in builder.Target.Events)
            {
                builder.Advices.OverrideEventAccessors(@event, nameof(Template), nameof(Template), null, tags: new TagDictionary() { ["name"] = @event.Name });
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
        public event EventHandler? TargetEvent1;
    }
}