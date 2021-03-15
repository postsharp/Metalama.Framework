// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Linq;
using Caravela.Framework.Impl.Linking;
using Caravela.Framework.UnitTests.Linker.Helpers;
using Xunit;

namespace Caravela.Framework.UnitTests.Linker
{
    public class MethodOverrideSignatureTests : LinkerTestBase
    {
        [Fact]
        public void ReturnsVoid_NoParameter()
        {
            var code = @"
class T
{
    void Foo()
    {
    }

    [PseudoOverride(Foo, TestAspect)]
    void Foo_Override()
    {
        link(this.Foo());
    }
}
";

            var expectedCode = @"
class T
{
    void Foo()
    {
    }
}
";

            var linkerInput = CreateLinkerInput( code );
            var linker = new AspectLinker( linkerInput );
            var result = linker.ToResult();

            var transformedText = GetCleanCompilation(result.Compilation).SyntaxTrees.Single().GetNormalizedText();
            Assert.Equal( expectedCode.Trim(), transformedText );
        }

        [Fact]
        public void ReturnsVoid_IntParameter()
        {
            var code = @"
class T
{
    void Foo(int x)
    {
    }

    [PseudoOverride(Foo, TestAspect)]
    void Foo_Override(int x)
    {
        link(this.Foo(x));
    }
}
";

            var expectedCode = @"
class T
{
    void Foo(int x)
    {
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
        public void ReturnsInt_NoParameter()
        {
            var code = @"
class T
{
    int Foo()
    {
        return 42;
    }

    [PseudoOverride(Foo, TestAspect)]
    void Foo_Override()
    {
        return link(this.Foo());
    }
}
";
            var expectedCode = @"
class T
{
    int Foo()
    {
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

        [Fact]
        public void ReturnsInt_IntParameter()
        {
            var code = @"
class T
{
    int Foo(int x)
    {
        return x;
    }

    [PseudoOverride(Foo, TestAspect)]
    void Foo_Override(int x)
    {
        return link(this.Foo(x));
    }
}
";
            var expectedCode = @"
class T
{
    int Foo(int x)
    {
        return x;
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
