using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.Methods.Linking.BaseMember_ThisExpression
{
    class Base
    {
        protected void Foo()
        {
        }
    }

    // <target>
    class Target : Base
    {
        [PseudoNotDiscardable]
        [PseudoNotInlineable]
        void Bar()
        {
        }

        [PseudoOverride(nameof(Bar), "TestAspect")]
        void Bar_Override()
        {
            link(_this.Foo, original)();
            link(_this.Foo, @base)();
            link(_this.Foo, self)();
            link(_this.Foo, final)();
        }
    }
}
