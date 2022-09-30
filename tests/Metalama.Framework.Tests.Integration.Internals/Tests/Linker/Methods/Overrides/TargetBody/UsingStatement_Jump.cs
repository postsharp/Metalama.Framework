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
            using var z11 = new Disposable();
            using var z12 = new Disposable();

            if (x == 0)
            {
                return;
            }

            Console.WriteLine("Before first dispose");

            using var z21 = new Disposable();
            using var z22 = new Disposable();

            Console.WriteLine("Before double dispose");

            using var z31 = new Disposable();
            using var z32 = new Disposable();

            Console.WriteLine("After dispose");
        }

        [PseudoOverride( nameof(Foo), "TestAspect1")]

        void Foo_Override1(int x)
        {
            Console.WriteLine("Before aspect1");

            using var z41 = new Disposable();
            using var z42 = new Disposable();

            if (x == 0)
            {
                return;
            }

            using var z51 = new Disposable();
            using var z52 = new Disposable();

            link(_this.Foo, inline)(x);

            using var z61 = new Disposable();
            using var z62 = new Disposable();

            Console.WriteLine("After aspect1");
        }

        [PseudoOverride(nameof(Foo), "TestAspect2")]

        void Foo_Override2(int x)
        {
            Console.WriteLine("Before aspect2");

            using var z71 = new Disposable();
            using var z72 = new Disposable();

            if (x == 0)
            {
                return;
            }

            using var z81 = new Disposable();
            using var z82 = new Disposable();

            link(_this.Foo, inline)(x);

            using var z91 = new Disposable();
            using var z92 = new Disposable();

            Console.WriteLine("After aspect2");
        }
    }
}
