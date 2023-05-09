﻿using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Fields.ExistingConflictNew
{ 
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce(WhenExists = OverrideStrategy.New)]
        public int ExistingField;
    }

    internal class BaseClass
    {
        public int ExistingField;
    }

    // <target>
    [Introduction]
    internal class TargetClass : BaseClass
    {
    }
}