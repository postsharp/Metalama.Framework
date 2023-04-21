using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Properties.Linking.StaticSource
{
    [PseudoLayerOrder("TestAspect0")]
    [PseudoLayerOrder("TestAspect1")]
    [PseudoLayerOrder("TestAspect2")]
    [PseudoLayerOrder("TestAspect3")]
    [PseudoLayerOrder("TestAspect4")]
    [PseudoLayerOrder("TestAspect5")]
    [PseudoLayerOrder("TestAspect6")]
    // <target>
    class Target
    {
        public static int Foo
        {
            get
            {
                System.Console.WriteLine("This is original code (discarded).");

                return 42;
            }
            set
            {
                System.Console.WriteLine("This is original code (discarded).");
            }
        }

        private static int Bar
        {
            get
            {
                System.Console.WriteLine("This is original code (discarded).");

                return 42;
            }
            set
            {
                System.Console.WriteLine("This is original code (discarded).");
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect0")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public static int Foo_Override0
        {
            get
            {
                // Should invoke source code.
                _ = link(_static.Target.Bar.get, @base);
                // Should invoke source code.
                _ = link(_static.Target.Bar.get, previous);
                // Should invoke source code.
                _ = link(_static.Target.Bar.get, current);
                // Should invoke the final declaration.
                _ = link(_static.Target.Bar.get, final);
                return 42;
            }
            set
            {
                // Should invoke source code.
                link[_static.Target.Bar.set, @base] = value;
                // Should invoke source code.
                link[_static.Target.Bar.set, previous] = value;
                // Should invoke source code.
                link[_static.Target.Bar.set, current] = value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.set, final] = value;
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect2")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public static int Foo_Override2
        {
            get
            {
                // Should invoke override 1_2.
                _ = link(_static.Target.Bar.get, @base);
                // Should invoke override 1_2.
                _ = link(_static.Target.Bar.get, previous);
                // Should invoke override 1_2.
                _ = link(_static.Target.Bar.get, current);
                // Should invoke the final declaration.
                _ = link(_static.Target.Bar.get, final);
                return 42;
            }
            set
            {
                // Should invoke override 1_2.
                link[_static.Target.Bar.set, @base] = value;
                // Should invoke override 1_2.
                link[_static.Target.Bar.set, previous] = value;
                // Should invoke override 1_2.
                link[_static.Target.Bar.set, current] = value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.set, final] = value;
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect4")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public static int Foo_Override4
        {
            get
            {
                // Should invoke override 3_2.
                _ = link(_static.Target.Bar.get, @base);
                // Should invoke override 3_2.
                _ = link(_static.Target.Bar.get, previous);
                // Should invoke override 3_2.
                _ = link(_static.Target.Bar.get, current);
                // Should invoke the final declaration.
                _ = link(_static.Target.Bar.get, final);
                return 42;
            }
            set
            {
                // Should invoke override 3_2.
                link[_static.Target.Bar.set, @base] = value;
                // Should invoke override 3_2.
                link[_static.Target.Bar.set, previous] = value;
                // Should invoke override 3_2.
                link[_static.Target.Bar.set, current] = value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.set, final] = value;
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect6")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public static int Foo_Override6
        {
            get
            {
                // Should invoke the final declaration.
                _ = link(_static.Target.Bar.get, @base);
                // Should invoke the final declaration.
                _ = link(_static.Target.Bar.get, previous);
                // Should invoke the final declaration.
                _ = link(_static.Target.Bar.get, current);
                // Should invoke the final declaration.
                _ = link(_static.Target.Bar.get, final);
                return 42;
            }
            set
            {
                // Should invoke the final declaration.
                link[_static.Target.Bar.set, @base] = value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.set, previous] = value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.set, current] = value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.set, final] = value;
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect1")]
        [PseudoNotInlineable]
        private static int Bar_Override1_1
        {
            get
            {
                // Should invoke source code.
                _ = link(_static.Target.Bar.get, @base);
                // Should invoke source code.
                _ = link(_static.Target.Bar.get, previous);
                // Should invoke override 1_2.
                _ = link(_static.Target.Bar.get, current);
                // Should invoke the final declaration.
                _ = link(_static.Target.Bar.get, final);
                return 42;
            }
            set
            {
                // Should invoke source code.
                link[_static.Target.Bar.set, @base] = value;
                // Should invoke source code.
                link[_static.Target.Bar.set, previous] = value;
                // Should invoke override 1_2.
                link[_static.Target.Bar.set, current] = value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.set, final] = value;
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect1")]
        [PseudoNotInlineable]
        private static int Bar_Override1_2
        {
            get
            {
                // Should invoke source code.
                _ = link(_static.Target.Bar.get, @base);
                // Should invoke override 1_1.
                _ = link(_static.Target.Bar.get, previous);
                // Should invoke override 1_2.
                _ = link(_static.Target.Bar.get, current);
                // Should invoke the final declaration.
                _ = link(_static.Target.Bar.get, final);
                return 42;
            }
            set
            {
                // Should invoke source code.
                link[_static.Target.Bar.set, @base] = value;
                // Should invoke override 1_1.
                link[_static.Target.Bar.set, previous] = value;
                // Should invoke override 1_2.
                link[_static.Target.Bar.set, current] = value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.set, final] = value;
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect3")]
        [PseudoNotInlineable]
        private static int Bar_Override3_1
        {
            get
            {
                // Should invoke override 1_2.
                _ = link(_static.Target.Bar.get, @base);
                // Should invoke override 1_2.
                _ = link(_static.Target.Bar.get, previous);
                // Should invoke override 3_2.
                _ = link(_static.Target.Bar.get, current);
                // Should invoke the final declaration.
                _ = link(_static.Target.Bar.get, final);
                return 42;
            }
            set
            {
                // Should invoke override 1_2.
                link[_static.Target.Bar.set, @base] = value;
                // Should invoke override 1_2.
                link[_static.Target.Bar.set, previous] = value;
                // Should invoke override 3_2.
                link[_static.Target.Bar.set, current] = value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.set, final] = value;
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect3")]
        [PseudoNotInlineable]
        private static int Bar_Override3_2
        {
            get
            {
                // Should invoke override 1_2.
                _ = link(_static.Target.Bar.get, @base);
                // Should invoke override 3_1.
                _ = link(_static.Target.Bar.get, previous);
                // Should invoke override 3_2.
                _ = link(_static.Target.Bar.get, current);
                // Should invoke the final declaration.
                _ = link(_static.Target.Bar.get, final);
                return 42;
            }
            set
            {
                // Should invoke override 1_2.
                link[_static.Target.Bar.set, @base] = value;
                // Should invoke override 3_1.
                link[_static.Target.Bar.set, previous] = value;
                // Should invoke override 3_2.
                link[_static.Target.Bar.set, current] = value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.set, final] = value;
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect5")]
        [PseudoNotInlineable]
        private static int Bar_Override5_1
        {
            get
            {
                // Should invoke override 3_2.
                _ = link(_static.Target.Bar.get, @base);
                // Should invoke override 3_2.
                _ = link(_static.Target.Bar.get, previous);
                // Should invoke the final declaration.
                _ = link(_static.Target.Bar.get, current);
                // Should invoke the final declaration.
                _ = link(_static.Target.Bar.get, final);
                return 42;
            }
            set
            {
                // Should invoke override 3_2.
                link[_static.Target.Bar.set, @base] = value;
                // Should invoke override 3_2.
                link[_static.Target.Bar.set, previous] = value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.set, current] = value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.set, final] = value;
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect5")]
        [PseudoNotInlineable]
        private static int Bar_Override5_2
        {
            get
            {
                // Should invoke override 3_2.
                _ = link(_static.Target.Bar.get, @base);
                // Should invoke override 5_1.
                _ = link(_static.Target.Bar.get, previous);
                // Should invoke the final declaration.
                _ = link(_static.Target.Bar.get, current);
                // Should invoke the final declaration.
                _ = link(_static.Target.Bar.get, final);
                return 42;
            }
            set
            {
                // Should invoke override 3_2.
                link[_static.Target.Bar.set, @base] = value;
                // Should invoke override 5_1.
                link[_static.Target.Bar.set, previous] = value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.set, current] = value;
                // Should invoke the final declaration.
                link[_static.Target.Bar.set, final] = value;
            }
        }
    }
}