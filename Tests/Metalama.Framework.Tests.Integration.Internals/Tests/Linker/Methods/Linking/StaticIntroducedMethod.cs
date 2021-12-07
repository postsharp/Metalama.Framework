using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.Methods.Linking.StaticIntroducedMethod
{
    [PseudoLayerOrder("TestAspect0")]
    [PseudoLayerOrder("TestAspect1")]
    [PseudoLayerOrder("TestAspect2")]
    [PseudoLayerOrder("TestAspect3")]
    [PseudoLayerOrder("TestAspect4")]
    [PseudoLayerOrder("TestAspect5")]
    // <target>
    class Target
    {
        public static void Foo()
        {
            Console.WriteLine("This is original code.");
        }

        [PseudoOverride(nameof(Foo), "TestAspect0")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public static void Foo_Override0()
        {
            // Should invoke empty code.
            link(_static.Target.Bar, original)();
            // Should invoke empty code.
            link(_static.Target.Bar, @base)();
            // Should invoke empty code.
            link(_static.Target.Bar, self)();
            // Should invoke the final declaration.
            link(_static.Target.Bar, final)();
        }

        [PseudoOverride(nameof(Foo), "TestAspect2")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public static void Foo_Override2()
        {
            // Should invoke empty code.
            link(_static.Target.Bar, original)();
            // Should invoke override 1.
            link(_static.Target.Bar, @base)();
            // Should invoke override 1.
            link(_static.Target.Bar, self)();
            // Should invoke the final declaration.
            link(_static.Target.Bar, final)();
        }

        [PseudoOverride(nameof(Foo), "TestAspect4")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public static void Foo_Override4()
        {
            // Should invoke empty code.
            link(_static.Target.Bar, original)();
            // Should invoke override 3.
            link(_static.Target.Bar, @base)();
            // Should invoke override 3.
            link(_static.Target.Bar, self)();
            // Should invoke the final declaration.
            link(_static.Target.Bar, final)();
        }

        [PseudoOverride(nameof(Foo), "TestAspect6")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public static void Foo_Override6()
        {
            // Should invoke empty code.
            link(_static.Target.Bar, original)();
            // Should invoke the final declaration.
            link(_static.Target.Bar, @base)();
            // Should invoke the final declaration.
            link(_static.Target.Bar, self)();
            // Should invoke the final declaration.
            link(_static.Target.Bar, final)();
        }

        [PseudoIntroduction("TestAspect1")]
        [PseudoNotInlineable]
        public static void Bar()
        {
            Console.WriteLine("This is introduced code (discarded).");
        }

        [PseudoOverride(nameof(Bar), "TestAspect1")]
        [PseudoNotInlineable]
        private static void Bar_Override1()
        {
            // Should invoke empty code.
            link(_static.Target.Bar, original)();
            // Should invoke empty code.
            link(_static.Target.Bar, @base)();
            // Should invoke override 1.
            link(_static.Target.Bar, self)();
            // Should invoke the final declaration.
            link(_static.Target.Bar, final)();
        }

        [PseudoOverride(nameof(Bar), "TestAspect3")]
        [PseudoNotInlineable]
        private static void Bar_Override3()
        {
            // Should invoke empty code.
            link(_static.Target.Bar, original)();
            // Should invoke override 1.
            link(_static.Target.Bar, @base)();
            // Should invoke override 3.
            link(_static.Target.Bar, self)();
            // Should invoke the final declaration.
            link(_static.Target.Bar, final)();
        }

        [PseudoOverride(nameof(Bar), "TestAspect5")]
        [PseudoNotInlineable]
        private static void Bar_Override5()
        {
            // Should invoke empty code.
            link(_static.Target.Bar, original)();
            // Should invoke override 3.
            link(_static.Target.Bar, @base)();
            // Should invoke the final declaration.
            link(_static.Target.Bar, self)();
            // Should invoke the final declaration.
            link(_static.Target.Bar, final)();
        }
    }
}
