using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Properties.AccessorVisibility
{
    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advise.IntroduceProperty( builder.Target, nameof(PropertyWithRestrictedGet) );
            builder.Advise.IntroduceProperty( builder.Target, nameof(AutoPropertyWithRestrictedGet) );
            builder.Advise.IntroduceProperty( builder.Target, nameof(PropertyWithRestrictedSet) );
            builder.Advise.IntroduceProperty( builder.Target, nameof(AutoPropertyWithRestrictedSet) );
            builder.Advise.IntroduceProperty( builder.Target, nameof(PropertyWithRestrictedInit) );
            builder.Advise.IntroduceProperty( builder.Target, nameof(AutoPropertyWithRestrictedInit) );
            builder.Advise.IntroduceProperty( builder.Target, nameof(ProtectedAutoPropertyWithPrivateProtectedSetter) );
            builder.Advise.IntroduceProperty( builder.Target, nameof(ProtectedInternalAutoPropertyWithProtectedSetter) );
        }

        [Template]
        public int PropertyWithRestrictedGet
        {
            private get
            {
                return 42;
            }

            set { }
        }

        [Template]
        public int AutoPropertyWithRestrictedGet { private get; set; }

        [Template]
        public int PropertyWithRestrictedSet
        {
            get
            {
                return 42;
            }

            private set { }
        }

        [Template]
        public int AutoPropertyWithRestrictedSet { get; private set; }

        [Template]
        public int PropertyWithRestrictedInit
        {
            get
            {
                return 42;
            }

            private init { }
        }

        [Template]
        public int AutoPropertyWithRestrictedInit { get; private init; }

        [Template]
        protected int ProtectedAutoPropertyWithPrivateProtectedSetter { get; private protected set; }

        [Template]
        protected internal int ProtectedInternalAutoPropertyWithProtectedSetter { get; protected set; }
    }

    // <target>
    [Introduction]
    internal class TargetClass { }
}