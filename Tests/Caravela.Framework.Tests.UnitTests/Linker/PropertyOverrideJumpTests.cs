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
        public void Jumps()
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
            var foo = link(this.Foo);
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
                link(this.Foo) = value;
            }s
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
            return link(this.Foo);
        }
        set
        {
            Test(""Set2"");
            link(this.Foo) = value;
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

    int Foo
    {
        get
        {
            Test(""Get1"");
            var foo = link(this.Foo);
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
                link(this.Foo) = value;
            }s
            else
            {
                throw new InvalidOperationException();
            }
        }
    }
}
";

            var linkerInput = CreateLinkerInput( code );
            var linker = new AspectLinker( linkerInput );
            var result = linker.ToResult();

            var transformedText = GetCleanCompilation( result.Compilation ).SyntaxTrees.Single().GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }
    }
}