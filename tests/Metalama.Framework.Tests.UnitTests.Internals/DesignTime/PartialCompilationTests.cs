// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Testing;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.TestFramework;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.DesignTime
{
    public class PartialCompilationTests : TestBase
    {
        [Fact]
        public void Bug28733()
        {
            using var testContext = this.CreateTestContext();

            var code = new Dictionary<string, string> { ["Class1.cs"] = "class Class1 { class Nested {} }" };

            var compilation = CreateCSharpCompilation( code );

            using var projectOptions = new TestProjectOptions();
            using var domain = new UnloadableCompileTimeDomain();
            var syntaxTree1 = compilation.SyntaxTrees.Single();
            var partialCompilation = PartialCompilation.CreatePartial( compilation, syntaxTree1 );

            // Under bug 28733, the following line would throw
            // `AssertionFailedException: The item Class1.Nested of type NonErrorNamedTypeSymbol has been visited twice.`
            _ = CompilationModel.CreateInitialInstance( new NullProject( testContext.ServiceProvider ), partialCompilation );
        }

        [Fact]
        public void TypeClosure()
        {
            using var testContext = this.CreateTestContext();

            var code = new Dictionary<string, string>
            {
                ["Class1.cs"] = "public class Class1 { }",
                ["Class2.cs"] = "public class Class2 { }",
                ["Class3.cs"] = "public class Class3 : Class2 { }",
                ["Interface1.cs"] = "public interface Interface1 { }",
                ["Interface2.cs"] = "public interface Interface2 : Interface1 { }",
                ["Interface3.cs"] = "public interface Interface3 : Interface2 { }",
                ["Class4.cs"] = "public class Class4 : Class3, Interface3 { }"
            };

            var compilation = CreateCSharpCompilation( code );
            var nullProject = new NullProject( testContext.ServiceProvider );

            // Tests for Class1.
            var syntaxTree1 = compilation.SyntaxTrees.Single( t => t.FilePath == "Class1.cs" );
            var compilationModel1 = CompilationModel.CreateInitialInstance( nullProject, PartialCompilation.CreatePartial( compilation, syntaxTree1 ));
            Assert.Single( compilationModel1.Types.Select( t => t.Name ), "Class1" );

            // Tests for Class3. The Types collection must contain the base class.
            var syntaxTree3 = compilation.SyntaxTrees.Single( t => t.FilePath == "Class3.cs" );
            var compilationModel3 = CompilationModel.CreateInitialInstance( nullProject, PartialCompilation.CreatePartial(compilation, syntaxTree3 ));
            Assert.Equal( new[] { "Class2", "Class3" }, compilationModel3.Types.Select( t => t.Name ).OrderBy( t => t ) );

            // Tests for Class4: the Types collection must contain the base class and the interfaces.
            var semanticModel4 = compilation.SyntaxTrees.Single( t => t.FilePath == "Class4.cs" );
            var compilationModel4 = CompilationModel.CreateInitialInstance( nullProject,PartialCompilation.CreatePartial( compilation, semanticModel4) );

            Assert.Equal(
                new[] { "Class2", "Class3", "Class4", "Interface1", "Interface2", "Interface3" },
                compilationModel4.Types.Select( t => t.Name ).OrderBy( t => t ) );
        }

        [Fact]
        public void Namespaces()
        {
            using var testContext = this.CreateTestContext();

            var code = new Dictionary<string, string>
            {
                ["Class1.cs"] = "namespace Ns1 { public class Class1 { } }",
                ["Class2.cs"] = "namespace Ns1 { public class Class2 : Class1 { } }",
                ["Class3.cs"] = "namespace Ns2 { public class Class3 { } }",
                ["Class4.cs"] = "namespace Ns1 { public class Class4 { } }"
            };

            var compilation = CreateCSharpCompilation( code );
            var nullProject = new NullProject( testContext.ServiceProvider );

            var syntaxTree1 = compilation.SyntaxTrees.Single( t => t.FilePath == "Class2.cs" );
            var compilationModel1 = CompilationModel.CreateInitialInstance( nullProject, PartialCompilation.CreatePartial(compilation, syntaxTree1) );

            var ns1 = compilationModel1.GlobalNamespace.Namespaces.Single();

            Assert.Equal( new[] { "Class1", "Class2" }, ns1.Types.Select( t => t.Name ).OrderBy( t => t ) );
        }

        [Fact]
        public void SyntaxTreeWithoutType()
        {
            using var testContext = this.CreateTestContext();

            var code = new Dictionary<string, string> { ["Class1.cs"] = "/* Intentionally empty */" };

            var compilation = CreateCSharpCompilation( code );
            var partialCompilation = PartialCompilation.CreatePartial( compilation, compilation.SyntaxTrees[0] );
            Assert.Single( partialCompilation.SyntaxTrees );
        }

        [Fact]
        public void SeveralModifications_Partial()
        {
            var code = new Dictionary<string, string> { ["Class1.cs"] = "/* Intentionally empty */" };

            var compilation = CreateCSharpCompilation( code );

            ApplySeveralModifications( PartialCompilation.CreatePartial( compilation, compilation.SyntaxTrees[0] ) );
        }

        private static void ApplySeveralModifications( PartialCompilation partialCompilation1 )
        {
            var initialCompilation = partialCompilation1.InitialCompilation;

            // Test the initial compilation.
            const string path1 = "1.cs";
            Assert.Single( partialCompilation1.SyntaxTrees );
            Assert.Empty( partialCompilation1.ModifiedSyntaxTrees );

            // Add a syntax tree.
            var partialCompilation2 = (PartialCompilation) partialCompilation1.AddSyntaxTrees( CSharpSyntaxTree.ParseText( "", path: path1 ) );
            Assert.Equal( 2, partialCompilation2.SyntaxTrees.Count );
            Assert.Single( partialCompilation2.ModifiedSyntaxTrees );
            Assert.Same( initialCompilation, partialCompilation2.InitialCompilation );
            Assert.Null( partialCompilation2.ModifiedSyntaxTrees[path1].OldTree );

            // Add a second syntax tree.
            var partialCompilation3 = (PartialCompilation) partialCompilation2.AddSyntaxTrees( CSharpSyntaxTree.ParseText( "", path: "2.cs" ) );
            Assert.Equal( 3, partialCompilation3.SyntaxTrees.Count );
            Assert.Equal( 2, partialCompilation3.ModifiedSyntaxTrees.Count );
            Assert.Null( partialCompilation3.ModifiedSyntaxTrees[path1].OldTree );
            Assert.Same( initialCompilation, partialCompilation3.InitialCompilation );

            // Modify syntax trees.
            var partialCompilation4 = (PartialCompilation) partialCompilation3.RewriteSyntaxTrees( new Rewriter() );
            Assert.Equal( 3, partialCompilation4.SyntaxTrees.Count );
            Assert.Equal( 3, partialCompilation4.ModifiedSyntaxTrees.Count );
            Assert.Null( partialCompilation4.ModifiedSyntaxTrees[path1].OldTree );
            Assert.Same( initialCompilation, partialCompilation4.InitialCompilation );
        }

        [Fact]
        public void SeveralModifications_Complete()
        {
            var code = new Dictionary<string, string> { ["Class1.cs"] = "/* Intentionally empty */" };

            var compilation = CreateCSharpCompilation( code );

            ApplySeveralModifications( PartialCompilation.CreateComplete( compilation ) );
        }

        private class Rewriter : SafeSyntaxRewriter
        {
            // Apply some arbitrary transformation.
            protected override SyntaxNode? VisitCore( SyntaxNode? node ) => base.VisitCore( node )!.WithTrailingTrivia( SyntaxFactory.Space );
        }
    }
}