// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Tests.UnitTests.Linker.Helpers;
using System.Linq;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Linker
{
    public class EventInlinerTests : LinkerTestBase
    {
        [Fact]
        public void AddAssignment()
        {
            var code = @"
class T
{
    delegate void Handler();
    void Test(string s)
    {
    }

    Handler field;

    event Handler Foo
    {
        add 
        {
            Test(""Original"");
            this.field += value;
        }
        remove {}
    }

    [PseudoOverride(Foo, TestAspect)]
    event Handler Foo_Override
    {
        add 
        {
            Test(""Before"");
            link(this.Foo.add, inline) += value;
            Test(""After"");
        }
        remove {}
    }
}
";

            var expectedCode = @"
class T
{
    delegate void Handler();
    void Test(string s)
    {
    }

    Handler field;
    event Handler Foo
    {
        add
        {
            Test(""Before"");
            Test(""Original"");
            this.field += value;
            Test(""After"");
        }

        remove
        {
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
        public void RemoveAssignment()
        {
            var code = @"
class T
{
    delegate void Handler();
    void Test(string s)
    {
    }

    Handler field;

    event Handler Foo
    {
        add {}
        remove 
        {
            Test(""Original"");
            this.field -= value;
        }
    }

    [PseudoOverride(Foo, TestAspect)]
    event Handler Foo_Override
    {
        add {}                
        remove  
        {
            Test(""Before"");
            link(this.Foo.remove, inline) -= value;
            Test(""After"");
        }
    }
}
";

            var expectedCode = @"
class T
{
    delegate void Handler();
    void Test(string s)
    {
    }

    Handler field;
    event Handler Foo
    {
        add
        {
        }

        remove
        {
            Test(""Before"");
            Test(""Original"");
            this.field -= value;
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