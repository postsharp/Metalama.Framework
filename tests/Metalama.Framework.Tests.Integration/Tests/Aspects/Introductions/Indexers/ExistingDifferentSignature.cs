using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Indexers.ExistingDifferentSignature
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
                    i.AddParameter("y", typeof(int));
                });
        }

        [Template]
        public dynamic? ExistingIndexer()
        {
            Console.WriteLine("This is introduced indexer.");
            return meta.Proceed();
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass
    {
        public virtual int this[int x]
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
}