using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Indexers.ExistingConflictOverrideBaseNonVirtual
{
    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advice.IntroduceIndexer(
                builder.Target,
                nameof(BaseIndexer),
                nameof(BaseIndexer),
                whenExists: OverrideStrategy.Override,
                buildIndexer: i =>
                {
                    i.Type = TypeFactory.GetType(typeof(int));
                    i.AddParameter("x", typeof(int));
                });
        }

        [Template]
        public dynamic? BaseIndexer()
        {
            Console.WriteLine( "This is introduced indexer." );
            return meta.Proceed();
        }
    }

    internal class BaseClass
    {
        public int this[int x]
        {
            get
            {
                return 13;
            }

            set
            {
            }
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass : BaseClass { }
}