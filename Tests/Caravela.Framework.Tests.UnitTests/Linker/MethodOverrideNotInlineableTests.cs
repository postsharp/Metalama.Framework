// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Tests.UnitTests.Linker.Helpers;
using System.Linq;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Linker
{
    public class MethodOverrideNotInlineableTests : LinkerTestBase
    {
        [Fact]
        public void ReturnsVoid_I_NI()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    [PseudoForceNotInlineable]
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

    [Caravela.Framework.Aspects.AspectLinkerOptions(ForceNotInlineable = true)]
    void Foo()
    {
        Test(""Before"");
        this.__Foo__OriginalImpl();
        Test(""After"");
    }

    [Caravela.Framework.Aspects.AspectLinkerOptions(ForceNotInlineable = true)]
    void __Foo__OriginalImpl()
    {
        Test(""Original"");
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
        public void ReturnsVoid_NI_I()
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

    [PseudoForceNotInlineable]
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
        this.Foo_Override();
    }

    void Foo_Override()
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
        public void ReturnsVoid_NI_NI()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    [PseudoForceNotInlineable]
    void Foo()
    {
        Test(""Original"");
    }

    [PseudoOverride(Foo, TestAspect)]
    [PseudoForceNotInlineable]
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

    [Caravela.Framework.Aspects.AspectLinkerOptions(ForceNotInlineable = true)]
    void Foo()
    {
        this.Foo_Override();
    }

    [Caravela.Framework.Aspects.AspectLinkerOptions(ForceNotInlineable = true)]
    void __Foo__OriginalImpl()
    {
        Test(""Original"");
    }

    void Foo_Override()
    {
        Test(""Before"");
        this.__Foo__OriginalImpl();
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
        public void ReturnsVoid_NI_I_NI()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    [PseudoForceNotInlineable]
    void Foo()
    {
        Test(""Original"");
    }

    [PseudoOverride(Foo, TestAspect1)]
    void Foo_Override1()
    {
        Test(""Before1"");
        link(this.Foo, inline)();
        Test(""After1"");
    }

    [PseudoOverride(Foo, TestAspect2)]
    [PseudoForceNotInlineable]
    void Foo_Override2()
    {
        Test(""Before2"");
        link(this.Foo, inline)();
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

    [Caravela.Framework.Aspects.AspectLinkerOptions(ForceNotInlineable = true)]
    void Foo()
    {
        this.Foo_Override2();
    }

    [Caravela.Framework.Aspects.AspectLinkerOptions(ForceNotInlineable = true)]
    void __Foo__OriginalImpl()
    {
        Test(""Original"");
    }

    void Foo_Override2()
    {
        Test(""Before2"");
        Test(""Before1"");
        this.__Foo__OriginalImpl();
        Test(""After1"");
        Test(""After2"");
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
        public void ReturnsVoid_I_NI_I()
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

    [PseudoOverride(Foo, TestAspect1)]
    [PseudoForceNotInlineable]
    void Foo_Override1()
    {
        Test(""Before1"");
        link(this.Foo, inline)();
        Test(""After1"");
    }

    [PseudoOverride(Foo, TestAspect2)]
    void Foo_Override2()
    {
        Test(""Before2"");
        link(this.Foo, inline)();
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

    void Foo()
    {
        Test(""Before2"");
        this.Foo_Override1();
        Test(""After2"");
    }

    void Foo_Override1()
    {
        Test(""Before1"");
        Test(""Original"");
        Test(""After1"");
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
        public void ReturnsInt_I_NI()
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
        result = link(this.Foo, inline)();
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
        result = this.__Foo__OriginalImpl();
        Test(""After"");
        return result;
    }

    [Caravela.Framework.Aspects.AspectLinkerOptions(ForceNotInlineable = true)]
    int __Foo__OriginalImpl()
    {
        Test(""Original"");
        return 42;
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