﻿using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0169

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Properties.PromoteSource
{
    [AttributeUsage( AttributeTargets.Field )]
    public class PromoteAttribute : FieldOrPropertyAspect
    {
        public override void BuildAspect( IAspectBuilder<IFieldOrProperty> builder )
        {
            builder.Advice.Override( builder.Target, nameof(Template) );
        }

        [Template]
        public dynamic? Template
        {
            get
            {
                return meta.Proceed();
            }

            set
            {
                meta.Proceed();
            }
        }
    }

    // <target>
    internal class TargetClass
    {
        [Promote]
        private int _field;

        public void Foo()
        {
            Console.WriteLine( "Original code." );
        }
    }
}