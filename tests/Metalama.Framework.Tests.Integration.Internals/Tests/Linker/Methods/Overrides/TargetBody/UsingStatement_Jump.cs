using System;
using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Methods.Overrides.TargetBody.UsingStatement_Jump
{
    class Disposable : IDisposable
    {
        public void Dispose()
        {
        }
    }

    // <target>
    class Target
    {
        void Foo(int x)
        {
            if (x == 0)
            {
                return;
            }

            Console.WriteLine("Before first dispose");

            using var z1 = new Disposable();

            Console.WriteLine("Before double dispose");

            using var z2 = new Disposable();
            using var z3 = new Disposable();

            Console.WriteLine("After dispose");
        }

        [PseudoOverride( nameof(Foo), "TestAspect")]

        void Foo_Override(int x)
        {
            Console.WriteLine("Before aspect");
            link(_this.Foo, inline)(x);
            Console.WriteLine("After aspect");
        }
    }
}
