using System;
using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Properties.Inliners.GetterCastReturn_NotSameType
{
    class Base
    {
        public virtual int Foo
        {
            get
            {
                return 0;
            }
        }
    }

    // <target>
    class Target : Base
    {
        [PseudoIntroduction(nameof(Foo), "TestAspect")]
        public override int Foo
        {
            get
            {
                Console.WriteLine( "Original");
                return 42;
            }
        }

        [PseudoOverride( nameof(Foo),"TestAspect")]
        int Foo_Override
        {
            get
            {
                Console.WriteLine( "Before");
                return (int)link( _this.Foo.get, inline, @base);
            }
        }
    }
}
