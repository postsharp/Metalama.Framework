// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Linking;
using Caravela.Framework.Tests.UnitTests.Linker.Helpers;
using System.Linq;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.Linker
{
    public class MethodIntroductionTests : LinkerTestBase
    {
        [Fact]
        public void ReturnsVoid_NoParameter_SimpleBody()
        {
            var code = @"
class T
{
    [PseudoIntroduction(TestAspect)]
    public void Foo()
    {
    }
}
";

            var expectedCode = @"
class T
{
    public void Foo()
    {
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
        public void ReturnsVoid_IntParameter_SimpleBody()
        {
            var code = @"
class T
{
    [PseudoIntroduction(TestAspect)]
    public void Foo(int x)
    {
    }
}
";

            var expectedCode = @"
class T
{
    public void Foo(int x)
    {
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
        public void ReturnsInt_NoParameter_SimpleBody()
        {
            var code = @"
class T
{
    [PseudoIntroduction(TestAspect)]
    public int Foo()
    {
        return 42;
    }
}
";

            var expectedCode = @"
class T
{
    public int Foo()
    {
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
        public void ReturnsInt_IntParameter_SimpleBody()
        {
            var code = @"
class T
{
    [PseudoIntroduction(TestAspect)]
    public int Foo(int x)
    {
        return 42;
    }
}
";

            var expectedCode = @"
class T
{
    public int Foo(int x)
    {
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