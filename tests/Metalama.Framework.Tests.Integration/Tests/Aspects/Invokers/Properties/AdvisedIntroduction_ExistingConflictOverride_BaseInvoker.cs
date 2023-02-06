using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.AdvisedIntroduction_ExistingConflictOverride_BaseInvoker;
using System;
using Metalama.Framework.Code.Invokers;

[assembly: AspectOrder( typeof(OverrideAttribute), typeof(IntroductionAttribute) )]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.AdvisedIntroduction_ExistingConflictOverride_BaseInvoker
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce( WhenExists = OverrideStrategy.Override )]
        public int BaseClassProperty
        {
            get
            {
                Console.WriteLine( "This is introduced property." );

                return meta.Target.FieldOrProperty.Value;
            }
        }

        [Introduce( WhenExists = OverrideStrategy.Override )]
        public int TargetClassProperty
        {
            get
            {
                Console.WriteLine( "This is introduced property." );

                return meta.Target.FieldOrProperty.Value;
            }
        }
    }

    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach (var property in builder.Target.Properties)
            {
                builder.Advice.Override( property, nameof(PropertyTemplate) );
            }
        }

        [Template]
        public dynamic? PropertyTemplate
        {
            get
            {
                Console.WriteLine( "Override" );

                return meta.Target.FieldOrProperty.Value;
            }

            set
            {
                Console.WriteLine( "Override" );
                meta.Target.FieldOrProperty.Value = value;
            }
        }
    }

    internal class BaseClass
    {
        public virtual int BaseClassProperty
        {
            get => 42;
        }
    }

    // <target>
    [Introduction]
    [Override]
    internal class TargetClass : BaseClass
    {
        public virtual int TargetClassProperty
        {
            get => 42;
        }
    }
}