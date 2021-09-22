﻿// @Skipped(#29134 - Invokers.Base is null for an override aspect applied to a field)

using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using Caravela.Framework.IntegrationTests.Aspects.Invokers.Fields.AdvisedIntroduction_Value;
using System.Linq;

[assembly: AspectOrder(typeof(TestAttribute), typeof(TestIntroductionAttribute))]

namespace Caravela.Framework.IntegrationTests.Aspects.Invokers.Fields.AdvisedIntroduction_Value
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TestIntroductionAttribute : Attribute, IAspect<INamedType>
    {
        [Introduce]
        public int Field;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class TestAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.AdviceFactory.OverrideFieldOrProperty(builder.Target.Fields.OfName(nameof(TestIntroductionAttribute.Field)).Single(), nameof(PropertyTemplate));
        }

        [Template]
        public dynamic? PropertyTemplate
        { 
            get
            {
                return meta.Target.FieldOrProperty.Value;
            }

            set
            {
                meta.Target.FieldOrProperty.Value = value;
            }
        }
    }

    // <target>
    [TestIntroduction]
    [Test]
    internal class TargetClass
    {
    }
}
