using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Uninlineable;

[assembly: AspectOrder( typeof(FirstOverrideAttribute), typeof(SecondOverrideAttribute) )]

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Uninlineable
{
    [AttributeUsage( AttributeTargets.Property, AllowMultiple = true )]
    public class FirstOverrideAttribute : PropertyAspect
    {
        public override void BuildAspect( IAspectBuilder<IProperty> builder )
        {
            builder.Advice.OverrideAccessors( builder.Target, nameof(GetTemplate), nameof(SetTemplate) );
        }

        [Template]
        public dynamic? GetTemplate()
        {
            Console.WriteLine( "This is the overridden getter." );
            _ = meta.Proceed();

            return meta.Proceed();
        }

        [Template]
        public void SetTemplate()
        {
            Console.WriteLine( "This is the overridden setter." );
            meta.Proceed();
            meta.Proceed();
        }
    }

    [AttributeUsage( AttributeTargets.Property, AllowMultiple = true )]
    public class SecondOverrideAttribute : PropertyAspect
    {
        public override void BuildAspect( IAspectBuilder<IProperty> builder )
        {
            builder.Advice.OverrideAccessors( builder.Target, nameof(GetTemplate), nameof(SetTemplate) );
        }

        [Template]
        public dynamic? GetTemplate()
        {
            Console.WriteLine( "This is the overridden getter." );
            _ = meta.Proceed();

            return meta.Proceed();
        }

        [Template]
        public void SetTemplate()
        {
            Console.WriteLine( "This is the overridden setter." );
            meta.Proceed();
            meta.Proceed();
        }
    }

    // <target>
    internal class TargetClass
    {
        private int _field;

        [FirstOverride]
        [SecondOverride]
        public int Property
        {
            get
            {
                return _field;
            }

            set
            {
                _field = value;
            }
        }
    }
}