﻿using System;
using System.Collections.Generic;
using System.Text;
using Caravela.Framework.Aspects;
using Caravela.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Methods.NameConflict;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

[assembly: AspectOrder(typeof(InnerOverrideAttribute), typeof(OuterOverrideAttribute))]

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Methods.NameConflict
{
    public class InnerOverrideAttribute : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            int i = 27;
            return proceed();
        }
    }

    public class OuterOverrideAttribute : OverrideMethodAspect
    {
        public override dynamic OverrideMethod()
        {
            int i = 42;
            return proceed();
        }
    }

    [TestOutput]
    internal class TargetClass
    {
        [InnerOverride]
        [OuterOverride]
        public int TargetMethod_ConflictBetweenOverrides()
        {
            return 42;
        }

        [InnerOverride]
        public int TargetMethod_ConflictWithTarget()
        {
            int i = 0;
            return 42;
        }

        [InnerOverride]
        [OuterOverride]
        public int TargetMethod_MultipleConflicts()
        {
            int i = 0;
            return 42;
        }
    }
}
