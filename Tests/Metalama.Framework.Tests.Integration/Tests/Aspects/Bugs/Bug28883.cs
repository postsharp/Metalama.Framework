using System;
using System.Collections.Generic;
using Metalama.Framework.Code;
using Metalama.Framework;
using Metalama.TestFramework;
using Metalama.Framework.Aspects;

// This checks that throw expressions in expression bodies work properly.
// Part of the fix was that the transformed run-time code for the aspect was incorrect, so we also cover it with tests.


namespace Metalama.Framework.Tests.Integration.Aspects.Bugs.Bug28883
{
    public class TestAttribute : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty 
        { 
            get
            {
                return meta.Target.FieldOrProperty.Invokers.Final.GetValue( meta.This );
            }
    
            set
            {
                meta.Target.FieldOrProperty.Invokers.Final.SetValue( meta.This, value );
            }
        }
    }

    // <target>
    class TargetCode
    {
        [TestAttribute]
        int Property { get; set; }
    }
}