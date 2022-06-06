using System;
using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Methods.Linking.IntroducedMethod
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
        public void Foo()
        {
            Console.WriteLine("This is original code.");
        }

        [PseudoOverride(nameof(Foo), "TestAspect0")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public void Foo_Override0()
        {
            // Should invoke empty code.
            link(_this.Bar, original)();
            // Should invoke empty code.
            link(_this.Bar, @base)();
            // Should invoke empty code.
            link(_this.Bar, self)();
            // Should invoke the final declaration.
            link(_this.Bar, final)();
        }

        [PseudoOverride(nameof(Foo), "TestAspect2")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public void Foo_Override2()
        {
            // Should invoke empty code.
            link(_this.Bar, original)();
            // Should invoke override 1.
            link(_this.Bar, @base)();
            // Should invoke override 1.
            link(_this.Bar, self)();
            // Should invoke the final declaration.
            link(_this.Bar, final)();
        }

        [PseudoOverride(nameof(Foo), "TestAspect4")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public void Foo_Override4()
        {
            // Should invoke empty code.
            link(_this.Bar, original)();
            // Should invoke override 3.
            link(_this.Bar, @base)();
            // Should invoke override 3.
            link(_this.Bar, self)();
            // Should invoke the final declaration.
            link(_this.Bar, final)();
        }

        [PseudoOverride(nameof(Foo), "TestAspect6")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public void Foo_Override6()
        {
            // Should invoke empty code.
            link(_this.Bar, original)();
            // Should invoke the final declaration.
            link(_this.Bar, @base)();
            // Should invoke the final declaration.
            link(_this.Bar, self)();
            // Should invoke the final declaration.
            link(_this.Bar, final)();
        }

        [PseudoIntroduction("TestAspect1")]
        [PseudoNotInlineable]
        public void Bar()
        {
            Console.WriteLine("This is introduced code (discarded).");
        }

        [PseudoOverride(nameof(Bar), "TestAspect1")]
        [PseudoNotInlineable]
        private void Bar_Override1()
        {
            // Should invoke empty code.
            link(_this.Bar, original)();
            // Should invoke empty code.
            link(_this.Bar, @base)();
            // Should invoke override 1.
            link(_this.Bar, self)();
            // Should invoke the final declaration.
            link(_this.Bar, final)();
        }

        [PseudoOverride(nameof(Bar), "TestAspect3")]
        [PseudoNotInlineable]
        private void Bar_Override3()
        {
            // Should invoke empty code.
            link(_this.Bar, original)();
            // Should invoke override 1.
            link(_this.Bar, @base)();
            // Should invoke override 3.
            link(_this.Bar, self)();
            // Should invoke the final declaration.
            link(_this.Bar, final)();
        }

        [PseudoOverride(nameof(Bar), "TestAspect5")]
        [PseudoNotInlineable]
        private void Bar_Override5()
        {
            // Should invoke empty code.
            link(_this.Bar, original)();
            // Should invoke override 3.
            link(_this.Bar, @base)();
            // Should invoke the final declaration.
            link(_this.Bar, self)();
            // Should invoke the final declaration.
            link(_this.Bar, final)();
        }
    }
}
