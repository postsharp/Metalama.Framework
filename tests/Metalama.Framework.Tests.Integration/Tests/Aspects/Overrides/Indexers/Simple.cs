using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.TestFramework;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.Simple
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
        public dynamic? GetIndexer()
        {
            Console.WriteLine("Override");
            return meta.Proceed();
        }

        [Template]
        public void SetIndexer()
        {
            Console.WriteLine("Override");
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
