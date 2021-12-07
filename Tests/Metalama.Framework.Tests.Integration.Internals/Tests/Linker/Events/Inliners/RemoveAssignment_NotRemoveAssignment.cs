﻿using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.Events.Inliners.RemoveAssignment_NotRemoveAssignment
{
    // <target>
    public class Target
    {
        private EventHandler? field;

        event EventHandler Foo
        {
            add { }
            remove
            {
                Console.WriteLine("Original");
                this.field -= value;
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect")]
        private event EventHandler Foo_Override
        {
            add { }
            remove
            {
                Console.WriteLine("Before");
                link[_this.Foo.add, inline] += null;
                Console.WriteLine("After");
            }
        }
    }
}
