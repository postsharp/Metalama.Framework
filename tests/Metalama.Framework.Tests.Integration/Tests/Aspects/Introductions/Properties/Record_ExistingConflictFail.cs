using Metalama.Framework.Aspects;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Properties.Record_ExistingConflictFail
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce( WhenExists = OverrideStrategy.Fail )]
        public int ExistingProperty
        {
            get => 42;
        }

        [Introduce( WhenExists = OverrideStrategy.Fail )]
        public static int ExistingProperty_Static
        {
            get => 42;
        }
    }

    // <target>
    [Introduction]
    internal record TargetRecord
    {
        public int ExistingProperty
        {
            get => 13;
        }

        public static int ExistingProperty_Static
        {
            get => 13;
        }
    }
}