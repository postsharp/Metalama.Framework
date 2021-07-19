// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Tests.UnitTests.Linker.Helpers;
using System.Linq;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Linker
{
    public class PropertyOverrideJumpTests : LinkerTestBase
    {
        [Fact]
        public void NoJumps()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    int _foo;
    int Foo
    {
        get
        {
            return _foo;
        }

        set
        {
            _foo = value;
        }
    }

    [PseudoOverride(Foo, TestAspect1)]
    int Foo_Override1
    {
        get
        {
            Test(""Get1"");
            int foo;
            foo = link(this.Foo.get, inline);
            if (foo > 0)
            {
                return foo;
            }
            else
            {
                return -foo;
            }
        }
        set
        {
            Test(""Set1"");
            if (value != 0)
            {
                link(this.Foo.set, inline) = value;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }

    [PseudoOverride(Foo, TestAspect2)]
    int Foo_Override2
    {
        get
        {
            Test(""Get2"");
            return link(this.Foo.get, inline);
        }
        set
        {
            Test(""Set2"");
            link(this.Foo.set, inline) = value;
        }
    }
}
";

            var expectedCode = @"
class T
{
    void Test(string s)
    {
    }

    int _foo;
    int Foo
    {
        get
        {
            Test(""Get2"");
            Test(""Get1"");
            int foo;
            foo = _foo;
            goto __aspect_return_1;
            __aspect_return_1:
                if (foo > 0)
                {
                    return foo;
                }
                else
                {
                    return -foo;
                }
        }

        set
        {
            Test(""Set2"");
            Test(""Set1"");
            if (value != 0)
            {
                _foo = value;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
";

            var linkerInput = CreateLinkerInput( code );
            var linker = new AspectLinker( this.ServiceProvider, linkerInput );
            var result = linker.ToResult();

            var transformedText = GetCleanCompilation( result.Compilation ).SyntaxTrees.Single().Value.GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }
    }
}