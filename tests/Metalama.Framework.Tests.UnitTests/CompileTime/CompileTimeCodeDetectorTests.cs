// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CompileTime;
using Metalama.Testing.UnitTesting;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CompileTime
{
    public sealed class CompileTimeCodeDetectorTests : UnitTestClass
    {
        [Fact]
        public void NotCompileTime()
        {
            var compilation = TestCompilationFactory.CreateCSharpCompilation( @"using Metalama.Framework.RunTime; namespace X { class Y {} } " );
            Assert.False( CompileTimeCodeFastDetector.HasCompileTimeCode( compilation.SyntaxTrees.Single().GetRoot() ) );
        }

        [Fact]
        public void EmptyFile()
        {
            var compilation = TestCompilationFactory.CreateCSharpCompilation( @"" );
            Assert.False( CompileTimeCodeFastDetector.HasCompileTimeCode( compilation.SyntaxTrees.Single().GetRoot() ) );
        }

        [Fact]
        public void InvalidFile()
        {
            var compilation = TestCompilationFactory.CreateCSharpCompilation(
                @"using Metalama.Framework.Aspects; namespace X class Y {} ",
                ignoreErrors: true );

            Assert.True( CompileTimeCodeFastDetector.HasCompileTimeCode( compilation.SyntaxTrees.Single().GetRoot() ) );
        }

        [Fact]
        public void TopLevelUsingMetalamaFramework()
        {
            var compilation = TestCompilationFactory.CreateCSharpCompilation( @"using Metalama.Framework; namespace X {class Y {} }" );
            Assert.True( CompileTimeCodeFastDetector.HasCompileTimeCode( compilation.SyntaxTrees.Single().GetRoot() ) );
        }
        
        [Fact]
        public void TopLevelUsingMetalamaFrameworkAspects()
        {
            var compilation = TestCompilationFactory.CreateCSharpCompilation( @"using Metalama.Framework.Aspects; namespace X {class Y {} }" );
            Assert.True( CompileTimeCodeFastDetector.HasCompileTimeCode( compilation.SyntaxTrees.Single().GetRoot() ) );
        }

        [Fact]
        public void TopLevelUsingMetalamaFrameworkProjects()
        {
            var compilation = TestCompilationFactory.CreateCSharpCompilation( @"using Metalama.Framework.Fabrics; namespace X {class Y {} }" );
            Assert.True( CompileTimeCodeFastDetector.HasCompileTimeCode( compilation.SyntaxTrees.Single().GetRoot() ) );
        }

        [Fact]
        public void Level1NamespaceUsing()
        {
            var compilation = TestCompilationFactory.CreateCSharpCompilation( @"namespace X { using Metalama.Framework.Aspects; class Y {} }" );
            Assert.True( CompileTimeCodeFastDetector.HasCompileTimeCode( compilation.SyntaxTrees.Single().GetRoot() ) );
        }

        [Fact]
        public void Level2NamespaceUsing()
        {
            var compilation = TestCompilationFactory.CreateCSharpCompilation( @"namespace X { namespace Y { using Metalama.Framework.Aspects; } }" );
            Assert.True( CompileTimeCodeFastDetector.HasCompileTimeCode( compilation.SyntaxTrees.Single().GetRoot() ) );
        }
    }
}