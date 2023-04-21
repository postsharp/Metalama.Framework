﻿using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Events.Linking.StaticIntroduced
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
        public static event System.EventHandler Foo
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

        [PseudoIntroduction("TestAspect1")]
        [PseudoNotInlineable]
        public static event System.EventHandler Bar
        {
            add
            {
                System.Console.WriteLine("SHOULD BE DISCARDED (this is introduced code).");
            }
            remove
            {
                System.Console.WriteLine("SHOULD BE DISCARDED (this is introduced code).");
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect0")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public static event System.EventHandler Foo_Override0
        {
            add
            {
                // Should invoke empty code.
                link[_static.Target.Bar.add, @base] += value;
                // Should invoke empty code.
                link[_static.Target.Bar.add, previous] += value;
                // Should invoke empty code.
                link[_static.Target.Bar.add, current] += value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.add, final] += value;
            }
            remove
            {
                // Should invoke empty code.
                link[_static.Target.Bar.remove, @base] -= value;
                // Should invoke empty code.
                link[_static.Target.Bar.remove, previous] -= value;
                // Should invoke empty code.
                link[_static.Target.Bar.remove, current] -= value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.remove, final] -= value;
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect2")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public static event System.EventHandler Foo_Override2
        {
            add
            {
                // Should invoke override 1_2.
                link[_static.Target.Bar.add, @base] += value;
                // Should invoke override 1_2.
                link[_static.Target.Bar.add, previous] += value;
                // Should invoke override 1_2.
                link[_static.Target.Bar.add, current] += value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.add, final] += value;
            }
            remove
            {
                // Should invoke override 1_2.
                link[_static.Target.Bar.remove, @base] -= value;
                // Should invoke override 1_2.
                link[_static.Target.Bar.remove, previous] -= value;
                // Should invoke override 1_2.
                link[_static.Target.Bar.remove, current] -= value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.remove, final] -= value;
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect4")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public static event System.EventHandler Foo_Override4
        {
            add
            {
                // Should invoke override 3_2.
                link[_static.Target.Bar.add, @base] += value;
                // Should invoke override 3_2.
                link[_static.Target.Bar.add, previous] += value;
                // Should invoke override 3_2.
                link[_static.Target.Bar.add, current] += value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.add, final] += value;
            }
            remove
            {
                // Should invoke override 3_2.
                link[_static.Target.Bar.remove, @base] -= value;
                // Should invoke override 3_2.
                link[_static.Target.Bar.remove, previous] -= value;
                // Should invoke override 3_2.
                link[_static.Target.Bar.remove, current] -= value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.remove, final] -= value;
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect6")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public static event System.EventHandler Foo_Override6
        {
            add
            {
                // Should invoke the final declaration.
                link[_static.Target.Bar.add, @base] += value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.add, previous] += value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.add, current] += value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.add, final] += value;
            }
            remove
            {
                // Should invoke the final declaration.
                link[_static.Target.Bar.remove, @base] -= value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.remove, previous] -= value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.remove, current] -= value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.remove, final] -= value;
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect1")]
        [PseudoNotInlineable]
        private static event System.EventHandler Bar_Override1_1
        {
            add
            {
                // Should invoke empty code.
                link[_static.Target.Bar.add, @base] += value;
                // Should invoke empty code.
                link[_static.Target.Bar.add, previous] += value;
                // Should invoke override 1_2.
                link[_static.Target.Bar.add, current] += value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.add, final] += value;
            }
            remove
            {
                // Should invoke empty code.
                link[_static.Target.Bar.remove, @base] -= value;
                // Should invoke empty code.
                link[_static.Target.Bar.remove, previous] -= value;
                // Should invoke override 1_2.
                link[_static.Target.Bar.remove, current] -= value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.remove, final] -= value;
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect1")]
        [PseudoNotInlineable]
        private static event System.EventHandler Bar_Override1_2
        {
            add
            {
                // Should invoke empty code.
                link[_static.Target.Bar.add, @base] += value;
                // Should invoke override 1_1.
                link[_static.Target.Bar.add, previous] += value;
                // Should invoke override 1_2.
                link[_static.Target.Bar.add, current] += value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.add, final] += value;
            }
            remove
            {
                // Should invoke empty code.
                link[_static.Target.Bar.remove, @base] -= value;
                // Should invoke override 1_1.
                link[_static.Target.Bar.remove, previous] -= value;
                // Should invoke override 1_2.
                link[_static.Target.Bar.remove, current] -= value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.remove, final] -= value;
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect3")]
        [PseudoNotInlineable]
        private static event System.EventHandler Bar_Override3_1
        {
            add
            {
                // Should invoke override 1_2.
                link[_static.Target.Bar.add, @base] += value;
                // Should invoke override 1_2.
                link[_static.Target.Bar.add, previous] += value;
                // Should invoke override 3_2.
                link[_static.Target.Bar.add, current] += value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.add, final] += value;
            }
            remove
            {
                // Should invoke override 1_2.
                link[_static.Target.Bar.remove, @base] -= value;
                // Should invoke override 1_2.
                link[_static.Target.Bar.remove, previous] -= value;
                // Should invoke override 3_2.
                link[_static.Target.Bar.remove, current] -= value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.remove, final] -= value;
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect3")]
        [PseudoNotInlineable]
        private static event System.EventHandler Bar_Override3_2
        {
            add
            {
                // Should invoke override 1_2.
                link[_static.Target.Bar.add, @base] += value;
                // Should invoke override 3_1.
                link[_static.Target.Bar.add, previous] += value;
                // Should invoke override 3_2.
                link[_static.Target.Bar.add, current] += value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.add, final] += value;
            }
            remove
            {
                // Should invoke override 1_2.
                link[_static.Target.Bar.remove, @base] -= value;
                // Should invoke override 3_1.
                link[_static.Target.Bar.remove, previous] -= value;
                // Should invoke override 3_2.
                link[_static.Target.Bar.remove, current] -= value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.remove, final] -= value;
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect5")]
        [PseudoNotInlineable]
        private static event System.EventHandler Bar_Override5_1
        {
            add
            {
                // Should invoke override 3_2.
                link[_static.Target.Bar.add, @base] += value;
                // Should invoke override 3_2.
                link[_static.Target.Bar.add, previous] += value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.add, current] += value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.add, final] += value;
            }
            remove
            {
                // Should invoke override 3_2.
                link[_static.Target.Bar.remove, @base] -= value;
                // Should invoke override 3_2.
                link[_static.Target.Bar.remove, previous] -= value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.remove, current] -= value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.remove, final] -= value;
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect5")]
        [PseudoNotInlineable]
        private static event System.EventHandler Bar_Override5_2
        {
            add
            {
                // Should invoke override 3_2.
                link[_static.Target.Bar.add, @base] += value;
                // Should invoke override 5_1.
                link[_static.Target.Bar.add, previous] += value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.add, current] += value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.add, final] += value;
            }
            remove
            {
                // Should invoke override 3_2.
                link[_static.Target.Bar.remove, @base] -= value;
                // Should invoke override 5_1.
                link[_static.Target.Bar.remove, previous] -= value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.remove, current] -= value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.remove, final] -= value;
            }
        }
    }
}