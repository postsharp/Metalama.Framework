using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Indexers.ExistingConflictOverride
{
    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advice.IntroduceIndexer(
                builder.Target, 
                nameof(ExistingBaseIndexer),
                nameof(ExistingBaseIndexer), 
                whenExists: OverrideStrategy.Override,
                buildIndexer: i =>
                {
                    i.Type = TypeFactory.GetType(typeof(int));
                    i.AddParameter("x", typeof(int));
                });

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

            builder.Advice.IntroduceIndexer(
                builder.Target, 
                nameof(NotExistingIndexer), 
                nameof(NotExistingIndexer), 
                whenExists: OverrideStrategy.Override,
                buildIndexer: i =>
                {
                    i.Type = TypeFactory.GetType(typeof(int));
                    i.AddParameter("x", typeof(int));
                    i.AddParameter("y", typeof(int));
                    i.AddParameter("z", typeof(int));
                });
        }

        [Template]
        public dynamic? ExistingBaseIndexer()
        {
            meta.InsertComment("Call the base indexer.");
            return meta.Proceed();
        }

        [Template]
        public dynamic? ExistingIndexer()
        {
            meta.InsertComment("Return a constant/do nothing.");
            return meta.Proceed();
        }

        [Template]
        public dynamic? NotExistingIndexer()
        {
            meta.InsertComment("Return default value/do nothing.");
            return meta.Proceed();
        }
    }

    internal class BaseClass
    {
        public virtual int this[int x]
        {
            get
            {
                return 27;
            }
            set
            {
            }
        }
    }

    // <target>
    [Introduction]
    internal class TargetClass : BaseClass
    {
        public virtual int this[int x, int y]
        {
            get
            {
                return 27;
            }
            set
            {
            }
        }
    }
}