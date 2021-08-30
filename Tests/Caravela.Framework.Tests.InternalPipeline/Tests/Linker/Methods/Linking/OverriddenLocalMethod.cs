﻿using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.Methods.Linking.OverriddenLocalMethod
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
        public void Foo()
        {
            Console.WriteLine("This is original code.");
        }

        [PseudoOverride(nameof(Foo), "TestAspect0")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public void Foo_Override0()
        {
            // Should invoke original code.
            link(_this.Bar, original)();
            // Should invoke original code.
            link(_this.Bar, @base)();
            // Should invoke original code.
            link(_this.Bar, self)();
            // Should invoke the final declaration.
            link(_this.Bar, final)();
        }

        [PseudoOverride(nameof(Foo), "TestAspect2")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public void Foo_Override2()
        {
            // Should invoke original code.
            link(_this.Bar, original)();
            // Should invoke override 1.
            link(_this.Bar, @base)();
            // Should invoke override 1.
            link(_this.Bar, self)();
            // Should invoke the final declaration.
            link(_this.Bar, final)();
        }

        [PseudoOverride(nameof(Foo), "TestAspect4")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public void Foo_Override4()
        {
            // Should invoke original code.
            link(_this.Bar, original)();
            // Should invoke override 3.
            link(_this.Bar, @base)();
            // Should invoke override 3.
            link(_this.Bar, self)();
            // Should invoke the final declaration.
            link(_this.Bar, final)();
        }

        [PseudoOverride(nameof(Foo), "TestAspect6")]
        [PseudoNotInlineable]
        [PseudoNotDiscardable]
        public void Foo_Override6()
        {
            // Should invoke original code.
            link(_this.Bar, original)();
            // Should invoke the final declaration.
            link(_this.Bar, @base)();
            // Should invoke the final declaration.
            link(_this.Bar, self)();
            // Should invoke the final declaration.
            link(_this.Bar, final)();
        }

        void Bar()
        {
            Console.WriteLine("This is original code.");
        }

        [PseudoOverride(nameof(Bar), "TestAspect1")]
        [PseudoNotInlineable]
        void Bar_Override1()
        {   
            // Should invoke original code.
            link(_this.Bar, original)();
            // Should invoke original code.
            link(_this.Bar, @base)();
            // Should invoke override 1.
            link(_this.Bar, self)();
            // Should invoke the final declaration.
            link(_this.Bar, final)();
        }

        [PseudoOverride(nameof(Bar), "TestAspect3")]
        [PseudoNotInlineable]
        void Bar_Override2()
        {
            // Should invoke original code.
            link(_this.Bar, original)();
            // Should invoke override 1.
            link(_this.Bar, @base)();
            // Should invoke override 3.
            link(_this.Bar, self)();
            // Should invoke the final declaration.
            link(_this.Bar, final)();
        }

        [PseudoOverride(nameof(Bar), "TestAspect5")]
        [PseudoNotInlineable]
        void Bar_Override3()
        {
            // Should invoke original code.
            link(_this.Bar, original)();
            // Should invoke override 3.
            link(_this.Bar, @base)();
            // Should invoke the final declaration.
            link(_this.Bar, self)();
            // Should invoke the final declaration.
            link(_this.Bar, final)();
        }
    }
}
