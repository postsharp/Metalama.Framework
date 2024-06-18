﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using System;
using System.Runtime.CompilerServices;

#pragma warning disable CS0169
#pragma warning disable CS0414

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.CallerAttributes
{
    /*
     * Tests that overriding property does correctly transforms caller attribute method invocations when the source is not inlined.
     */

    public class OverrideAttribute : PropertyAspect
    {
        public override void BuildAspect( IAspectBuilder<IProperty> builder )
        {
            builder.OverrideAccessors( nameof(Override), nameof(Override) );
        }

        [Template]
        public dynamic? Override()
        {
            // Block inlining.
            _ = meta.Proceed();
            Console.WriteLine( "This is the overridden method." );

            return meta.Proceed();
        }
    }

    // <target>
    internal class TargetClass
    {
        [Override]
        public int OverriddenProperty
        {
            get
            {
                MethodWithCallerMemberName( 42 );
                MethodWithCallerMemberName( 42, y: 27 );
                MethodWithCallerMemberName( 42, name1: "foo", y: 27 );
                MethodWithCallerMemberName( 42, "foo", 27 );
                MethodWithCallerMemberName( 42, "foo", 27, "bar" );

                return 42;
            }

            set
            {
                MethodWithCallerMemberName( 42 );
                MethodWithCallerMemberName( 42, y: 27 );
                MethodWithCallerMemberName( 42, name1: "foo", y: 27 );
                MethodWithCallerMemberName( 42, "foo", 27 );
                MethodWithCallerMemberName( 42, "foo", 27, "bar" );
            }
        }

        public void MethodWithCallerMemberName( int x, [CallerMemberName] string name1 = "", int y = 0, [CallerMemberName] string name2 = "" ) { }
    }
}