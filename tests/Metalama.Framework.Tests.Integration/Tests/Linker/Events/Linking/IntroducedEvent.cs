using System;
using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Events.Linking.IntroducedEvent
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
                link[_this.Bar.add, @base] += value;
                // Should invoke empty code.
                link[_this.Bar.add, previous] += value;
                // Should invoke empty code.
                link[_this.Bar.add, current] += value;
                // Should invoke the final declaration.
                link[_this.Bar.add, final] += value;
            }

            remove
            {
                // Should invoke empty code.
                link[_this.Bar.remove, @base] -= value;
                // Should invoke empty code.
                link[_this.Bar.remove, previous] -= value;
                // Should invoke empty code.
                link[_this.Bar.remove, current] -= value;
                // Should invoke the final declaration.
                link[_this.Bar.remove, final] -= value;
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect2")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public event EventHandler Foo_Override2
        {
            add
            {
                // Should invoke override 1.
                link[_this.Bar.add, @base] += value;
                // Should invoke override 1.
                link[_this.Bar.add, previous] += value;
                // Should invoke override 1.
                link[_this.Bar.add, current] += value;
                // Should invoke the final declaration.
                link[_this.Bar.add, final] += value;
            }

            remove
            {
                // Should invoke override 1.
                link[_this.Bar.remove, @base] -= value;
                // Should invoke override 1.
                link[_this.Bar.remove, previous] -= value;
                // Should invoke override 1.
                link[_this.Bar.remove, current] -= value;
                // Should invoke the final declaration.
                link[_this.Bar.remove, final] -= value;
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect4")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public event EventHandler Foo_Override4
        {
            add
            {
                // Should invoke override 3.
                link[_this.Bar.add, @base] += value;
                // Should invoke override 3.
                link[_this.Bar.add, previous] += value;
                // Should invoke override 3.
                link[_this.Bar.add, current] += value;
                // Should invoke the final declaration.
                link[_this.Bar.add, final] += value;
            }

            remove
            {
                // Should invoke override 3.
                link[_this.Bar.remove, @base] -= value;
                // Should invoke override 3.
                link[_this.Bar.remove, previous] -= value;
                // Should invoke override 3.
                link[_this.Bar.remove, current] -= value;
                // Should invoke the final declaration.
                link[_this.Bar.remove, final] -= value;
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect6")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public event EventHandler Foo_Override6
        {
            add
            {
                // Should invoke the final declaration.
                link[_this.Bar.add, @base] += value;
                // Should invoke the final declaration.
                link[_this.Bar.add, previous] += value;
                // Should invoke the final declaration.
                link[_this.Bar.add, current] += value;
                // Should invoke the final declaration.
                link[_this.Bar.add, final] += value;
            }

            remove
            {
                // Should invoke the final declaration.
                link[_this.Bar.remove, @base] -= value;
                // Should invoke the final declaration.
                link[_this.Bar.remove, previous] -= value;
                // Should invoke the final declaration.
                link[_this.Bar.remove, current] -= value;
                // Should invoke the final declaration.
                link[_this.Bar.remove, final] -= value;
            }
        }

        [PseudoIntroduction("TestAspect1")]
        [PseudoNotInlineable]
        public event EventHandler Bar
        {
            add
            {
                Console.WriteLine("This is introduced code (discarded).");
            }

            remove
            {
                Console.WriteLine("This is introduced code (discarded).");
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect1")]
        [PseudoNotInlineable]
        private event EventHandler Bar_Override1
        {
            add
            {
                // Should invoke empty code.
                link[_this.Bar.add, @base] += value;
                // Should invoke empty code.
                link[_this.Bar.add, previous] += value;
                // Should invoke override 1.
                link[_this.Bar.add, current] += value;
                // Should invoke the final declaration.
                link[_this.Bar.add, final] += value;
            }

            remove
            {
                // Should invoke empty code.
                link[_this.Bar.remove, @base] -= value;
                // Should invoke empty code.
                link[_this.Bar.remove, previous] -= value;
                // Should invoke override 1.
                link[_this.Bar.remove, current] -= value;
                // Should invoke the final declaration.
                link[_this.Bar.remove, final] -= value;
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect3")]
        [PseudoNotInlineable]
        private event EventHandler Bar_Override3
        {
            add
            {
                // Should invoke override 1.
                link[_this.Bar.add, @base] += value;
                // Should invoke override 1.
                link[_this.Bar.add, previous] += value;
                // Should invoke override 3.
                link[_this.Bar.add, current] += value;
                // Should invoke the final declaration.
                link[_this.Bar.add, final] += value;
            }

            remove
            {
                // Should invoke override 1.
                link[_this.Bar.remove, @base] -= value;
                // Should invoke override 1.
                link[_this.Bar.remove, previous] -= value;
                // Should invoke override 3.
                link[_this.Bar.remove, current] -= value;
                // Should invoke the final declaration.
                link[_this.Bar.remove, final] -= value;
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect5")]
        [PseudoNotInlineable]
        private event EventHandler Bar_Override5
        {
            add
            {
                // Should invoke override 3.
                link[_this.Bar.add, @base] += value;
                // Should invoke override 3.
                link[_this.Bar.add, previous] += value;
                // Should invoke the final declaration.
                link[_this.Bar.add, current] += value;
                // Should invoke the final declaration.
                link[_this.Bar.add, final] += value;
            }

            remove
            {
                // Should invoke override 3.
                link[_this.Bar.remove, @base] -= value;
                // Should invoke override 3.
                link[_this.Bar.remove, previous] -= value;
                // Should invoke the final declaration.
                link[_this.Bar.remove, current] -= value;
                // Should invoke the final declaration.
                link[_this.Bar.remove, final] -= value;
            }
        }
    }
}
