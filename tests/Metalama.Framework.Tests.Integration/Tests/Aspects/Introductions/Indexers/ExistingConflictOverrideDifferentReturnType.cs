using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Indexers.ExistingConflictOverrideDifferentReturnType
{
    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advice.IntroduceIndexer(
                builder.Target,
                nameof(ExistingIndexer),
                nameof(ExistingIndexer),
                whenExists: OverrideStrategy.Override,
                buildIndexer: i =>
                {
                    i.Type = TypeFactory.GetType(typeof(int));
                    i.AddParameter("x", typeof(int));
                });
        }

        [Template]
        public dynamic? ExistingIndexer()
        {
            Console.WriteLine("This is introduced indexer.");
            return meta.Proceed();
        }
    }

    internal class BaseClass
    {
        public virtual object? this[int x]
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