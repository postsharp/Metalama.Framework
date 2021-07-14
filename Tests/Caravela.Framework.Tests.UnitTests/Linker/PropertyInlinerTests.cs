// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Tests.UnitTests.Linker.Helpers;
using System.Linq;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Linker
{
    public class PropertyInlinerTests : LinkerTestBase
    {
        [Fact]
        public void GetterReturn()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    int Foo
    {
        get 
        {
            Test(""Original"");
            return 42;
        }
    }

    [PseudoOverride(Foo, TestAspect)]
    int Foo_Override
    {
        get
        {
            Test(""Before"");
            return link(this.Foo.get, inline);
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
            Test(""Before"");
            Test(""Original"");
            return 42;
        }
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
        public void GetterCastReturn()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    int Foo
    {
        get 
        {
            Test(""Original"");
            return 42;
        }
    }

    [PseudoOverride(Foo, TestAspect)]
    int Foo_Override
    {
        get
        {
            Test(""Before"");
            return (int)link(this.Foo.get, inline);
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
            Test(""Before"");
            Test(""Original"");
            return 42;
        }
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
        public void GetterCastReturn_DifferentType()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    int Foo
    {
        get 
        {
            Test(""Original"");
            return 42;
        }
    }

    [PseudoOverride(Foo, TestAspect)]
    int Foo_Override
    {
        get
        {
            Test(""Before"");
            return (short)link(this.Foo.get, inline);
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
            Test(""Before"");
            return (short)this.__Foo__OriginalImpl;
        }
    }

    int __Foo__OriginalImpl
    {
        get
        {
            Test(""Original"");
            return 42;
        }
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
        public void GetterAssignment()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    int Foo
    {
        get 
        {
            Test(""Original"");
            return 42;
        }
    }

    [PseudoOverride(Foo, TestAspect)]
    int Foo_Override
    {
        get
        {
            Test(""Before"");
            int x;
            x = link(this.Foo.get, inline);
            Test(""After"");
            return x;
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
}
";

            var linkerInput = CreateLinkerInput( code );
            var linker = new AspectLinker( this.ServiceProvider, linkerInput );
            var result = linker.ToResult();

            var transformedText = GetCleanCompilation( result.Compilation ).SyntaxTrees.Single().GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }

        [Fact]
        public void GetterAssignment_AddAssignment()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    int Foo
    {
        get
        {
            Test(""Original"");
            return 42;
        }
    }

    [PseudoOverride(Foo, TestAspect)]
    int Foo_Override
    {
        get
        {
            Test(""Before"");
            int x;
            x += link(this.Foo.get, inline);
            Test(""After"");
            return x;
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
            Test(""Before"");
            int x;
            x += this.__Foo__OriginalImpl;
            Test(""After"");
            return x;
        }
    }

    int __Foo__OriginalImpl
    {
        get
        {
            Test(""Original"");
            return 42;
        }
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
        public void SetterAssignment()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    int field;

    int Foo
    {
        set 
        {
            Test(""Original"");
            this.field = value;
        }
    }

    [PseudoOverride(Foo, TestAspect)]
    int Foo_Override
    {
        set
        {
            Test(""Before"");
            link(this.Foo.set, inline) = value;
            Test(""After"");
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

    int field;
    int Foo
    {
        set
        {
            Test(""Before"");
            Test(""Original"");
            this.field = value;
            Test(""After"");
        }
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