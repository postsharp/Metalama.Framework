using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Events.Linking.SourceNew
{
    class Base
    {
        public virtual event System.EventHandler Bar
        {
            add
            {
            }
            remove
            {
            }
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
        public event System.EventHandler Foo
        {
            add
            {
                System.Console.WriteLine("This is original code (discarded).");
            }
            remove
            {
                System.Console.WriteLine("This is original code (discarded).");
            }
        }

        public new event System.EventHandler Bar
        {
            add
            {
                System.Console.WriteLine("This is original code.");
            }
            remove
            {
                System.Console.WriteLine("This is original code.");
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect0")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public event System.EventHandler Foo_Override0
        {
            add
            {
                // Should invoke source code.
                link[_this.Bar.add, @base] += value;
                // Should invoke source code.
                link[_this.Bar.add, previous] += value;
                // Should invoke source code.
                link[_this.Bar.add, current] += value;
                // Should invoke the final declaration.
                link[_this.Bar.add, final] += value;
            }
            remove
            {
                // Should invoke source code.
                link[_this.Bar.remove, @base] -= value;
                // Should invoke source code.
                link[_this.Bar.remove, previous] -= value;
                // Should invoke source code.
                link[_this.Bar.remove, current] -= value;
                // Should invoke the final declaration.
                link[_this.Bar.remove, final] -= value;
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect2")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public event System.EventHandler Foo_Override2
        {
            add
            {
                // Should invoke override 1_2.
                link[_this.Bar.add, @base] += value;
                // Should invoke override 1_2.
                link[_this.Bar.add, previous] += value;
                // Should invoke override 1_2.
                link[_this.Bar.add, current] += value;
                // Should invoke the final declaration.
                link[_this.Bar.add, final] += value;
            }
            remove
            {
                // Should invoke override 1_2.
                link[_this.Bar.remove, @base] -= value;
                // Should invoke override 1_2.
                link[_this.Bar.remove, previous] -= value;
                // Should invoke override 1_2.
                link[_this.Bar.remove, current] -= value;
                // Should invoke the final declaration.
                link[_this.Bar.remove, final] -= value;
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect4")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public event System.EventHandler Foo_Override4
        {
            add
            {
                // Should invoke override 3_2.
                link[_this.Bar.add, @base] += value;
                // Should invoke override 3_2.
                link[_this.Bar.add, previous] += value;
                // Should invoke override 3_2.
                link[_this.Bar.add, current] += value;
                // Should invoke the final declaration.
                link[_this.Bar.add, final] += value;
            }
            remove
            {
                // Should invoke override 3_2.
                link[_this.Bar.remove, @base] -= value;
                // Should invoke override 3_2.
                link[_this.Bar.remove, previous] -= value;
                // Should invoke override 3_2.
                link[_this.Bar.remove, current] -= value;
                // Should invoke the final declaration.
                link[_this.Bar.remove, final] -= value;
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect6")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public event System.EventHandler Foo_Override6
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

        [PseudoOverride(nameof(Bar), "TestAspect1")]
        [PseudoNotInlineable]
        private event System.EventHandler Bar_Override1_1
        {
            add
            {
                // Should invoke source code.
                link[_this.Bar.add, @base] += value;
                // Should invoke source code.
                link[_this.Bar.add, previous] += value;
                // Should invoke override 1_2.
                link[_this.Bar.add, current] += value;
                // Should invoke the final declaration.
                link[_this.Bar.add, final] += value;
            }
            remove
            {
                // Should invoke source code.
                link[_this.Bar.remove, @base] -= value;
                // Should invoke source code.
                link[_this.Bar.remove, previous] -= value;
                // Should invoke override 1_2.
                link[_this.Bar.remove, current] -= value;
                // Should invoke the final declaration.
                link[_this.Bar.remove, final] -= value;
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect1")]
        [PseudoNotInlineable]
        private event System.EventHandler Bar_Override1_2
        {
            add
            {
                // Should invoke source code.
                link[_this.Bar.add, @base] += value;
                // Should invoke override 1_1.
                link[_this.Bar.add, previous] += value;
                // Should invoke override 1_2.
                link[_this.Bar.add, current] += value;
                // Should invoke the final declaration.
                link[_this.Bar.add, final] += value;
            }
            remove
            {
                // Should invoke source code.
                link[_this.Bar.remove, @base] -= value;
                // Should invoke override 1_1.
                link[_this.Bar.remove, previous] -= value;
                // Should invoke override 1_2.
                link[_this.Bar.remove, current] -= value;
                // Should invoke the final declaration.
                link[_this.Bar.remove, final] -= value;
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect3")]
        [PseudoNotInlineable]
        private event System.EventHandler Bar_Override3_1
        {
            add
            {
                // Should invoke override 1_2.
                link[_this.Bar.add, @base] += value;
                // Should invoke override 1_2.
                link[_this.Bar.add, previous] += value;
                // Should invoke override 3_2.
                link[_this.Bar.add, current] += value;
                // Should invoke the final declaration.
                link[_this.Bar.add, final] += value;
            }
            remove
            {
                // Should invoke override 1_2.
                link[_this.Bar.remove, @base] -= value;
                // Should invoke override 1_2.
                link[_this.Bar.remove, previous] -= value;
                // Should invoke override 3_2.
                link[_this.Bar.remove, current] -= value;
                // Should invoke the final declaration.
                link[_this.Bar.remove, final] -= value;
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect3")]
        [PseudoNotInlineable]
        private event System.EventHandler Bar_Override3_2
        {
            add
            {
                // Should invoke override 1_2.
                link[_this.Bar.add, @base] += value;
                // Should invoke override 3_1.
                link[_this.Bar.add, previous] += value;
                // Should invoke override 3_2.
                link[_this.Bar.add, current] += value;
                // Should invoke the final declaration.
                link[_this.Bar.add, final] += value;
            }
            remove
            {
                // Should invoke override 1_2.
                link[_this.Bar.remove, @base] -= value;
                // Should invoke override 3_1.
                link[_this.Bar.remove, previous] -= value;
                // Should invoke override 3_2.
                link[_this.Bar.remove, current] -= value;
                // Should invoke the final declaration.
                link[_this.Bar.remove, final] -= value;
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect5")]
        [PseudoNotInlineable]
        private event System.EventHandler Bar_Override5_1
        {
            add
            {
                // Should invoke override 3_2.
                link[_this.Bar.add, @base] += value;
                // Should invoke override 3_2.
                link[_this.Bar.add, previous] += value;
                // Should invoke the final declaration.
                link[_this.Bar.add, current] += value;
                // Should invoke the final declaration.
                link[_this.Bar.add, final] += value;
            }
            remove
            {
                // Should invoke override 3_2.
                link[_this.Bar.remove, @base] -= value;
                // Should invoke override 3_2.
                link[_this.Bar.remove, previous] -= value;
                // Should invoke the final declaration.
                link[_this.Bar.remove, current] -= value;
                // Should invoke the final declaration.
                link[_this.Bar.remove, final] -= value;
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect5")]
        [PseudoNotInlineable]
        private event System.EventHandler Bar_Override5_2
        {
            add
            {
                // Should invoke override 3_2.
                link[_this.Bar.add, @base] += value;
                // Should invoke override 5_1.
                link[_this.Bar.add, previous] += value;
                // Should invoke the final declaration.
                link[_this.Bar.add, current] += value;
                // Should invoke the final declaration.
                link[_this.Bar.add, final] += value;
            }
            remove
            {
                // Should invoke override 3_2.
                link[_this.Bar.remove, @base] -= value;
                // Should invoke override 5_1.
                link[_this.Bar.remove, previous] -= value;
                // Should invoke the final declaration.
                link[_this.Bar.remove, current] -= value;
                // Should invoke the final declaration.
                link[_this.Bar.remove, final] -= value;
            }
        }
    }
}