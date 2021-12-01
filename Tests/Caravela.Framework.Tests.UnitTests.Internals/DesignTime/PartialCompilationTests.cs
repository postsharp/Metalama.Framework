// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.TestFramework;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests.DesignTime
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
            var compilationModel1 = CompilationModel.CreateInitialInstance( nullProject, compilation, syntaxTree1 );
            Assert.Single( compilationModel1.Types.Select( t => t.Name ), "Class1" );

            // Tests for Class3. The Types collection must contain the base class.
            var syntaxTree3 = compilation.SyntaxTrees.Single( t => t.FilePath == "Class3.cs" );
            var compilationModel3 = CompilationModel.CreateInitialInstance( nullProject, compilation, syntaxTree3 );
            Assert.Equal( new[] { "Class2", "Class3" }, compilationModel3.Types.Select( t => t.Name ).OrderBy( t => t ) );

            // Tests for Class4: the Types collection must contain the base class and the interfaces.
            var semanticModel4 = compilation.SyntaxTrees.Single( t => t.FilePath == "Class4.cs" );
            var compilationModel4 = CompilationModel.CreateInitialInstance( nullProject, compilation, semanticModel4 );

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
            var compilationModel1 = CompilationModel.CreateInitialInstance( nullProject, compilation, syntaxTree1 );

            var ns1 = compilationModel1.GlobalNamespace.Namespaces.Single();

            Assert.Equal( new[] { "Class1", "Class2" }, ns1.Types.Select( t => t.Name ).OrderBy( t => t ) );
        }
    }
}