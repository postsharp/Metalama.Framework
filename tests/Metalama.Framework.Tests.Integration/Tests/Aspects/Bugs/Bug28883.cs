using System;
using System.Collections.Generic;
using Metalama.Framework.Code;
using Metalama.Framework;
using Metalama.Testing.AspectTesting;
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
                return meta.Target.FieldOrProperty.GetValue( meta.This );
            }

            set
            {
                meta.Target.FieldOrProperty.SetValue( meta.This, value );
            }
        }
    }

    // <target>
    internal class TargetCode
    {
        [TestAttribute]
        private int Property { get; set; }
    }
}