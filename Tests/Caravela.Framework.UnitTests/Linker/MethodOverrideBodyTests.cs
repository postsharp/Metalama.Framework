// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Linq;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.UnitTests.Linker.Helpers;
using Xunit;

namespace Caravela.Framework.UnitTests.Linker
{
    public partial class MethodOverrideBodyTests : LinkerTestBase
    {
        [Fact]
        public void ReturnsVoid_BeforeAfterStatements()
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
        link(this.Foo());
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
            var linker = new AspectLinker( linkerInput );
            var result = linker.ToResult();

            var transformedText = GetCleanCompilation( result.Compilation ).SyntaxTrees.Single().GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }

        [Fact]
        public void ReturnsInt_BeforeAfterStatements()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    int Foo(int x)
    {
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
        result = x;
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
