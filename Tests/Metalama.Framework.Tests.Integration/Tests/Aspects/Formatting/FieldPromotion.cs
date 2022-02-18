using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Formatting.FieldPromotion
{
    public class TestAspect : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            foreach (var property in builder.Target.Properties.Where(p => !p.IsAbstract && p.Writeability == Writeability.All))
            {
                builder.Advices.OverrideFieldOrPropertyAccessors(property, null, nameof(this.OverridePropertySetter));
            }
        }

        [Template]
        private dynamic OverridePropertySetter(dynamic value)
        {
            if (value != meta.Target.Property.Value)
            {
                meta.Proceed();
            }

            return value;
        }
    }

    // <target>
    [TestAspect]
    public class Target
    {
        public int _field;

        public int GetAutoProperty { get; }
        
        public int InitAutoProperty { get; init; }

        public int AutoProperty { get; set; }

        public int Property
        {
            get => _field;
            set => _field = value;
        }
    }
}
