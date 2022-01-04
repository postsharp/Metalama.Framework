using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.NoProceed
{
    [AttributeUsage( AttributeTargets.Property, AllowMultiple = true )]
    public class OverrideAttribute : Attribute, IAspect<IProperty>
    {
        void IAspect<IProperty>.BuildAspect( IAspectBuilder<IProperty> builder )
        {
            builder.Advices.OverrideFieldOrPropertyAccessors( builder.Target, nameof(GetTemplate), nameof(SetTemplate) );
        }

        [Template]
        public dynamic? GetTemplate()
        {
            Console.WriteLine( "This is the overridden getter." );

            return default;
        }

        [Template]
        public void SetTemplate()
        {
            Console.WriteLine( "This is the overridden setter." );
        }

        public void BuildEligibility( IEligibilityBuilder<IProperty> builder ) { }
    }

    // <target>
    internal class TargetClass
    {
        private int _field;

        [Override]
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