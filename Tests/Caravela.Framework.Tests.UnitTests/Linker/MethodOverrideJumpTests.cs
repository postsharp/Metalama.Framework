// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Linq;
using Caravela.Framework.Impl.Linking;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Linker
{
    public partial class MethodOverrideJumpTests : Helpers.LinkerTestBase
    {
        [Fact]
        public void ReturnsVoid_FJ()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    void Foo(int x)
    {
        Test(""Original Start"");
        if (x == 0)
        {
            return;
        }
        Test(""Original End"");
    }

    [PseudoOverride(Foo, TestAspect)]
    void Foo_Override(int x)
    {
        Test(""Before"");
        link(this.Foo(x));
        Test(""After"");
    }
}
";

            var expectedCode = @"
class T
{
    void Test(string s)
    {
    }

    void Foo(int x)
    {
        Test(""Before"");
        Test(""Original Start"");
        if (x == 0)
        {
            goto __aspect_return_1;
        }

        Test(""Original End"");
        __aspect_return_1:
            ;
        Test(""After"");
    }
}
";

            var linkerInput = CreateLinkerInput( code );
            var linker = new AspectLinker( linkerInput );
            var result = linker.ToResult();

            var transformedText = GetCleanCompilation( result.Compilation ).SyntaxTrees.Single().GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }

        [Fact]
        public void ReturnsVoid_NJ_FJ()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    void Foo(int x)
    {
        Test(""Original Start"");
        if (x == 0)
        {
            return;
        }
        Test(""Original End"");
    }

    [PseudoOverride(Foo, TestAspect1)]
    void Foo_Override1(int x)
    {
        Test(""Before1"");
        link(this.Foo(x));
        Test(""After1"");
    }

    [PseudoOverride(Foo, TestAspect2)]
    void Foo_Override2(int x)
    {
        Test(""Before2"");
        link(this.Foo(x));
        Test(""After2"");
    }
}
";

            var expectedCode = @"
class T
{
    void Test(string s)
    {
    }

    void Foo(int x)
    {
        Test(""Before2"");
        Test(""Before1"");
        Test(""Original Start"");
        if (x == 0)
        {
            goto __aspect_return_2;
        }

        Test(""Original End"");
        __aspect_return_2:
            ;
        Test(""After1"");
        Test(""After2"");
    }
}
";

            var linkerInput = CreateLinkerInput( code );
            var linker = new AspectLinker( linkerInput );
            var result = linker.ToResult();

            var transformedText = GetCleanCompilation( result.Compilation ).SyntaxTrees.Single().GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }

        [Fact]
        public void ReturnsVoid_FJ_NJ()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    void Foo(int x)
    {
        Test(""Original"");
    }

    [PseudoOverride(Foo, TestAspect1)]
    void Foo_Override1(int x)
    {
        Test(""Before1"");
        if (x == 0)
        {
            return;
        }
        link(this.Foo(x));
        Test(""After1"");
    }

    [PseudoOverride(Foo, TestAspect2)]
    void Foo_Override2(int x)
    {
        Test(""Before2"");
        link(this.Foo(x));
        Test(""After2"");
    }
}
";

            var expectedCode = @"
class T
{
    void Test(string s)
    {
    }

    void Foo(int x)
    {
        Test(""Before2"");
        Test(""Before1"");
        if (x == 0)
        {
            goto __aspect_return_1;
        }

        Test(""Original"");
        Test(""After1"");
        __aspect_return_1:
            ;
        Test(""After2"");
    }
}
";

            var linkerInput = CreateLinkerInput( code );
            var linker = new AspectLinker( linkerInput );
            var result = linker.ToResult();

            var transformedText = GetCleanCompilation( result.Compilation ).SyntaxTrees.Single().GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }

        [Fact]
        public void ReturnsVoid_FJ_FJ()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    void Foo(int x)
    {
        Test(""Original Start"");
        if (x == 0)
        {
            return;
        }

        Test(""Original End"");
    }

    [PseudoOverride(Foo, TestAspect1)]
    void Foo_Override1(int x)
    {
        Test(""Before1"");
        if (x == 0)
        {
            return;
        }
        link(this.Foo(x));
        Test(""After1"");
    }

    [PseudoOverride(Foo, TestAspect2)]
    void Foo_Override2(int x)
    {
        Test(""Before2"");
        link(this.Foo(x));
        Test(""After2"");
    }
}
";

            var expectedCode = @"
class T
{
    void Test(string s)
    {
    }

    void Foo(int x)
    {
        Test(""Before2"");
        Test(""Before1"");
        if (x == 0)
        {
            goto __aspect_return_1;
        }

        Test(""Original Start"");
        if (x == 0)
        {
            goto __aspect_return_2;
        }

        Test(""Original End"");
        __aspect_return_2:
            ;
        Test(""After1"");
        __aspect_return_1:
            ;
        Test(""After2"");
    }
}
";

            var linkerInput = CreateLinkerInput( code );
            var linker = new AspectLinker( linkerInput );
            var result = linker.ToResult();

            var transformedText = GetCleanCompilation( result.Compilation ).SyntaxTrees.Single().GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }

        [Fact]
        public void ReturnsInt_FJ()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    int Foo(int x)
    {
        Test(""Original Start"");
        if (x == 0)
        {
            return 42;
        }
        Test(""Original End"");
        return x;
    }

    [PseudoOverride(Foo, TestAspect)]
    int Foo_Override(int x)
    {
        Test(""Before"");
        int result;
        result = link(this.Foo(x));
        Test(""After"");
        return result;
    }
}
";

            var expectedCode = @"
class T
{
    void Test(string s)
    {
    }

    int Foo(int x)
    {
        Test(""Before"");
        int result;
        Test(""Original Start"");
        if (x == 0)
        {
            result = 42;
            goto __aspect_return_1;
        }

        Test(""Original End"");
        result = x;
        goto __aspect_return_1;
        __aspect_return_1:
            ;
        Test(""After"");
        return result;
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
