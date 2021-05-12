// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CompileTime;
using System.Linq;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.CompileTime
{
    public class CompileTimeCodeDetectorTests : TestBase
    {
        [Fact]
        public void NotCompileTime()
        {
            var compilation = CreateCSharpCompilation( @"using Caravela.Framework; namespace X { class Y {} } " );
            Assert.False( CompileTimeCodeDetector.HasCompileTimeCode( compilation.SyntaxTrees.Single().GetRoot() ) );
        }

        [Fact]
        public void EmptyFile()
        {
            var compilation = CreateCSharpCompilation( @"" );
            Assert.False( CompileTimeCodeDetector.HasCompileTimeCode( compilation.SyntaxTrees.Single().GetRoot() ) );
        }

        [Fact]
        public void InvalidFile()
        {
            var compilation = CreateCSharpCompilation( @"using Caravela.Framework; namespace X class Y {} ", ignoreErrors: true );
            Assert.False( CompileTimeCodeDetector.HasCompileTimeCode( compilation.SyntaxTrees.Single().GetRoot() ) );
        }

        [Fact]
        public void TopLevelUsingCaravelaFrameworkAspects()
        {
            var compilation = CreateCSharpCompilation( @"using Caravela.Framework.Aspects; namespace X {class Y {} }" );
            Assert.True( CompileTimeCodeDetector.HasCompileTimeCode( compilation.SyntaxTrees.Single().GetRoot() ) );
        }

        [Fact]
        public void TopLevelUsingCaravelaFrameworkProjects()
        {
            var compilation = CreateCSharpCompilation( @"using Caravela.Framework.Project; namespace X {class Y {} }" );
            Assert.True( CompileTimeCodeDetector.HasCompileTimeCode( compilation.SyntaxTrees.Single().GetRoot() ) );
        }

        [Fact]
        public void Level1NamespaceUsing()
        {
            var compilation = CreateCSharpCompilation( @"namespace X { using Caravela.Framework.Aspects; class Y {} }" );
            Assert.True( CompileTimeCodeDetector.HasCompileTimeCode( compilation.SyntaxTrees.Single().GetRoot() ) );
        }

        [Fact]
        public void Level2NamespaceUsing()
        {
            var compilation = CreateCSharpCompilation( @"namespace X { namespace Y { using Caravela.Framework.Aspects; } }" );
            Assert.True( CompileTimeCodeDetector.HasCompileTimeCode( compilation.SyntaxTrees.Single().GetRoot() ) );
        }
    }
}