// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Tests.UnitTests.Linker.Helpers;
using System.Linq;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Linker
{
    public class LinkingTests : LinkerTestBase
    {
        [Fact]
        public void ThisExpression_Original_NoTransform()
        {
            var code = @"
class T
{
    void Foo()
    {
    }

    void Bar()
    {
    }

    [PseudoOverride(Bar, TestAspect)]
    void Bar_Override()
    {
        link(this.Foo, original)();
    }
}
";

            var expectedCode = @"
class T
{
    void Foo()
    {
    }

    void Bar()
    {
        this.Foo();
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
        public void ThisExpression_Final_NoTransform()
        {
            var code = @"
class T
{
    void Foo()
    {
    }

    void Bar()
    {
    }

    [PseudoOverride(Bar, TestAspect)]
    void Bar_Override()
    {
        link(this.Foo, final)();
    }
}
";

            var expectedCode = @"
class T
{
    void Foo()
    {
    }

    void Bar()
    {
        this.Foo();
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
        public void BaseExpression_Original_NoTransform()
        {
            var code = @"
class S
{
    protected void Foo()
    {
    }
}

class T : S
{
    void Bar()
    {
    }

    [PseudoOverride(Bar, TestAspect)]
    void Bar_Override()
    {
        link(base.Foo, original)();
    }
}
";

            var expectedCode = @"
class S
{
    protected void Foo()
    {
    }
}

class T : S
{
    void Bar()
    {
        base.Foo();
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
        public void BaseExpression_Final_NoTransform()
        {
            var code = @"
class S
{
    protected void Foo()
    {
    }
}

class T : S
{
    void Bar()
    {
    }

    [PseudoOverride(Bar, TestAspect)]
    void Bar_Override()
    {
        link(base.Foo, final)();
    }
}
";

            var expectedCode = @"
class S
{
    protected void Foo()
    {
    }
}

class T : S
{
    void Bar()
    {
        base.Foo();
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