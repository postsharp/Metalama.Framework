﻿// @Skipped(BUG_ID_TBD)
// @OutputAllSyntaxTrees

using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Fields.PartialType_SyntaxTrees
{
    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            foreach (var field in builder.Target.Fields)
            {
                builder.Advices.OverrideFieldOrPropertyAccessors(field, nameof(Template), nameof(Template), tags: new TagDictionary() { ["name"] = field.Name });
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
        public int TargetField1;
    }
}