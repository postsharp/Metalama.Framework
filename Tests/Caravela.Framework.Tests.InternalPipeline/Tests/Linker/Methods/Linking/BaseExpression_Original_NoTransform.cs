using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.Methods.Linking.BaseExpression_Original_NoTransform
{
    // <target>
    class Base
    {
        protected void Foo()
        {
        }
    }

    class Target : Base
    {
        void Bar()
        {
        }

        [PseudoOverride(nameof(Bar), "TestAspect")]
        void Bar_Override()
        {
            link(_this.Foo, original)();
        }
    }
}
