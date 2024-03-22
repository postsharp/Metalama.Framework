﻿using Metalama.Framework.Aspects;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Fields.ExistingConflictFail
{ 
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce(WhenExists = OverrideStrategy.Fail)]
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