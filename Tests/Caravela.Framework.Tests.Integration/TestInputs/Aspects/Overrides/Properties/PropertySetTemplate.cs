using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.PropertySetTemplate
{
    public class OverrideAttribute : IAspect<IFieldOrProperty>
    {
        public dynamic? OverrideProperty
        {
            set
            {
                Console.WriteLine($"This is the overridden setter.");
                var discard = meta.Proceed();
            }
        }
    }
}
