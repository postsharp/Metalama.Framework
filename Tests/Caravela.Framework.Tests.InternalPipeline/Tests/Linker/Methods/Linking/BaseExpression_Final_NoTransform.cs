using System;
using static Caravela.Framework.Tests.Integration.Tests.Linker.Api;

namespace Caravela.Framework.Tests.Integration.Tests.Linker.Methods.Linking.BaseExpression_Final_NoTransform
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
        void Bar()
        {
        }

        [PseudoOverride(nameof(Bar), "TestAspect")]
        void Bar_Override()
        {
            link(_this.Foo, final)();
        }
    }
}
