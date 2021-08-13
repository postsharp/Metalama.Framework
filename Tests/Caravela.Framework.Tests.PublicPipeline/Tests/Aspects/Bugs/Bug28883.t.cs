using System;
using System.Collections.Generic;
using Caravela.Framework.Code;
using Caravela.Framework;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

// This checks that throw expressions in expression bodies work properly.
// Part of the fix was that the transformed run-time code for the aspect was incorrect, so we also cover it with tests.


namespace Caravela.Framework.Tests.Integration.Aspects.Bugs.Bug28883
{
    public class TestAttribute : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty
{
    get => throw new System.NotSupportedException("Compile-time only code cannot be called at run-time.");
    set => throw new System.NotSupportedException("Compile-time only code cannot be called at run-time.");
}

    }

    class TargetCode
    {
        [TestAttribute]
        int Property {get    {
        return this.Property;
    }

set    {
        this.Property= value;
    }
}
        
      
    }
}