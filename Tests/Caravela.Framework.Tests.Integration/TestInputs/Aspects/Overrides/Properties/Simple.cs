using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using System;
using System.Collections.Generic;
using System.Text;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Simple
{
    public class OverrideAttribute : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty
        {
            get
            {
                Console.WriteLine("This is the overridden getter.");
                return proceed();
            }

            set
            {
                Console.WriteLine($"This is the overridden setter.");
                var discard = proceed();
            }
        }
    }

    [TestOutput]
    internal class TargetClass
    {
        private int _field;

        [Override]
        public int Property
        {
            get
            {
                return this._field;
            }

            set
            {
                this._field = value;
            }
        }
    }
}