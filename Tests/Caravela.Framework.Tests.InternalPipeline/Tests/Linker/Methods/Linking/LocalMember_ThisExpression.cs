using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.Methods.Linking.LocalMember_ThisExpression
{
    // <target>
    class Target
    {
        void Bar()
        {
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

        [PseudoOverride(nameof(Bar), "TestAspect2")]
        [PseudoNotInlineable]
        void Bar_Override2()
        {
            // Should invoke original code.
            link(_this.Bar, original)();
            // Should invoke override 1.
            link(_this.Bar, @base)();
            // Should invoke override 2.
            link(_this.Bar, self)();
            // Should invoke the final declaration.
            link(_this.Bar, final)();
        }

        [PseudoOverride(nameof(Bar), "TestAspect3")]
        [PseudoNotInlineable]
        void Bar_Override3()
        {
            // Should invoke original code.
            link(_this.Bar, original)();
            // Should invoke override 2.
            link(_this.Bar, @base)();
            // Should invoke the final declaration.
            link(_this.Bar, self)();
            // Should invoke the final declaration.
            link(_this.Bar, final)();
        }
    }
}
