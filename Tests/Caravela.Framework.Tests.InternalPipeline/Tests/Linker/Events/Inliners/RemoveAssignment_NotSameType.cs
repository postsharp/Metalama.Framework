﻿using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.Events.Inliners.RemoveAssignment_NotSameType
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
