using System;
using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Methods.Linking.StaticSourceNew
{
    public class Base
    {
        public static void Bar()
        {
        }
    }

    [PseudoLayerOrder("TestAspect0")]
    [PseudoLayerOrder("TestAspect1")]
    [PseudoLayerOrder("TestAspect2")]
    [PseudoLayerOrder("TestAspect3")]
    [PseudoLayerOrder("TestAspect4")]
    [PseudoLayerOrder("TestAspect5")]
    [PseudoLayerOrder("TestAspect6")]
    // <target>
    class Target : Base
    {
        public static void Foo()
        {
            System.Console.WriteLine("This is original code (discarded).");
        }

        public static new void Bar()
        {
            Console.WriteLine("This is original code.");
        }

        [PseudoOverride(nameof(Foo), "TestAspect0")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public static void Foo_Override0()
        {
            // Should invoke source code.
            link(_static.Target.Bar, @base)();
            // Should invoke source code.
            link(_static.Target.Bar, previous)();
            // Should invoke source code.
            link(_static.Target.Bar, current)();
            // Should invoke the final declaration.
            link(_static.Target.Bar, final)();
        }

        [PseudoOverride(nameof(Foo), "TestAspect2")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public static void Foo_Override2()
        {
            // Should invoke override 1_2.
            link(_static.Target.Bar, @base)();
            // Should invoke override 1_2.
            link(_static.Target.Bar, previous)();
            // Should invoke override 1_2.
            link(_static.Target.Bar, current)();
            // Should invoke the final declaration.
            link(_static.Target.Bar, final)();
        }

        [PseudoOverride(nameof(Foo), "TestAspect4")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public static void Foo_Override4()
        {
            // Should invoke override 3_2.
            link(_static.Target.Bar, @base)();
            // Should invoke override 3_2.
            link(_static.Target.Bar, previous)();
            // Should invoke override 3_2.
            link(_static.Target.Bar, current)();
            // Should invoke the final declaration.
            link(_static.Target.Bar, final)();
        }

        [PseudoOverride(nameof(Foo), "TestAspect6")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public static void Foo_Override6()
        {
            // Should invoke the final declaration.
            link(_static.Target.Bar, @base)();
            // Should invoke the final declaration.
            link(_static.Target.Bar, previous)();
            // Should invoke the final declaration.
            link(_static.Target.Bar, current)();
            // Should invoke the final declaration.
            link(_static.Target.Bar, final)();
        }

        [PseudoOverride(nameof(Bar), "TestAspect1")]
        [PseudoNotInlineable]
        static void Bar_Override1_1()
        {
            // Should invoke source code.
            link(_static.Target.Bar, @base)();
            // Should invoke source code.
            link(_static.Target.Bar, previous)();
            // Should invoke override 1_2.
            link(_static.Target.Bar, current)();
            // Should invoke the final declaration.
            link(_static.Target.Bar, final)();
        }

        [PseudoOverride(nameof(Bar), "TestAspect1")]
        [PseudoNotInlineable]
        static void Bar_Override1_2()
        {
            // Should invoke source code.
            link(_static.Target.Bar, @base)();
            // Should invoke override 1_1.
            link(_static.Target.Bar, previous)();
            // Should invoke override 1_2.
            link(_static.Target.Bar, current)();
            // Should invoke the final declaration.
            link(_static.Target.Bar, final)();
        }

        [PseudoOverride(nameof(Bar), "TestAspect3")]
        [PseudoNotInlineable]
        static void Bar_Override3_1()
        {
            // Should invoke override 1_2.
            link(_static.Target.Bar, @base)();
            // Should invoke override 1_2.
            link(_static.Target.Bar, previous)();
            // Should invoke override 3_2.
            link(_static.Target.Bar, current)();
            // Should invoke the final declaration.
            link(_static.Target.Bar, final)();
        }

        [PseudoOverride(nameof(Bar), "TestAspect3")]
        [PseudoNotInlineable]
        static void Bar_Override3_2()
        {
            // Should invoke override 1_2.
            link(_static.Target.Bar, @base)();
            // Should invoke override 3_1.
            link(_static.Target.Bar, previous)();
            // Should invoke override 3_2.
            link(_static.Target.Bar, current)();
            // Should invoke the final declaration.
            link(_static.Target.Bar, final)();
        }

        [PseudoOverride(nameof(Bar), "TestAspect5")]
        [PseudoNotInlineable]
        static void Bar_Override5_1()
        {
            // Should invoke override 3_2.
            link(_static.Target.Bar, @base)();
            // Should invoke override 3_2.
            link(_static.Target.Bar, previous)();
            // Should invoke the final declaration.
            link(_static.Target.Bar, current)();
            // Should invoke the final declaration.
            link(_static.Target.Bar, final)();
        }

        [PseudoOverride(nameof(Bar), "TestAspect5")]
        [PseudoNotInlineable]
        static void Bar_Override5_2()
        {
            // Should invoke override 3_2.
            link(_static.Target.Bar, @base)();
            // Should invoke override 5_1.
            link(_static.Target.Bar, previous)();
            // Should invoke the final declaration.
            link(_static.Target.Bar, current)();
            // Should invoke the final declaration.
            link(_static.Target.Bar, final)();
        }
    }
}
