// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Tests.UnitTests.Linker.Helpers;
using System.Linq;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Linker
{
    public class PropertyOverrideBasicTests : LinkerTestBase
    {
        [Fact]
        public void BlockBodies()
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
            return 42;
        }

        set
        {
        }
    }

    [PseudoOverride(Foo, TestAspect)]
    int Foo_Override
    {
        get
        {
            Test(""Get"");
            return link(this.Foo.get, inline);
        }
        set
        {
            Test(""Set"");
            link(this.Foo.set, inline) = value;
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
            Test(""Get"");
            return 42;
        }

        set
        {
            Test(""Set"");
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
        public void ExpressionBodies()
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
        get => this._foo;
        set => this._foo = value;
    }

    [PseudoOverride(Foo, TestAspect)]
    int Foo_Override
    {
        get
        {
            Test(""Get"");
            return link(this.Foo.get, inline);
        }
        set
        {
            Test(""Set"");
            link(this.Foo.set, inline) = value;
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

    int _foo;
    int Foo
    {
        get
        {
            Test(""Get"");
            return this._foo;
        }

        set
        {
            Test(""Set"");
            this._foo = value;
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
        public void AutoProperty()
        {
            var code = @"
class T
{
    void Test(string s)
    {
    }

    int Foo { get; set; }

    [PseudoOverride(Foo, TestAspect)]
    int Foo_Override
    {
        get
        {
            Test(""Get"");
            return link(this.Foo.get, inline);
        }
        set
        {
            Test(""Set"");
            link(this.Foo.set, inline) = value;
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

    private int _foo;
    int Foo
    {
        get
        {
            Test(""Get"");
            return this._foo;
        }

        set
        {
            Test(""Set"");
            this._foo = value;
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