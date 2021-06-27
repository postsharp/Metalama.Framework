// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Tests.UnitTests.Linker.Helpers;
using System.Linq;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Linker
{
    public class MethodOverrideBodyTests : LinkerTestBase
    {
        [Fact]
        public void ReturnsVoid_Simple()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    void Foo()
    {
        Test(""Original"");
    }

    [PseudoOverride(Foo, TestAspect)]
    void Foo_Override()
    {
        Test(""Before"");
        link(this.Foo, inline)();
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

    void Foo()
    {
        Test(""Before"");
        Test(""Original"");
        Test(""After"");
    }
}
";

            var linkerInput = CreateLinkerInput( code );
            var linker = new AspectLinker( this.ServiceProvider, linkerInput );
            var result = linker.ToResult();

            var transformedText = GetCleanCompilation( result.Compilation ).SyntaxTrees.Single().GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }

        [Fact]
        public void ReturnsVoid_Condition()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    void Foo()
    {
        Test(""Original"");
    }

    [PseudoOverride(Foo, TestAspect)]
    void Foo_Override()
    {
        Test(""Before"");
        if (true)
        {
            link(this.Foo, inline)();
        }

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

    void Foo()
    {
        Test(""Before"");
        if (true)
        {
            Test(""Original"");
        }

        Test(""After"");
    }
}
";

            var linkerInput = CreateLinkerInput( code );
            var linker = new AspectLinker( this.ServiceProvider, linkerInput );
            var result = linker.ToResult();

            var transformedText = GetCleanCompilation( result.Compilation ).SyntaxTrees.Single().GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }

        [Fact]
        public void ReturnsVoid_While()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    void Foo()
    {
        Test(""Original"");
    }

    [PseudoOverride(Foo, TestAspect)]
    void Foo_Override()
    {
        Test(""Before"");
        int i;
        while (i < 5)
        {
            link(this.Foo, inline)();
            i++;
        }

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

    void Foo()
    {
        Test(""Before"");
        int i;
        while (i < 5)
        {
            Test(""Original"");
            i++;
        }

        Test(""After"");
    }
}
";

            var linkerInput = CreateLinkerInput( code );
            var linker = new AspectLinker( this.ServiceProvider, linkerInput );
            var result = linker.ToResult();

            var transformedText = GetCleanCompilation( result.Compilation ).SyntaxTrees.Single().GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }

        [Fact]
        public void ReturnsVoid_For()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    void Foo()
    {
        Test(""Original"");
    }

    [PseudoOverride(Foo, TestAspect)]
    void Foo_Override()
    {
        Test(""Before"");
        for (int i = 0; i < 5; i++)
        {
            link(this.Foo, inline)();
        }

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

    void Foo()
    {
        Test(""Before"");
        for (int i = 0; i < 5; i++)
        {
            Test(""Original"");
        }

        Test(""After"");
    }
}
";

            var linkerInput = CreateLinkerInput( code );
            var linker = new AspectLinker( this.ServiceProvider, linkerInput );
            var result = linker.ToResult();

            var transformedText = GetCleanCompilation( result.Compilation ).SyntaxTrees.Single().GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }

        [Fact]
        public void ReturnsVoid_Foreach()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    void Foo()
    {
        Test(""Original"");
    }

    [PseudoOverride(Foo, TestAspect)]
    void Foo_Override()
    {
        Test(""Before"");
        foreach (var i in new[]{1, 2, 3, 4, 5})
        {
            link(this.Foo, inline)();
        }

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

    void Foo()
    {
        Test(""Before"");
        foreach (var i in new[]{1, 2, 3, 4, 5})
        {
            Test(""Original"");
        }

        Test(""After"");
    }
}
";

            var linkerInput = CreateLinkerInput( code );
            var linker = new AspectLinker( this.ServiceProvider, linkerInput );
            var result = linker.ToResult();

            var transformedText = GetCleanCompilation( result.Compilation ).SyntaxTrees.Single().GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }

        [Fact]
        public void ReturnsInt_Simple()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    int Foo(int x)
    {
        Test(""Original"");
        return x;
    }

    [PseudoOverride(Foo, TestAspect)]
    int Foo_Override(int x)
    {
        Test(""Before"");
        int result;
        result = link(this.Foo, inline)(x);
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
        Test(""Original"");
        result = x;
        goto __aspect_return_1;
        __aspect_return_1:
        Test(""After"");
        return result;
    }
}
";

            var linkerInput = CreateLinkerInput( code );
            var linker = new AspectLinker( this.ServiceProvider, linkerInput );
            var result = linker.ToResult();

            var transformedText = GetCleanCompilation( result.Compilation ).SyntaxTrees.Single().GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }

        [Fact]
        public void ReturnsInt_Condition()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    int Foo(int x)
    {
        Test(""Original"");
        return x;
    }

    [PseudoOverride(Foo, TestAspect)]
    int Foo_Override(int x)
    {
        Test(""Before"");
        int result = 0;
        if (x == 0)
        {
            result = link(this.Foo, inline)(x);
        }

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
        int result = 0;
        if (x == 0)
        {
            Test(""Original"");
            result = x;
            goto __aspect_return_1;
            __aspect_return_1:
        }

        Test(""After"");
        return result;
    }
}
";

            var linkerInput = CreateLinkerInput( code );
            var linker = new AspectLinker( this.ServiceProvider, linkerInput );
            var result = linker.ToResult();

            var transformedText = GetCleanCompilation( result.Compilation ).SyntaxTrees.Single().GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }

        [Fact]
        public void ReturnsInt_While()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    int Foo(int x)
    {
        Test(""Original"");
        return x;
    }

    [PseudoOverride(Foo, TestAspect)]
    int Foo_Override(int x)
    {
        Test(""Before"");
        int i = 0;
        int k = 0;
        while (i < 0)
        {
            int result;
            result = link(this.Foo, inline)(x);
            k += result;
            i++;
        }

        Test(""After"");
        return k;
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
        int i = 0;
        int k = 0;
        while (i < 0)
        {
            int result;
            Test(""Original"");
            result = x;
            goto __aspect_return_1;
            __aspect_return_1:
            k += result;
            i++;
        }

        Test(""After"");
        return k;
    }
}
";

            var linkerInput = CreateLinkerInput( code );
            var linker = new AspectLinker( this.ServiceProvider, linkerInput );
            var result = linker.ToResult();

            var transformedText = GetCleanCompilation( result.Compilation ).SyntaxTrees.Single().GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }
    }
}