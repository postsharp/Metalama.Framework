using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.EventFields.Overrides.Jump.EventHandler_FJ_FJ_NJ
{
    // <target>
    class Target
    {
        event EventHandler? Foo;

        [PseudoOverride( nameof(Foo),"TestAspect1")]
        event EventHandler? Foo_Override1
        {
            add
            {
                Console.WriteLine("Before1");
                if (new Random().Next() == 0)
                {
                    return;
                }
                link[_this.Foo, inline] += value;
                Console.WriteLine("After1");
            }

            remove
            {
                Console.WriteLine("Before1");
                if (new Random().Next() == 0)
                {
                    return;
                }
                link[_this.Foo, inline] -= value;
                Console.WriteLine("After1");
            }
        }

        [PseudoOverride( nameof(Foo),"TestAspect2")]
        event EventHandler? Foo_Override2
        {
            add
            {
                Console.WriteLine("Before2");
                if (new Random().Next() == 0)
                {
                    return;
                }
                link[_this.Foo, inline] += value;
                Console.WriteLine("After2");
            }

            remove
            {
                Console.WriteLine("Before2");
                if (new Random().Next() == 0)
                {
                    return;
                }
                link[_this.Foo, inline] -= value;
                Console.WriteLine("After2");
            }
        }
    }
}
