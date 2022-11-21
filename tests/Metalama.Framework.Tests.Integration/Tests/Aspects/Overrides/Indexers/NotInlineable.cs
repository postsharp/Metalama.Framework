using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.TestFramework;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.NotInlineable
{
    public class TestAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            foreach (var indexer in builder.Target.Indexers)
            {
                builder.Advice.OverrideAccessors(indexer, nameof(GetIndexer), nameof(SetIndexer), tags: new { T = "first" });
                builder.Advice.OverrideAccessors(indexer, nameof(GetIndexer), nameof(SetIndexer), tags: new { T = "second" });
                builder.Advice.OverrideAccessors(indexer, nameof(GetIndexer), nameof(SetIndexer), tags: new { T = "third" });
                builder.Advice.OverrideAccessors(indexer, nameof(GetIndexer), nameof(SetIndexer), tags: new { T = "fourth" });
            }
        }

        [Template]
        public dynamic? GetIndexer()
        {
            Console.WriteLine($"Override {meta.Tags["T"]}");
            var x = meta.Proceed();
            return meta.Proceed();
        }

        [Template]
        public void SetIndexer()
        {
            Console.WriteLine($"Override {meta.Tags["T"]}");
            meta.Proceed();
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

        public string this[string x]
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
