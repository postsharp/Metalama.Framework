﻿// @Skipped(#28883)

using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;

namespace Caravela.Framework.IntegrationTests.Aspects.Invokers.Properties.AdvisedSource_FinalInvoker
{
    public class TestAttribute : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty 
        { 
            get
            {
                return meta.FieldOrProperty.Invokers.Final.GetValue( meta.This );
            }

            set
            {
                meta.FieldOrProperty.Invokers.Final.SetValue( meta.This, value );
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
