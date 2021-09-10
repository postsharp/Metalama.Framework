using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.Properties.Linking.IntroducedEvent
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
        public int Foo
        {
            get
            {
                Console.WriteLine("This is original code.");
                return 0;
            }

            set
            {
                Console.WriteLine("This is original code.");
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect0")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public int Foo_Override0
        {
            get
            {
                // Should invoke empty code.
                _ = link[_this.Bar, original];
                // Should invoke empty code.
                _ = link[_this.Bar, @base];
                // Should invoke empty code.
                _ = link[_this.Bar, self];
                // Should invoke the final declaration.
                _ = link[_this.Bar, final];

                return 42;
            }

            set
            {
                // Should invoke empty code.
                link[_this.Bar, original] = value;
                // Should invoke empty code.
                link[_this.Bar, @base] = value;
                // Should invoke empty code.
                link[_this.Bar, self] = value;
                // Should invoke the final declaration.
                link[_this.Bar, final] = value;
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect2")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public int Foo_Override2
        {
            get
            {
                // Should invoke empty code.
                _ = link[_this.Bar, original];
                // Should invoke override 1.
                _ = link[_this.Bar, @base];
                // Should invoke override 1.
                _ = link[_this.Bar, self];
                // Should invoke the final declaration.
                _ = link[_this.Bar, final];

                return 42;
            }

            set
            {
                // Should invoke empty code.
                link[_this.Bar, original] = value;
                // Should invoke override 1.
                link[_this.Bar, @base] = value;
                // Should invoke override 1.
                link[_this.Bar, self] = value;
                // Should invoke the final declaration.
                link[_this.Bar, final] = value;
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect4")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public int Foo_Override4
        {
            get
            {
                // Should invoke empty code.
                _ = link[_this.Bar, original];
                // Should invoke override 3.
                _ = link[_this.Bar, @base];
                // Should invoke override 3.
                _ = link[_this.Bar, self];
                // Should invoke the final declaration.
                _ = link[_this.Bar, final];

                return 42;
            }

            set
            {
                // Should invoke empty code.
                link[_this.Bar, original] = value;
                // Should invoke override 3.
                link[_this.Bar, @base] = value;
                // Should invoke override 3.
                link[_this.Bar, self] = value;
                // Should invoke the final declaration.
                link[_this.Bar, final] = value;
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect6")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public int Foo_Override6
        {
            get
            {
                // Should invoke empty code.
                _ = link[_this.Bar, original];
                // Should invoke the final declaration.
                _ = link[_this.Bar, @base];
                // Should invoke the final declaration.
                _ = link[_this.Bar, self];
                // Should invoke the final declaration.
                _ = link[_this.Bar, final];

                return 42;
            }

            set
            {
                // Should invoke empty code.
                link[_this.Bar, original] = value;
                // Should invoke the final declaration.
                link[_this.Bar, @base] = value;
                // Should invoke the final declaration.
                link[_this.Bar, self] = value;
                // Should invoke the final declaration.
                link[_this.Bar, final] = value;
            }
        }

        [PseudoIntroduction("TestAspect1")]
        [PseudoNotInlineable]
        public int Bar
        {
            get
            {
                Console.WriteLine("This is introduced code (discarded).");
                return 0;
            }

            set
            {
                Console.WriteLine("This is introduced code (discarded).");
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect1")]
        [PseudoNotInlineable]
        private int Bar_Override1
        {
            get
            {
                // Should invoke empty code.
                _ = link[_this.Bar, original];
                // Should invoke empty code.
                _ = link[_this.Bar, @base];
                // Should invoke override 1.
                _ = link[_this.Bar, self];
                // Should invoke the final declaration.
                _ = link[_this.Bar, final];

                return 42;
            }

            set
            {
                // Should invoke empty code.
                link[_this.Bar, original] = value;
                // Should invoke empty code.
                link[_this.Bar, @base] = value;
                // Should invoke override 1.
                link[_this.Bar, self] = value;
                // Should invoke the final declaration.
                link[_this.Bar, final] = value;
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect3")]
        [PseudoNotInlineable]
        private int Bar_Override3
        {
            get
            {
                // Should invoke empty code.
                _ = link[_this.Bar, original];
                // Should invoke override 1.
                _ = link[_this.Bar, @base];
                // Should invoke override 3.
                _ = link[_this.Bar, self];
                // Should invoke the final declaration.
                _ = link[_this.Bar, final];

                return 42;
            }

            set
            {
                // Should invoke empty code.
                link[_this.Bar, original] = value;
                // Should invoke override 1.
                link[_this.Bar, @base] = value;
                // Should invoke override 3.
                link[_this.Bar, self] = value;
                // Should invoke the final declaration.
                link[_this.Bar, final] = value;
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect5")]
        [PseudoNotInlineable]
        private int Bar_Override5
        {
            get
            {
                // Should invoke empty code.
                _ = link[_this.Bar, original];
                // Should invoke override 3.
                _ = link[_this.Bar, @base];
                // Should invoke the final declaration.
                _ = link[_this.Bar, self];
                // Should invoke the final declaration.
                _ = link[_this.Bar, final];

                return 42;
            }

            set
            {
                // Should invoke empty code.
                link[_this.Bar, original] = value;
                // Should invoke override 3.
                link[_this.Bar, @base] = value;
                // Should invoke the final declaration.
                link[_this.Bar, self] = value;
                // Should invoke the final declaration.
                link[_this.Bar, final] = value;
            }
        }
    }
}
