// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Tests.UnitTests.Linker.Helpers;
using System.Linq;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Linker
{
    public class MethodInlinerTests : LinkerTestBase
    {
        [Fact]
        public void MethodInvocation_Void()
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

            var transformedText = GetCleanCompilation( result.Compilation ).SyntaxTrees.Single().Value.GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }

        [Fact]
        public void MethodInvocation_Int()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    int Foo()
    {
        Test(""Original"");
        return 42;
    }

    [PseudoOverride(Foo, TestAspect)]
    int Foo_Override()
    {
        Test(""Before"");
        link(this.Foo, inline)();
        Test(""After"");
        return 42;
    }
}
";

            var expectedCode = @"
class T
{
    void Test(string s)
    {
    }

    int Foo()
    {
        Test(""Before"");
        Test(""Original"");
        _ = 42;
        goto __aspect_return_1;
        __aspect_return_1:
            Test(""After"");
        return 42;
    }
}
";

            var linkerInput = CreateLinkerInput( code );
            var linker = new AspectLinker( this.ServiceProvider, linkerInput );
            var result = linker.ToResult();

            var transformedText = GetCleanCompilation( result.Compilation ).SyntaxTrees.Single().Value.GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }

        [Fact]
        public void MethodDiscard()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    int Foo()
    {
        Test(""Original"");
        return 42;
    }

    [PseudoOverride(Foo, TestAspect)]
    int Foo_Override()
    {
        Test(""Before"");
        _ = link(this.Foo, inline)();        
        Test(""After"");
        return 42;
    }
}
";

            var expectedCode = @"
class T
{
    void Test(string s)
    {
    }

    int Foo()
    {
        Test(""Before"");
        Test(""Original"");
        _ = 42;
        goto __aspect_return_1;
        __aspect_return_1:
            Test(""After"");
        return 42;
    }
}
";

            var linkerInput = CreateLinkerInput( code );
            var linker = new AspectLinker( this.ServiceProvider, linkerInput );
            var result = linker.ToResult();

            var transformedText = GetCleanCompilation( result.Compilation ).SyntaxTrees.Single().Value.GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }

        [Fact]
        public void MethodAssignment()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    int Foo()
    {
        Test(""Original"");
        return 42;
    }

    [PseudoOverride(Foo, TestAspect)]
    int Foo_Override()
    {
        Test(""Before"");
        int x;
        x = link(this.Foo, inline)();        
        Test(""After"");
        return x;
    }
}
";

            var expectedCode = @"
class T
{
    void Test(string s)
    {
    }

    int Foo()
    {
        Test(""Before"");
        int x;
        Test(""Original"");
        x = 42;
        goto __aspect_return_1;
        __aspect_return_1:
            Test(""After"");
        return x;
    }
}
";

            var linkerInput = CreateLinkerInput( code );
            var linker = new AspectLinker( this.ServiceProvider, linkerInput );
            var result = linker.ToResult();

            var transformedText = GetCleanCompilation( result.Compilation ).SyntaxTrees.Single().Value.GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }

        [Fact]
        public void MethodAssignment_NonSimple()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    int Foo()
    {
        Test(""Original"");
        return 42;
    }

    [PseudoOverride(Foo, TestAspect)]
    int Foo_Override()
    {
        Test(""Before"");
        int x;
        x += link(this.Foo, inline)();        
        Test(""After"");
        return x;
    }
}
";

            var expectedCode = @"
class T
{
    void Test(string s)
    {
    }

    int Foo()
    {
        Test(""Before"");
        int x;
        x += this.__Foo__OriginalImpl();
        Test(""After"");
        return x;
    }

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

            var transformedText = GetCleanCompilation( result.Compilation ).SyntaxTrees.Single().Value.GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }

        [Fact]
        public void MethodReturn()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    int Foo()
    {
        Test(""Original"");
        return 42;
    }

    [PseudoOverride(Foo, TestAspect)]
    int Foo_Override()
    {
        Test(""Before"");      
        return link(this.Foo, inline)(); 
    }
}
";

            var expectedCode = @"
class T
{
    void Test(string s)
    {
    }

    int Foo()
    {
        Test(""Before"");
        Test(""Original"");
        return 42;
    }
}
";

            var linkerInput = CreateLinkerInput( code );
            var linker = new AspectLinker( this.ServiceProvider, linkerInput );
            var result = linker.ToResult();

            var transformedText = GetCleanCompilation( result.Compilation ).SyntaxTrees.Single().Value.GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }

        [Fact]
        public void MethodCastReturn()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    int Foo()
    {
        Test(""Original"");
        return 42;
    }

    [PseudoOverride(Foo, TestAspect)]
    int Foo_Override()
    {
        Test(""Before"");      
        return (int)link(this.Foo, inline)(); 
    }
}
";

            var expectedCode = @"
class T
{
    void Test(string s)
    {
    }

    int Foo()
    {
        Test(""Before"");
        Test(""Original"");
        return 42;
    }
}
";

            var linkerInput = CreateLinkerInput( code );
            var linker = new AspectLinker( this.ServiceProvider, linkerInput );
            var result = linker.ToResult();

            var transformedText = GetCleanCompilation( result.Compilation ).SyntaxTrees.Single().Value.GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }

        [Fact]
        public void MethodCastReturn_DifferentType()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    int Foo()
    {
        Test(""Original"");
        return 42;
    }

    [PseudoOverride(Foo, TestAspect)]
    int Foo_Override()
    {
        Test(""Before"");      
        return (short)link(this.Foo, inline)(); 
    }
}
";

            var expectedCode = @"
class T
{
    void Test(string s)
    {
    }

    int Foo()
    {
        Test(""Before"");
        return (short)this.__Foo__OriginalImpl();
    }

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

            var transformedText = GetCleanCompilation( result.Compilation ).SyntaxTrees.Single().Value.GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }
    }
}