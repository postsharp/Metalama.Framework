using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.EventFields.Linking.IntroducedEvent
{
    [PseudoLayerOrder("TestAspect0")]
    [PseudoLayerOrder("TestAspect1")]
    [PseudoLayerOrder("TestAspect2")]
    [PseudoLayerOrder("TestAspect3")]
    [PseudoLayerOrder("TestAspect4")]
    [PseudoLayerOrder("TestAspect5")]
    [PseudoLayerOrder("TestAspect6")]
    [PseudoLayerOrder("TestAspect7")]
    // <target>
    class Target
    {
        public event EventHandler Foo
        {
            add => Console.WriteLine("This is original code.");
            remove => Console.WriteLine("This is original code.");
        }

        [PseudoOverride(nameof(Foo), "TestAspect0")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public event EventHandler Foo_Override0
        {
            add
            {
                // Should invoke empty code.
                link[_this.Bar, original] += value;
                // Should invoke empty code.
                link[_this.Bar, @base] += value;
                // Should invoke empty code.
                link[_this.Bar, self] += value;
                // Should invoke the final declaration.
                link[_this.Bar, final] += value;
            }

            remove
            {
                // Should invoke empty code.
                link[_this.Bar, original] -= value;
                // Should invoke empty code.
                link[_this.Bar, @base] -= value;
                // Should invoke empty code.
                link[_this.Bar, self] -= value;
                // Should invoke the final declaration.
                link[_this.Bar, final] -= value;
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect3")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public event EventHandler Foo_Override3
        {
            add
            {
                // Should invoke empty code.
                link[_this.Bar, original] += value;
                // Should invoke override 2.
                link[_this.Bar, @base] += value;
                // Should invoke override 2.
                link[_this.Bar, self] += value;
                // Should invoke the final declaration.
                link[_this.Bar, final] += value;
            }

            remove
            {
                // Should invoke empty code.
                link[_this.Bar, original] -= value;
                // Should invoke override 2.
                link[_this.Bar, @base] -= value;
                // Should invoke override 2.
                link[_this.Bar, self] -= value;
                // Should invoke the final declaration.
                link[_this.Bar, final] -= value;
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect5")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public event EventHandler Foo_Override5
        {
            add
            {
                // Should invoke empty code.
                link[_this.Bar, original] += value;
                // Should invoke override 4.
                link[_this.Bar, @base] += value;
                // Should invoke override 4.
                link[_this.Bar, self] += value;
                // Should invoke the final declaration.
                link[_this.Bar, final] += value;
            }

            remove
            {
                // Should invoke empty code.
                link[_this.Bar, original] -= value;
                // Should invoke override 4.
                link[_this.Bar, @base] -= value;
                // Should invoke override 4.
                link[_this.Bar, self] -= value;
                // Should invoke the final declaration.
                link[_this.Bar, final] -= value;
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect7")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public event EventHandler Foo_Override7
        {
            add
            {
                // Should invoke empty code.
                link[_this.Bar, original] += value;
                // Should invoke the final declaration.
                link[_this.Bar, @base] += value;
                // Should invoke the final declaration.
                link[_this.Bar, self] += value;
                // Should invoke the final declaration.
                link[_this.Bar, final] += value;
            }

            remove
            {
                // Should invoke empty code.
                link[_this.Bar, original] -= value;
                // Should invoke the final declaration.
                link[_this.Bar, @base] -= value;
                // Should invoke the final declaration.
                link[_this.Bar, self] -= value;
                // Should invoke the final declaration.
                link[_this.Bar, final] -= value;
            }
        }

        [PseudoIntroduction("TestAspect1")]
        [PseudoNotInlineable]
        public event EventHandler? Bar;

        [PseudoOverride(nameof(Bar), "TestAspect2")]
        [PseudoNotInlineable]
        private event EventHandler? Bar_Override2
        {
            add
            {
                // Should invoke empty code.
                link[_this.Bar, original] += value;
                // Should invoke source code.
                link[_this.Bar, @base] += value;
                // Should invoke override 2.
                link[_this.Bar, self] += value;
                // Should invoke the final declaration.
                link[_this.Bar, final] += value;
            }

            remove
            {
                // Should invoke empty code.
                link[_this.Bar, original] -= value;
                // Should invoke introduced event field.
                link[_this.Bar, @base] -= value;
                // Should invoke override 2.
                link[_this.Bar, self] -= value;
                // Should invoke the final declaration.
                link[_this.Bar, final] -= value;
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect4")]
        [PseudoNotInlineable]
        private event EventHandler? Bar_Override4
        {
            add
            {
                // Should invoke empty code.
                link[_this.Bar, original] += value;
                // Should invoke override 2.
                link[_this.Bar, @base] += value;
                // Should invoke override 4.
                link[_this.Bar, self] += value;
                // Should invoke the final declaration.
                link[_this.Bar, final] += value;
            }

            remove
            {
                // Should invoke empty code.
                link[_this.Bar, original] -= value;
                // Should invoke override 2.
                link[_this.Bar, @base] -= value;
                // Should invoke override 4.
                link[_this.Bar, self] -= value;
                // Should invoke the final declaration.
                link[_this.Bar, final] -= value;
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect6")]
        [PseudoNotInlineable]
        private event EventHandler? Bar_Override6
        {
            add
            {
                // Should invoke empty code.
                link[_this.Bar, original] += value;
                // Should invoke override 4.
                link[_this.Bar, @base] += value;
                // Should invoke the final declaration.
                link[_this.Bar, self] += value;
                // Should invoke the final declaration.
                link[_this.Bar, final] += value;
            }

            remove
            {
                // Should invoke empty code.
                link[_this.Bar, original] -= value;
                // Should invoke override 4.
                link[_this.Bar, @base] -= value;
                // Should invoke the final declaration.
                link[_this.Bar, self] -= value;
                // Should invoke the final declaration.
                link[_this.Bar, final] -= value;
            }
        }
    }
}
