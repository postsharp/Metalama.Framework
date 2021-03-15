// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Linq;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.UnitTests.Linker.Helpers;
using Xunit;

namespace Caravela.Framework.UnitTests.Linker
{
    public partial class MethodOverrideJumpTests : LinkerTestBase
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

    [PseudoForceNotInlineable]
    int Foo()
    {
        Test(""Original"");
        return 42;
    }

    [PseudoOverride(Foo, TestAspect)]
    int Foo_Override()
    {
        Test(""Before"");
        int result;
        result = link(this.Foo());
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

    [Caravela.Framework.Aspects.AspectLinkerOptions(ForceNotInlineable = true)]
    int Foo()
    {
        Test(""Before"");
        int result;
        result = this.__Foo__OriginalBody();
        Test(""After"");
        return result;
    }

    [Caravela.Framework.Aspects.AspectLinkerOptions(ForceNotInlineable = true)]
    int __Foo__OriginalBody()
    {
        Test(""Original"");
        return 42;
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
