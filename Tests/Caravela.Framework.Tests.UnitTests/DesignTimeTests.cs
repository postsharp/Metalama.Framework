// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.DesignTime;
using Caravela.Framework.Impl.DesignTime.UserDiagnostics;
using Caravela.TestFramework;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Xunit;

namespace Caravela.Framework.Tests.UnitTests
{
    public class DesignTimeTests : TestBase
    {
        [Fact]
        public void CreatePartialCompilationModel()
        {
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

            // Tests for Class1.
            var syntaxTree1 = compilation.SyntaxTrees.Single( t => t.FilePath == "Class1.cs" );
            var compilationModel1 = CompilationModel.CreateInitialInstance( compilation, syntaxTree1 );
            Assert.Single( compilationModel1.DeclaredTypes.Select( t => t.Name ), "Class1" );

            // Tests for Class3. The Types collection must contain the base class.
            var syntaxTree3 = compilation.SyntaxTrees.Single( t => t.FilePath == "Class3.cs" );
            var compilationModel3 = CompilationModel.CreateInitialInstance( compilation, syntaxTree3 );
            Assert.Equal( new[] { "Class2", "Class3" }, compilationModel3.DeclaredTypes.Select( t => t.Name ).OrderBy( t => t ) );

            // Tests for Class4: the Types collection must contain the base class and the interfaces.
            var semanticModel4 = compilation.SyntaxTrees.Single( t => t.FilePath == "Class4.cs" );
            var compilationModel4 = CompilationModel.CreateInitialInstance( compilation, semanticModel4 );

            Assert.Equal(
                new[] { "Class2", "Class3", "Class4", "Interface1", "Interface2", "Interface3" },
                compilationModel4.DeclaredTypes.Select( t => t.Name ).OrderBy( t => t ) );
        }

        [Fact]
        public void NoAspect()
        {
            // Test that we can initialize the pipeline with a different compilation than the one with which we execute it.

            var code = new Dictionary<string, string> { ["Class1.cs"] = "public class Class1 { }", ["Class2.cs"] = "public class Class2 { }" };

            var compilation = CreateCSharpCompilation( code );

            using var buildOptions = new TestProjectOptions();
            using var domain = new UnloadableCompileTimeDomain();
            DesignTimeAspectPipeline pipeline = new( buildOptions, domain );
            var syntaxTree1 = compilation.SyntaxTrees.Single( t => t.FilePath == "Class1.cs" );
            pipeline.Execute( PartialCompilation.CreatePartial( compilation, syntaxTree1 ), CancellationToken.None );

            var syntaxTree2 = compilation.SyntaxTrees.Single( t => t.FilePath == "Class2.cs" );
            pipeline.Execute( PartialCompilation.CreatePartial( compilation, syntaxTree2 ), CancellationToken.None );
        }

        [Fact]
        public void InitializePipelineWithDifferentCompilation()
        {
            // Test that we can initialize the pipeline with a different compilation than the one with which we execute it.

            var code = new Dictionary<string, string>
            {
                ["Aspect.cs"] =
                    "public class Aspect : Caravela.Framework.Aspects.OverrideMethodAspect { public override dynamic OverrideMethod() { return null; } }",
                ["Class1.cs"] = "public class Class1 { }",
                ["Class2.cs"] = "public class Class2 { [Aspect]  void Method() {} }"
            };

            var compilation = CreateCSharpCompilation( code );

            using var buildOptions = new TestProjectOptions();
            using var domain = new UnloadableCompileTimeDomain();
            DesignTimeAspectPipeline pipeline = new( buildOptions, domain );
            var syntaxTree1 = compilation.SyntaxTrees.Single( t => t.FilePath == "Class1.cs" );
            pipeline.Execute( PartialCompilation.CreatePartial( compilation, syntaxTree1 ), CancellationToken.None );

            var syntaxTree2 = compilation.SyntaxTrees.Single( t => t.FilePath == "Class2.cs" );
            pipeline.Execute( PartialCompilation.CreatePartial( compilation, syntaxTree2 ), CancellationToken.None );
        }

        [Fact]
        public void DiagnosticUserProfileSerialization()
        {
            UserDiagnosticRegistrationFile file = new();
            var originalDiagnostic = new UserDiagnosticRegistration( "MY001", DiagnosticSeverity.Error, "Category", "Title" );
            file.Diagnostics.Add( "MY001", originalDiagnostic );
            file.Suppressions.Add( "MY001" );

            StringWriter stringWriter = new();
            file.Write( stringWriter );
            var roundtrip = UserDiagnosticRegistrationFile.ReadContent( stringWriter.ToString() );

            Assert.Contains( "MY001", roundtrip.Suppressions );
            Assert.Contains( "MY001", roundtrip.Diagnostics.Keys );
        }
    }
}