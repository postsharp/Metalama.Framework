﻿using System;
using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Fields.Linking.IntroducedField
{
    [PseudoLayerOrder("TestAspect0")]
    [PseudoLayerOrder("TestAspect1")]
    [PseudoLayerOrder("TestAspect2")]
    [PseudoLayerOrder("TestAspect3")]
    [PseudoLayerOrder("TestAspect4")]
    [PseudoLayerOrder("TestAspect5")]
    [PseudoLayerOrder("TestAspect6")]
    [PseudoLayerOrder("TestAspect7")]
    [PseudoLayerOrder("TestAspect8")]
    [PseudoLayerOrder("TestAspect9")]
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
                // Should invoke source code.
                _ = link[_this.Bar, @base];
                // Should invoke source code.
                _ = link[_this.Bar, self];
                // Should invoke the final declaration.
                _ = link[_this.Bar, final];

                return 42;
            }

            set
            {
                // Should invoke empty code.
                link[_this.Bar, original] = value;
                // Should invoke source code.
                link[_this.Bar, @base] = value;
                // Should invoke source code.
                link[_this.Bar, self] = value;
                // Should invoke the final declaration.
                link[_this.Bar, final] = value;
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect5")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public int Foo_Override5
        {
            get
            {
                // Should invoke empty code.
                _ = link[_this.Bar, original];
                // Should invoke override 4.
                _ = link[_this.Bar, @base];
                // Should invoke override 4.
                _ = link[_this.Bar, self];
                // Should invoke the final declaration.
                _ = link[_this.Bar, final];

                return 42;
            }

            set
            {
                // Should invoke empty code.
                link[_this.Bar, original] = value;
                // Should invoke override 4.
                link[_this.Bar, @base] = value;
                // Should invoke override 4.
                link[_this.Bar, self] = value;
                // Should invoke the final declaration.
                link[_this.Bar, final] = value;
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect7")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public int Foo_Override7
        {
            get
            {
                // Should invoke empty code.
                _ = link[_this.Bar, original];
                // Should invoke override 6.
                _ = link[_this.Bar, @base];
                // Should invoke override 6.
                _ = link[_this.Bar, self];
                // Should invoke the final declaration.
                _ = link[_this.Bar, final];

                return 42;
            }

            set
            {
                // Should invoke empty code.
                link[_this.Bar, original] = value;
                // Should invoke override 6.
                link[_this.Bar, @base] = value;
                // Should invoke override 6.
                link[_this.Bar, self] = value;
                // Should invoke the final declaration.
                link[_this.Bar, final] = value;
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect9")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public int Foo_Override9
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

        [PseudoReplaced]
        [PseudoIntroduction( "TestAspect1")]
        public int Bar;

        [PseudoReplacement(nameof(Bar))]
        [PseudoIntroduction("TestAspect3")]
        public int Bar_Replacement { get; set; }

        [PseudoOverride(nameof(Bar), "TestAspect4")]
        [PseudoNotInlineable]
        public int Bar_Override4
        {
            get
            {
                // Should invoke empty code.
                _ = link[_this.Bar, original];
                // Should invoke source code.
                _ = link[_this.Bar, @base];
                // Should invoke override 4.
                _ = link[_this.Bar, self];
                // Should invoke the final declaration.
                _ = link[_this.Bar, final];

                return 42;
            }

            set
            {
                // Should invoke empty code.
                link[_this.Bar, original] = value;
                // Should invoke source code.
                link[_this.Bar, @base] = value;
                // Should invoke override 4.
                link[_this.Bar, self] = value;
                // Should invoke the final declaration.
                link[_this.Bar, final] = value;
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect6")]
        [PseudoNotInlineable]
        public int Bar_Override6
        {
            get
            {
                // Should invoke empty code.
                _ = link[_this.Bar, original];
                // Should invoke override 4.
                _ = link[_this.Bar, @base];
                // Should invoke override 6.
                _ = link[_this.Bar, self];
                // Should invoke the final declaration.
                _ = link[_this.Bar, final];

                return 42;
            }

            set
            {
                // Should invoke empty code.
                link[_this.Bar, original] = value;
                // Should invoke override 4.
                link[_this.Bar, @base] = value;
                // Should invoke override 6.
                link[_this.Bar, self] = value;
                // Should invoke the final declaration.
                link[_this.Bar, final] = value;
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect8")]
        [PseudoNotInlineable]
        public int Bar_Override8
        {
            get
            {
                // Should invoke empty code.
                _ = link[_this.Bar, original];
                // Should invoke override 6.
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
                // Should invoke override 6.
                link[_this.Bar, @base] = value;
                // Should invoke the final declaration.
                link[_this.Bar, self] = value;
                // Should invoke the final declaration.
                link[_this.Bar, final] = value;
            }
        }
    }
}
