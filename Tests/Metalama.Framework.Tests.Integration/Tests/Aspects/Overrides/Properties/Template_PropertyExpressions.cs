﻿using Metalama.Framework.Aspects;
using Metalama.TestFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Properties.Template_PropertyExpressions
{
    /*
     * Tests that expression bodied template works against all kinds of bodies of the property.
     */

    internal class TestAttribute : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty
        {
            get => meta.ParseExpression("default");
            set => Console.WriteLine("Overridden");
        }
    }

    // <target>
    public class Target
    {
        [Test]
        public int BlockBodiedAccessors
        {
            get
            {
                Console.WriteLine("Original");
                return 42;
            }
            set
            {
                Console.WriteLine("Original");
            }
        }

        [Test]
        public int ExpressionBodiedAccessors
        {
            get => 42;            
            set => Console.WriteLine("Original");
        }

        [Test]
        public int ExpressionBodiedProperty => 42;


        [Test]
        public int AutoProperty { get; set; }

        [Test]
        public int AutoGetOnlyProperty { get; }
    }
}
