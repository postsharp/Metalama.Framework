// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline.Diff;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime
{
    public class SyntaxTreeComparisonTests
    {
        [Fact]
        public void SameTrees()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText( @"class C {}" );

            Assert.False( CompilationChangeTracker.IsDifferent( syntaxTree, syntaxTree ) );
        }

        [Fact]
        public void SameContent()
        {
            var syntaxTree1 = CSharpSyntaxTree.ParseText( @"class C {}" );
            var syntaxTree2 = CSharpSyntaxTree.ParseText( @"class C {}" );

            Assert.False( CompilationChangeTracker.IsDifferent( syntaxTree1, syntaxTree2 ) );
        }

        [Fact]
        public void ChangeInCommentLine()
        {
            var syntaxTree1 = CSharpSyntaxTree.ParseText( "// Comment 1\nclass C {}" );

            var syntaxTree2 = CSharpSyntaxTree.ParseText( "// Comment 2\nclass C {}" );

            Assert.False( CompilationChangeTracker.IsDifferent( syntaxTree1, syntaxTree2 ) );
        }

        [Fact]
        public void TypingInCommentBlock()
        {
            var syntaxTree1 = CSharpSyntaxTree.ParseText( "/* Comment */ class C {}" );

            var syntaxTree2 = CSharpSyntaxTree.ParseText( "/* Comment 2222222222 */ class C {}" );

            Assert.False( CompilationChangeTracker.IsDifferent( syntaxTree1, syntaxTree2 ) );
        }

        [Fact]
        public void DeletingInCommentBlock()
        {
            var syntaxTree1 = CSharpSyntaxTree.ParseText( "/* Comment 111111111111111111 */ class C {} " );

            var syntaxTree2 = CSharpSyntaxTree.ParseText( "/* Comment */ class C {} " );

            Assert.False( CompilationChangeTracker.IsDifferent( syntaxTree1, syntaxTree2 ) );
        }

        [Fact]
        public void CommentOutDeclaration()
        {
            var syntaxTree1 = CSharpSyntaxTree.ParseText( "class C {} " );

            var syntaxTree2 = CSharpSyntaxTree.ParseText( "// class C {}" );

            Assert.True( CompilationChangeTracker.IsDifferent( syntaxTree1, syntaxTree2 ) );
        }

        [Fact]
        public void UncommentDeclaration()
        {
            var syntaxTree1 = CSharpSyntaxTree.ParseText( "// class C {}" );

            var syntaxTree2 = CSharpSyntaxTree.ParseText( "class C {}" );

            Assert.True( CompilationChangeTracker.IsDifferent( syntaxTree1, syntaxTree2 ) );
        }

        [Fact]
        public void AddingWhitespace()
        {
            var syntaxTree1 = CSharpSyntaxTree.ParseText( "class C {}" );

            var syntaxTree2 = CSharpSyntaxTree.ParseText( "class C {  }" );

            Assert.False( CompilationChangeTracker.IsDifferent( syntaxTree1, syntaxTree2 ) );
        }

        [Fact]
        public void RemovingSomeWhitespace()
        {
            var syntaxTree1 = CSharpSyntaxTree.ParseText( "class C {   }" );

            var syntaxTree2 = CSharpSyntaxTree.ParseText( "class C { }" );

            Assert.False( CompilationChangeTracker.IsDifferent( syntaxTree1, syntaxTree2 ) );
        }

        [Fact]
        public void RemovingAllWhitespace()
        {
            var syntaxTree1 = CSharpSyntaxTree.ParseText( "class C {   }" );

            var syntaxTree2 = CSharpSyntaxTree.ParseText( "class C {}" );

            Assert.False( CompilationChangeTracker.IsDifferent( syntaxTree1, syntaxTree2 ) );
        }

        [Fact]
        public void RemovingRequiredWhitespace()
        {
            var syntaxTree1 = CSharpSyntaxTree.ParseText( "class C {}" );

            var syntaxTree2 = CSharpSyntaxTree.ParseText( "classC {}" );

            Assert.True( CompilationChangeTracker.IsDifferent( syntaxTree1, syntaxTree2 ) );
        }

        [Fact]
        public void ChangeInMethodBody()
        {
            var syntaxTree1 = CSharpSyntaxTree.ParseText( "class C { int M() { return 1; } }" );

            var syntaxTree2 = CSharpSyntaxTree.ParseText( "class C { int M() { return 2; } }" );

            Assert.False( CompilationChangeTracker.IsDifferent( syntaxTree1, syntaxTree2 ) );
        }

        [Fact]
        public void ChangeInMethodExpression()
        {
            var syntaxTree1 = CSharpSyntaxTree.ParseText( "class C { int M() => 1; } }" );

            var syntaxTree2 = CSharpSyntaxTree.ParseText( "class C { int M() => 2; } }" );

            Assert.False( CompilationChangeTracker.IsDifferent( syntaxTree1, syntaxTree2 ) );
        }

        [Fact]
        public void ChangeInMethodReturnType()
        {
            var syntaxTree1 = CSharpSyntaxTree.ParseText( "class C { int M() => 1; } }" );

            var syntaxTree2 = CSharpSyntaxTree.ParseText( "class C { long M() => 1; } }" );

            Assert.True( CompilationChangeTracker.IsDifferent( syntaxTree1, syntaxTree2 ) );
        }

        [Fact]
        public void ChangeInPropertyGetterBody()
        {
            var syntaxTree1 = CSharpSyntaxTree.ParseText( "class C { int M { get { return 1; } } }" );

            var syntaxTree2 = CSharpSyntaxTree.ParseText( "class C { int M { get { return 2; } } }" );

            Assert.False( CompilationChangeTracker.IsDifferent( syntaxTree1, syntaxTree2 ) );
        }

        [Fact]
        public void ChangeInPropertyGetterExpression()
        {
            var syntaxTree1 = CSharpSyntaxTree.ParseText( "class C { int M { get => 1; } }" );

            var syntaxTree2 = CSharpSyntaxTree.ParseText( "class C { int M { get => 2; } }" );

            Assert.False( CompilationChangeTracker.IsDifferent( syntaxTree1, syntaxTree2 ) );
        }

        [Fact]
        public void ChangeInPropertyExpression()
        {
            var syntaxTree1 = CSharpSyntaxTree.ParseText( "class C { int M => 1; }" );

            var syntaxTree2 = CSharpSyntaxTree.ParseText( "class C { int M => 2; }" );

            Assert.False( CompilationChangeTracker.IsDifferent( syntaxTree1, syntaxTree2 ) );
        }

        [Fact]
        public void ChangeInPropertyInitializer()
        {
            var syntaxTree1 = CSharpSyntaxTree.ParseText( "class C { int M { get; } = 1; }" );

            var syntaxTree2 = CSharpSyntaxTree.ParseText( "class C { int M { get; } = 2; }" );

            Assert.False( CompilationChangeTracker.IsDifferent( syntaxTree1, syntaxTree2 ) );
        }

        [Fact]
        public void ChangeInFieldInitializer()
        {
            var syntaxTree1 = CSharpSyntaxTree.ParseText( "class C { int M = 1; }" );

            var syntaxTree2 = CSharpSyntaxTree.ParseText( "class C { int M = 2; }" );

            Assert.False( CompilationChangeTracker.IsDifferent( syntaxTree1, syntaxTree2 ) );
        }

        [Fact( Skip = "Adding aspects to local functions is not yet supported." )]
        public void AddLocalFunction()
        {
            var syntaxTree1 = CSharpSyntaxTree.ParseText( "class C { int M() {} }" );

            var syntaxTree2 = CSharpSyntaxTree.ParseText( "class C { int M() { void N() {] } }" );

            Assert.True( CompilationChangeTracker.IsDifferent( syntaxTree1, syntaxTree2 ) );
        }

        [Fact]
        public void ChangeInCompileTimeCode()
        {
            var syntaxTree1 = CSharpSyntaxTree.ParseText( "using Metalama.Framework.Aspects; class C { int M { get => 1; } }" );

            var syntaxTree2 = CSharpSyntaxTree.ParseText( "using Metalama.Framework.Aspects; class C { int M { get => 2; } }" );

            Assert.True( CompilationChangeTracker.IsDifferent( syntaxTree1, syntaxTree2 ) );
        }

        [Fact]
        public void AddPartial()
        {
            var syntaxTree1 = CSharpSyntaxTree.ParseText( "class C {}" );

            var syntaxTree2 = CSharpSyntaxTree.ParseText( "partial class C {}" );

            Assert.True( CompilationChangeTracker.IsDifferent( syntaxTree1, syntaxTree2 ) );
        }

        [Fact]
        public void RemovePartial()
        {
            var syntaxTree1 = CSharpSyntaxTree.ParseText( "partial class C {}" );

            var syntaxTree2 = CSharpSyntaxTree.ParseText( "class C {}" );

            Assert.True( CompilationChangeTracker.IsDifferent( syntaxTree1, syntaxTree2 ) );
        }
    }
}