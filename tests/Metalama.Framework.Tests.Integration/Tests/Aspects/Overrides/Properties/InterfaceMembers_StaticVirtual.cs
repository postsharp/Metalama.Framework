﻿#if TEST_OPTIONS
// @RequiredConstant(NET7_0_OR_GREATER) - Not available on .NET Framework
// @RequiredConstant(ROSLYN_4_4_0_OR_GREATER)
#endif

#if NET7_0_OR_GREATER && ROSLYN_4_4_0_OR_GREATER

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Properties.InterfaceMembers_StaticVirtual
{
    /*
     * Tests overriding of interface static virtual methods.
     */

    internal class OverrideAttribute : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty 
        { 
            get
            {
                Console.WriteLine("Override.");
                return meta.Proceed();
            }

            set
            {
                Console.WriteLine("Override.");
                meta.Proceed();
            }
        }
    }

    // <target>
    public interface Interface
    {
        [Override]
        public static virtual int StaticVirtualProperty
        {
            get => 42;
            set { }
        }
    }

    // <target>
    public class TargetClass : Interface
    {
    }
}

#endif