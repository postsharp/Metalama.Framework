﻿using System;
using static Metalama.Framework.Tests.LinkerTests.Tests.Api;

namespace Metalama.Framework.Tests.LinkerTests.Tests.Events.Inliners.AddAssignment_NotValue
{
    // <target>
    public class Target
    {
        private EventHandler? field;

        event EventHandler Foo
        {
            add
            {
                Console.WriteLine("Original");
                this.field += value;
            }
            remove { }
        }

        [PseudoOverride(nameof(Foo), "TestAspect")]
        private event EventHandler Foo_Override
        {
            add
            {
                Console.WriteLine("Before");
                link[_this.Foo.add, inline] += (EventHandler)((s, ea) => { });
                Console.WriteLine("After");
            }
            remove { }
        }
    }
}
