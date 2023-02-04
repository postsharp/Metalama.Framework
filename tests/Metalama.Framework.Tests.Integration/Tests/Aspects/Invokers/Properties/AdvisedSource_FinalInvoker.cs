﻿using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.IntegrationTests.Aspects.Invokers.Properties.AdvisedSource_FinalInvoker
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
    internal class TargetClass
    {
        [Test]
        public int Property { get; set; }
    }
}