﻿using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.EventFields.Linking.OverridenBaseEvent
{
    class Base
    {
        public virtual event EventHandler? Foo;
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
        public override event EventHandler? Foo;

        [PseudoOverride(nameof(Foo), "TestAspect0")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public event EventHandler Foo_Override0
        {
            add
            {
                // Should invoke base declaration.
                link[_this.Bar, original] += value;
                // Should invoke base declaration.
                link[_this.Bar, @base] += value;
                // Should invoke base declaration.
                link[_this.Bar, self] += value;
                // Should invoke the final declaration.
                link[_this.Bar, final] += value;
            }

            remove
            {
                // Should invoke base declaration.
                link[_this.Bar, original] -= value;
                // Should invoke base declaration.
                link[_this.Bar, @base] -= value;
                // Should invoke base declaration.
                link[_this.Bar, self] -= value;
                // Should invoke the final declaration.
                link[_this.Bar, final] -= value;
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect2")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public event EventHandler Foo_Override2
        {
            add
            {
                // Should invoke base declaration.
                link[_this.Bar, original] += value;
                // Should invoke override 1.
                link[_this.Bar, @base] += value;
                // Should invoke override 1.
                link[_this.Bar, self] += value;
                // Should invoke the final declaration.
                link[_this.Bar, final] += value;
            }

            remove
            {
                // Should invoke base declaration.
                link[_this.Bar, original] -= value;
                // Should invoke override 1.
                link[_this.Bar, @base] -= value;
                // Should invoke override 1.
                link[_this.Bar, self] -= value;
                // Should invoke the final declaration.
                link[_this.Bar, final] -= value;
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect4")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public event EventHandler Foo_Override4
        {
            add
            {
                // Should invoke base declaration.
                link[_this.Bar, original] += value;
                // Should invoke override 3.
                link[_this.Bar, @base] += value;
                // Should invoke override 3.
                link[_this.Bar, self] += value;
                // Should invoke the final declaration.
                link[_this.Bar, final] += value;
            }

            remove
            {
                // Should invoke base declaration.
                link[_this.Bar, original] -= value;
                // Should invoke override 3.
                link[_this.Bar, @base] -= value;
                // Should invoke override 3.
                link[_this.Bar, self] -= value;
                // Should invoke the final declaration.
                link[_this.Bar, final] -= value;
            }
        }

        [PseudoOverride(nameof(Foo), "TestAspect6")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public event EventHandler Foo_Override6
        {
            add
            {
                // Should invoke base declaration.
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
                // Should invoke base declaration.
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

        [PseudoOverride(nameof(Bar), "TestAspect1")]
        [PseudoNotInlineable]
        private event EventHandler Bar_Override1
        {
            add
            {
                // Should invoke base declaration.
                link[_this.Bar, original] += value;
                // Should invoke base declaration.
                link[_this.Bar, @base] += value;
                // Should invoke override 1.
                link[_this.Bar, self] += value;
                // Should invoke the final declaration.
                link[_this.Bar, final] += value;
            }

            remove
            {
                // Should invoke base declaration.
                link[_this.Bar, original] -= value;
                // Should invoke base declaration.
                link[_this.Bar, @base] -= value;
                // Should invoke override 1.
                link[_this.Bar, self] -= value;
                // Should invoke the final declaration.
                link[_this.Bar, final] -= value;
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect3")]
        [PseudoNotInlineable]
        private event EventHandler Bar_Override3
        {
            add
            {
                // Should invoke base declaration.
                link[_this.Bar, original] += value;
                // Should invoke override 1.
                link[_this.Bar, @base] += value;
                // Should invoke override 3.
                link[_this.Bar, self] += value;
                // Should invoke the final declaration.
                link[_this.Bar, final] += value;
            }

            remove
            {
                // Should invoke base declaration.
                link[_this.Bar, original] -= value;
                // Should invoke override 1.
                link[_this.Bar, @base] -= value;
                // Should invoke override 3.
                link[_this.Bar, self] -= value;
                // Should invoke the final declaration.
                link[_this.Bar, final] -= value;
            }
        }

        [PseudoOverride(nameof(Bar), "TestAspect5")]
        [PseudoNotInlineable]
        private event EventHandler Bar_Override5
        {
            add
            {
                // Should invoke base declaration.
                link[_this.Bar, original] += value;
                // Should invoke override 3.
                link[_this.Bar, @base] += value;
                // Should invoke the final declaration.
                link[_this.Bar, self] += value;
                // Should invoke the final declaration.
                link[_this.Bar, final] += value;
            }

            remove
            {
                // Should invoke base declaration.
                link[_this.Bar, original] -= value;
                // Should invoke override 3.
                link[_this.Bar, @base] -= value;
                // Should invoke the final declaration.
                link[_this.Bar, self] -= value;
                // Should invoke the final declaration.
                link[_this.Bar, final] -= value;
            }
        }
    }
}
