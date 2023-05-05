﻿using System;
using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Methods.Linking.IntroducedNew
{
    class Base
    {
        public virtual void Bar()
        {
        }
    }

    [PseudoLayerOrder("A0")]
    [PseudoLayerOrder("A1")]
    [PseudoLayerOrder("A2")]
    [PseudoLayerOrder("A3")]
    [PseudoLayerOrder("A4")]
    [PseudoLayerOrder("A5")]
    [PseudoLayerOrder("A6")]
    // <target>
    class Target : Base
    {
        public void Foo()
        {
            System.Console.WriteLine("This is original code (discarded).");
        }

        [PseudoIntroduction("A1")]
        [PseudoNotInlineable]
        public new void Bar()
        {
            Console.WriteLine("SHOULD BE DISCARDED (this is introduced code).");
        }

        [PseudoOverride(nameof(Foo), "A0")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public void Foo_Override0()
        {
            // Should invoke base declaration.
            link(_this.Bar, @base)();
            // Should invoke base declaration.
            link(_this.Bar, previous)();
            // Should invoke base declaration.
            link(_this.Bar, current)();
            // Should invoke the final declaration.
            link(_this.Bar, final)();
        }

        [PseudoOverride(nameof(Foo), "A2")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public void Foo_Override2()
        {
            // Should invoke override 1_2.
            link(_this.Bar, @base)();
            // Should invoke override 1_2.
            link(_this.Bar, previous)();
            // Should invoke override 1_2.
            link(_this.Bar, current)();
            // Should invoke the final declaration.
            link(_this.Bar, final)();
        }

        [PseudoOverride(nameof(Foo), "A4")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public void Foo_Override4()
        {
            // Should invoke override 3_2.
            link(_this.Bar, @base)();
            // Should invoke override 3_2.
            link(_this.Bar, previous)();
            // Should invoke override 3_2.
            link(_this.Bar, current)();
            // Should invoke the final declaration.
            link(_this.Bar, final)();
        }

        [PseudoOverride(nameof(Foo), "A6")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public void Foo_Override6()
        {
            // Should invoke the final declaration.
            link(_this.Bar, @base)();
            // Should invoke the final declaration.
            link(_this.Bar, previous)();
            // Should invoke the final declaration.
            link(_this.Bar, current)();
            // Should invoke the final declaration.
            link(_this.Bar, final)();
        }

        [PseudoOverride(nameof(Bar), "A1")]
        [PseudoNotInlineable]
        private void Bar_Override1_1()
        {
            // Should invoke base declaration.
            link(_this.Bar, @base)();
            // Should invoke base declaration.
            link(_this.Bar, previous)();
            // Should invoke override 1_2.
            link(_this.Bar, current)();
            // Should invoke the final declaration.
            link(_this.Bar, final)();
        }

        [PseudoOverride(nameof(Bar), "A1")]
        [PseudoNotInlineable]
        private void Bar_Override1_2()
        {
            // Should invoke base declaration.
            link(_this.Bar, @base)();
            // Should invoke override 1_1.
            link(_this.Bar, previous)();
            // Should invoke override 1_2.
            link(_this.Bar, current)();
            // Should invoke the final declaration.
            link(_this.Bar, final)();
        }

        [PseudoOverride(nameof(Bar), "A3")]
        [PseudoNotInlineable]
        private void Bar_Override3_1()
        {
            // Should invoke override 1_2.
            link(_this.Bar, @base)();
            // Should invoke override 1_2.
            link(_this.Bar, previous)();
            // Should invoke override 3_2.
            link(_this.Bar, current)();
            // Should invoke the final declaration.
            link(_this.Bar, final)();
        }

        [PseudoOverride(nameof(Bar), "A3")]
        [PseudoNotInlineable]
        private void Bar_Override3_2()
        {
            // Should invoke override 1_2.
            link(_this.Bar, @base)();
            // Should invoke override 3_1.
            link(_this.Bar, previous)();
            // Should invoke override 3_2.
            link(_this.Bar, current)();
            // Should invoke the final declaration.
            link(_this.Bar, final)();
        }

        [PseudoOverride(nameof(Bar), "A5")]
        [PseudoNotInlineable]
        private void Bar_Override5_1()
        {
            // Should invoke override 3_2.
            link(_this.Bar, @base)();
            // Should invoke override 3_2.
            link(_this.Bar, previous)();
            // Should invoke the final declaration.
            link(_this.Bar, current)();
            // Should invoke the final declaration.
            link(_this.Bar, final)();
        }

        [PseudoOverride(nameof(Bar), "A5")]
        [PseudoNotInlineable]
        private void Bar_Override5_2()
        {
            // Should invoke override 3_2.
            link(_this.Bar, @base)();
            // Should invoke override 5_1.
            link(_this.Bar, previous)();
            // Should invoke the final declaration.
            link(_this.Bar, current)();
            // Should invoke the final declaration.
            link(_this.Bar, final)();
        }
    }
}
