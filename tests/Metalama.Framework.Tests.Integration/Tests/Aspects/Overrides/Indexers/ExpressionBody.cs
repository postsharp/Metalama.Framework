using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Indexers.ExpressionBody
{
    public class TestAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach (var indexer in builder.Target.Indexers)
            {
                builder.Advice.OverrideAccessors( indexer, nameof(GetIndexer), null );
            }
        }

        [Template]
        public dynamic? GetIndexer()
        {
            Console.WriteLine( "Override" );

            return meta.Proceed();
        }
    }

    // <target>
    [Test]
    internal class TargetClass
    {
        public int this[ int x ] => x;
    }
}