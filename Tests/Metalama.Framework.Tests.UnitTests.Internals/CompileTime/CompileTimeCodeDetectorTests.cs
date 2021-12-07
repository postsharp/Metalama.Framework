// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl.CompileTime;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.CompileTime
{
    public class CompileTimeCodeDetectorTests : TestBase
    {
        [Fact]
        public void NotCompileTime()
        {
            var compilation = CreateCSharpCompilation( @"using Metalama.Framework.RunTime; namespace X { class Y {} } " );
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
            var compilation = CreateCSharpCompilation( @"using Metalama.Framework.Aspects; namespace X class Y {} ", ignoreErrors: true );
            Assert.True( CompileTimeCodeDetector.HasCompileTimeCode( compilation.SyntaxTrees.Single().GetRoot() ) );
        }

        [Fact]
        public void TopLevelUsingMetalamaFrameworkAspects()
        {
            var compilation = CreateCSharpCompilation( @"using Metalama.Framework.Aspects; namespace X {class Y {} }" );
            Assert.True( CompileTimeCodeDetector.HasCompileTimeCode( compilation.SyntaxTrees.Single().GetRoot() ) );
        }

        [Fact]
        public void TopLevelUsingMetalamaFrameworkProjects()
        {
            var compilation = CreateCSharpCompilation( @"using Metalama.Framework.Fabrics; namespace X {class Y {} }" );
            Assert.True( CompileTimeCodeDetector.HasCompileTimeCode( compilation.SyntaxTrees.Single().GetRoot() ) );
        }

        [Fact]
        public void Level1NamespaceUsing()
        {
            var compilation = CreateCSharpCompilation( @"namespace X { using Metalama.Framework.Aspects; class Y {} }" );
            Assert.True( CompileTimeCodeDetector.HasCompileTimeCode( compilation.SyntaxTrees.Single().GetRoot() ) );
        }

        [Fact]
        public void Level2NamespaceUsing()
        {
            var compilation = CreateCSharpCompilation( @"namespace X { namespace Y { using Metalama.Framework.Aspects; } }" );
            Assert.True( CompileTimeCodeDetector.HasCompileTimeCode( compilation.SyntaxTrees.Single().GetRoot() ) );
        }
    }
}