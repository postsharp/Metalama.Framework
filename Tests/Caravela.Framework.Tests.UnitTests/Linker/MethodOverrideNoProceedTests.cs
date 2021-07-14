// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Tests.UnitTests.Linker.Helpers;
using System.Linq;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Linker
{
    public class MethodOverrideNoProceedTests : LinkerTestBase
    {
        [Fact]
        public void ReturnsVoid_NP()
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

    [PseudoOverride(Foo, TestAspect)]
    void Foo_Override(int x)
    {
        Test(""Override"");
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
        Test(""Override"");
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
        public void ReturnsVoid_NP_P()
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
        Test(""Override1 Start"");
        link(this.Foo, inline)(x);
        Test(""Override1 End"");
    }

    [PseudoOverride(Foo, TestAspect2)]
    void Foo_Override2(int x)
    {
        Test(""Override2"");
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
        Test(""Override2"");
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
        public void ReturnsVoid_P_NP()
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
        Test(""Override1"");
    }

    [PseudoOverride(Foo, TestAspect2)]
    void Foo_Override2(int x)
    {
        Test(""Override2 Start"");
        link(this.Foo, inline)(x);
        Test(""Override2 End"");
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
        Test(""Override2 Start"");
        Test(""Override1"");
        Test(""Override2 End"");
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
        public void ReturnsVoid_NP_NP()
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
        Test(""Override1"");
    }

    [PseudoOverride(Foo, TestAspect2)]
    void Foo_Override2(int x)
    {
        Test(""Override2"");
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
        Test(""Override2"");
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