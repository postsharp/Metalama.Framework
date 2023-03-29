using System;
using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Properties.Linking.OverridenBaseEvent
{
    class Base
    {
        public virtual int Bar
        {
            get 
            {
                return 0;
            }
            set { }
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
                // Should invoke base declaration.
                _ = link[_this.Bar.get, @base];
                // Should invoke base declaration.
                _ = link[_this.Bar.get, previous];
                // Should invoke base declaration.
                _ = link[_this.Bar.get, current];
                // Should invoke the final declaration.
                _ = link[_this.Bar.get, final];

                return 42;
            }

            set
            {
                // Should invoke base declaration.
                link[_this.Bar.set, @base] = value;
                // Should invoke base declaration.
                link[_this.Bar.set, previous] = value;
                // Should invoke base declaration.
                link[_this.Bar.set, current] = value;
                // Should invoke the final declaration.
                link[_this.Bar.set, final] = value;
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect2")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public int Foo_Override2
        {
            get
            {
                // Should invoke override 1.
                _ = link[_this.Bar.get, @base];
                // Should invoke override 1.
                _ = link[_this.Bar.get, previous];
                // Should invoke override 1.
                _ = link[_this.Bar.get, current];
                // Should invoke the final declaration.
                _ = link[_this.Bar.get, final];

                return 42;
            }

            set
            {
                // Should invoke override 1.
                link[_this.Bar.set, @base] = value;
                // Should invoke override 1.
                link[_this.Bar.set, previous] = value;
                // Should invoke override 1.
                link[_this.Bar.set, current] = value;
                // Should invoke the final declaration.
                link[_this.Bar.set, final] = value;
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect4")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public int Foo_Override4
        {
            get
            {
                // Should invoke override 3.
                _ = link[_this.Bar.get, @base];
                // Should invoke override 3.
                _ = link[_this.Bar.get, previous];
                // Should invoke override 3.
                _ = link[_this.Bar.get, current];
                // Should invoke the final declaration.
                _ = link[_this.Bar.get, final];

                return 42;
            }

            set
            {
                // Should invoke override 3.
                link[_this.Bar.set, @base] = value;
                // Should invoke override 3.
                link[_this.Bar.set, previous] = value;
                // Should invoke override 3.
                link[_this.Bar.set, current] = value;
                // Should invoke the final declaration.
                link[_this.Bar.set, final] = value;
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect6")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public int Foo_Override6
        {
            get
            {
                // Should invoke the final declaration.
                _ = link[_this.Bar.get, @base];
                // Should invoke the final declaration.
                _ = link[_this.Bar.get, previous];
                // Should invoke the final declaration.
                _ = link[_this.Bar.get, current];
                // Should invoke the final declaration.
                _ = link[_this.Bar.get, final];

                return 42;
            }

            set
            {
                // Should invoke the final declaration.
                link[_this.Bar.set, @base] = value;
                // Should invoke the final declaration.
                link[_this.Bar.set, previous] = value;
                // Should invoke the final declaration.
                link[_this.Bar.set, current] = value;
                // Should invoke the final declaration.
                link[_this.Bar.set, final] = value;
            }
        }

        [PseudoIntroduction("TestAspect1")]
        [PseudoNotInlineable]
        public override int Bar
        {
            get
            {
                return 0;
            }

            set
            {
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect1")]
        [PseudoNotInlineable]
        private int Bar_Override1
        {
            get
            {
                // Should invoke base declaration.
                _ = link[_this.Bar.get, @base];
                // Should invoke base declaration.
                _ = link[_this.Bar.get, previous];
                // Should invoke override 1.
                _ = link[_this.Bar.get, current];
                // Should invoke the final declaration.
                _ = link[_this.Bar.get, final];

                return 42;
            }

            set
            {
                // Should invoke base declaration.
                link[_this.Bar.set, @base] = value;
                // Should invoke base declaration.
                link[_this.Bar.set, previous] = value;
                // Should invoke override 1.
                link[_this.Bar.set, current] = value;
                // Should invoke the final declaration.
                link[_this.Bar.set, final] = value;
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect3")]
        [PseudoNotInlineable]
        private int Bar_Override3
        {
            get
            {
                // Should invoke override 1.
                _ = link[_this.Bar.get, @base];
                // Should invoke override 1.
                _ = link[_this.Bar.get, previous];
                // Should invoke override 3.
                _ = link[_this.Bar.get, current];
                // Should invoke the final declaration.
                _ = link[_this.Bar.get, final];

                return 42;
            }

            set
            {
                // Should invoke override 1.
                link[_this.Bar.set, @base] = value;
                // Should invoke override 1.
                link[_this.Bar.set, previous] = value;
                // Should invoke override 3.
                link[_this.Bar.set, current] = value;
                // Should invoke the final declaration.
                link[_this.Bar.set, final] = value;
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect5")]
        [PseudoNotInlineable]
        private int Bar_Override5
        {
            get
            {
                // Should invoke override 3.
                _ = link[_this.Bar.get, @base];
                // Should invoke override 3.
                _ = link[_this.Bar.get, previous];
                // Should invoke the final declaration.
                _ = link[_this.Bar.get, current];
                // Should invoke the final declaration.
                _ = link[_this.Bar.get, final];

                return 42;
            }

            set
            {
                // Should invoke override 3.
                link[_this.Bar.set, @base] = value;
                // Should invoke override 3.
                link[_this.Bar.set, previous] = value;
                // Should invoke the final declaration.
                link[_this.Bar.set, current] = value;
                // Should invoke the final declaration.
                link[_this.Bar.set, final] = value;
            }
        }
    }
}
