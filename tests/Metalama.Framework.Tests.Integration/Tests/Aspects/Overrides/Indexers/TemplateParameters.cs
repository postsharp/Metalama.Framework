using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.TestFramework;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.TemplateParameters
{
    public class TestAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            foreach (var indexer in builder.Target.Indexers)
            {
                builder.Advice.OverrideAccessors(indexer, nameof(GetIndexer), nameof(SetIndexer));
            }
        }

        [Template]
        public dynamic? GetIndexer(dynamic? x)
        {
            Console.WriteLine($"Override [{x}]");
            return meta.Proceed();
        }

        [Template]
        public void SetIndexer(dynamic? x, dynamic? value)
        {
            Console.WriteLine($"Override [{x}] {value}");
            meta.Proceed();
        }
    }

    // <target>
    [Test]
    internal class TargetClass
    {
        public int this[int x]
        {   
            get
            {
                Console.WriteLine("Original");
                return x; 
            }

            set
            {
                Console.WriteLine("Original");
            }
        }
    }
}
