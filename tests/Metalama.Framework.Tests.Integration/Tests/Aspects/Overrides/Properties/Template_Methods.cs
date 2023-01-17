﻿using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Template_Methods
{
    /*
     * Tests method templates for both accessors.
     */

    // TODO: Get-only auto-property does not get override.

    [AttributeUsage( AttributeTargets.Property )]
    public class TestAttribute : FieldOrPropertyAspect
    {
        public override void BuildAspect( IAspectBuilder<IFieldOrProperty> builder )
        {
            builder.Advise.OverrideAccessors( builder.Target, nameof(GetProperty), nameof(SetProperty) );
        }

        [Template]
        public dynamic? GetProperty()
        {
            Console.WriteLine( "This is the overridden getter." );

            return meta.Proceed();
        }

        [Template]
        public void SetProperty()
        {
            Console.WriteLine( "This is the overridden setter." );
            meta.Proceed();
        }
    }

    // <target>
    internal class TargetClass
    {
        [Test]
        public int BlockBodiedAccessors
        {
            get
            {
                Console.WriteLine( "Original" );

                return 42;
            }
            set
            {
                Console.WriteLine( "Original" );
            }
        }

        [Test]
        public int ExpressionBodiedAccessors
        {
            get => 42;
            set => Console.WriteLine( "Original" );
        }

        [Test]
        public int ExpressionBodiedProperty => 42;

        [Test]
        public int AutoProperty { get; set; }

        [Test]
        public int AutoGetOnlyProperty { get; }
    }
}