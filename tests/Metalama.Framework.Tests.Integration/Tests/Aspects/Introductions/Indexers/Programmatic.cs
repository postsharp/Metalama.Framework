﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Indexer.Programmatic
{
    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advice.IntroduceIndexer(
                builder.Target,
                new[] { (typeof(int), "x") },
                nameof(GetIndexerTemplate),
                nameof(SetIndexerTemplate),
                buildIndexer: p =>
                {
                    p.Accessibility = Accessibility.Public;
                });
        }

        [Template]
        public dynamic? GetIndexerTemplate()
        {
            Console.WriteLine("Introduced");
            return meta.Proceed();
        }

        [Template]
        public void SetIndexerTemplate(dynamic? value)
        {
            Console.WriteLine("Introduced");
            meta.Proceed();
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass { }
}
