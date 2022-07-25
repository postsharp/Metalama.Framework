using System;
using static Metalama.Framework.Tests.Integration.Tests.Linker.Api;

namespace Metalama.Framework.Tests.Integration.Tests.Linker.Properties.Overrides.Body.ExpressionBodies
{
    // <target>
    class Target
    {
        int _foo;
        int Foo
        {
            get => this._foo;
            set => this._foo = value;
        }

        [PseudoOverride( nameof(Foo),"TestAspect")]
        int Foo_Override
        {
            get
            {
                Console.WriteLine( "Get");
                return link[ _this.Foo.get, inline ];
            }
            set
            {
                Console.WriteLine( "Set");
                link[ _this.Foo.set, inline ] = value;
            }
        }
    }
}
