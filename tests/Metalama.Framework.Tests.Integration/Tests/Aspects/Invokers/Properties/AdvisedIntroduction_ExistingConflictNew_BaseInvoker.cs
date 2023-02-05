using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.AdvisedIntroduction_ExistingConflictNew_BaseInvoker;
using System;
using Metalama.Framework.Code.Invokers;

[assembly: AspectOrder( typeof(TestAttribute), typeof(IntroductionAttribute) )]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.AdvisedIntroduction_ExistingConflictNew_BaseInvoker
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce( WhenExists = OverrideStrategy.New )]
        public int BaseClassProperty
        {
            get
            {
                Console.WriteLine( "This is introduced property." );

                return meta.Target.FieldOrProperty.With( InvokerOptions.Base ).Value;
            }
        }

        [Introduce( WhenExists = OverrideStrategy.New )]
        public int TargetClassProperty
        {
            get
            {
                Console.WriteLine( "This is introduced property." );

                return meta.Target.FieldOrProperty.With( InvokerOptions.Base ).Value;
            }
        }
    }

    public class TestAttribute : TypeAspect
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

                return meta.Target.FieldOrProperty.With( InvokerOptions.Base ).Value;
            }

            set
            {
                Console.WriteLine( "Override" );
                meta.Target.FieldOrProperty.With( InvokerOptions.Base ).Value = value;
            }
        }
    }

    internal class BaseClass
    {
        public int BaseClassProperty
        {
            get => 42;
        }
    }

    // <target>
    [Introduction]
    [Test]
    internal class TargetClass : BaseClass
    {
        public int TargetClassProperty
        {
            get => 42;
        }
    }
}