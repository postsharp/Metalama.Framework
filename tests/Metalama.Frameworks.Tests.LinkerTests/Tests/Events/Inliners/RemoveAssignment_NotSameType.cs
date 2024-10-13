﻿using System;
using static Metalama.Framework.Tests.LinkerTests.Tests.Api;

namespace Metalama.Framework.Tests.LinkerTests.Tests.Events.Inliners.RemoveAssignment_NotSameType
{
    public class Base
    {
        protected virtual event EventHandler Foo
        {
            add { }
            remove { }
        }
    }


    // <target>
    public class Target : Base
    {
        [PseudoIntroduction("TestAspect")]
        protected override event EventHandler Foo
        {
            add { }
            remove { }
        }

        [PseudoOverride(nameof(Foo), "TestAspect")]
        private event EventHandler Foo_Override
        {
            add
            {
            }
            remove
            {
                Console.WriteLine("Before");
                link[_this.Foo.add, inline, @base] -= value;
                Console.WriteLine("After");
            }
        }
    }
}
