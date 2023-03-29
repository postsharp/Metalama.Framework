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
        public void Foo()
        {
        }


        [PseudoOverride(nameof(Foo), "TestAspect0")]
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

        [PseudoOverride(nameof(Foo), "TestAspect2")]
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

        [PseudoOverride(nameof(Foo), "TestAspect4")]
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

        [PseudoOverride(nameof(Foo), "TestAspect6")]
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

        [PseudoIntroduction("TestAspect1")]
        [PseudoNotInlineable]
        public new void Bar()
        {
        }

        [PseudoOverride(nameof(Bar), "TestAspect1")]
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

        [PseudoOverride(nameof(Bar), "TestAspect1")]
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

        [PseudoOverride(nameof(Bar), "TestAspect3")]
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

        [PseudoOverride(nameof(Bar), "TestAspect3")]
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

        [PseudoOverride(nameof(Bar), "TestAspect5")]
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

        [PseudoOverride(nameof(Bar), "TestAspect5")]
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
