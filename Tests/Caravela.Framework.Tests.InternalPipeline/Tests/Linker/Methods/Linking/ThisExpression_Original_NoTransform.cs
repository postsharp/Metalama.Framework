using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.Methods.Linking.ThisExpression_Original_NoTransform
{
    // <target>
    class Target
    {
        void Foo()
        {
        }

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
