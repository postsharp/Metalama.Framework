// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Introspection;
using Metalama.Framework.Workspaces;
using Metalama.Testing.UnitTesting;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

#pragma warning disable VSTHRD200

namespace Metalama.Framework.Tests.Workspaces
{
    public sealed class WorkspaceTests : UnitTestClass
    {
        private static readonly ImmutableDictionary<string, string> _buildProperties = ImmutableDictionary<string, string>.Empty
            .Add( "DOTNET_ROOT_X64", "" )
            .Add( "MSBUILD_EXE_PATH", "" )
            .Add( "MSBuildSDKsPath", "" );

        [Fact]
        public async Task LoadProjectSingleTarget()

        {
            using var testContext = this.CreateTestContext();

            var projectPath = Path.Combine( testContext.BaseDirectory, "Project.csproj" );
            var codePath = Path.Combine( testContext.BaseDirectory, "Code.cs" );

            await File.WriteAllTextAsync(
                projectPath,
                @"
<Project Sdk=""Microsoft.NET.Sdk"">
    <PropertyGroup>
        <TargetFramework>netstandard2.0</TargetFramework>
    </PropertyGroup>
</Project>
" );

            await File.WriteAllTextAsync( codePath, "class MyClass {}" );

            var workspaceCollection = new WorkspaceCollection( testContext.ServiceProvider ) { IgnoreLoadErrors = true };

            using var workspace = await workspaceCollection.LoadAsync( [projectPath], _buildProperties );

            Assert.Single( workspace.Projects );
            Assert.Single( workspace.Projects[0].Types );

            Assert.False( workspace.Projects[0].IsMetalamaEnabled );
            Assert.Same( workspace.Projects[0].SourceCode.Compilations[0], workspace.Projects[0].TransformedCode.Compilations[0] );
        }

        [Fact]
        public async Task LoadProjectMultiTarget()
        {
            using var testContext = this.CreateTestContext();

            var projectPath = Path.Combine( testContext.BaseDirectory, "Project.csproj" );
            var codePath = Path.Combine( testContext.BaseDirectory, "Code.cs" );

            await File.WriteAllTextAsync(
                projectPath,
                @"
<Project Sdk=""Microsoft.NET.Sdk"">
    <PropertyGroup>
        <TargetFrameworks>netstandard2.0;net6.0</TargetFrameworks>
    </PropertyGroup>
</Project>
" );

            await File.WriteAllTextAsync( codePath, "class MyClass {}" );

            var workspaceCollection = new WorkspaceCollection( testContext.ServiceProvider ) { IgnoreLoadErrors = true };

            using var workspace = await workspaceCollection.LoadAsync( projectPath );

            Assert.Equal( 2, workspace.Projects.Length );
            Assert.Equal( 2, workspace.SourceCode.Types.Length );
        }

        [Fact]
        public async Task IgnoreErrors()
        {
            using var testContext = this.CreateTestContext();

            var projectPath = await CreateMetalamaEnabledProjectAsync(
                testContext,
                "using Metalama.Framework.Aspects;  [CompileTime] class MyClass /* Intentional syntax error in compile-time code .*/ " );

            var workspaceCollection = new WorkspaceCollection( testContext.ServiceProvider ) { IgnoreLoadErrors = true };

            using var workspace = await workspaceCollection.LoadAsync( [projectPath], _buildProperties );

            Assert.Throws<CompilationFailedException>( () => workspace.AspectInstances );
            Assert.Throws<CompilationFailedException>( () => workspace.AspectClasses );
            Assert.Throws<CompilationFailedException>( () => workspace.TransformedCode );
            Assert.Throws<CompilationFailedException>( () => workspace.Transformations );
            Assert.Throws<CompilationFailedException>( () => workspace.Advice );

            // Changing the option in the workspace should have effect deep inside the code model.
            workspace.WithIgnoreErrors();

            Assert.Empty( workspace.AspectInstances );
            Assert.Empty( workspace.AspectClasses );
            Assert.Empty( workspace.Transformations );
            Assert.Empty( workspace.Advice );
        }

        private static async Task<string> CreateMetalamaEnabledProjectAsync(
            TestContext testContext,
            string code,
            string? projectName = null,
            string[]? dependentProjectPaths = null )
        {
            dependentProjectPaths ??= [];
            projectName ??= "Foo";

            var compilationForReferences = TestCompilationFactory.CreateCSharpCompilation( "" );

            var libraryReferences = compilationForReferences.ExternalReferences.OfType<PortableExecutableReference>()
                .Select( r => $"<Reference Include=\"{r.FilePath}\" />" );

            var projectReferences = dependentProjectPaths.Select( r => $"<ProjectReference Include=\"{r}\" />" );

            var projectDirectory = Path.Combine( testContext.BaseDirectory, projectName );
            Directory.CreateDirectory( projectDirectory );
            var projectPath = Path.Combine( projectDirectory, $"{projectName}.csproj" );
            var codePath = Path.Combine( projectDirectory, $"Code.cs" );

            await File.WriteAllTextAsync(
                projectPath,
                $@"
<Project Sdk=""Microsoft.NET.Sdk"">
    <PropertyGroup>
        <TargetFramework>netstandard2.0</TargetFramework>
        <DefineConstants>METALAMA</DefineConstants>
        <Nullable>enable</Nullable>
    </PropertyGroup>
    <ItemGroup>
        {string.Join( Environment.NewLine, libraryReferences )}
        {string.Join( Environment.NewLine, projectReferences )}
    </ItemGroup>
</Project>
" );

            await File.WriteAllTextAsync( codePath, code );

            return projectPath;
        }

        [Fact]
        public async Task MetalamaEnabled()
        {
            using var testContext = this.CreateTestContext();

            var projectPath = await CreateMetalamaEnabledProjectAsync(
                testContext,
                @"
using Metalama.Framework.Advising; 
using Metalama.Framework.Aspects; 

class MyAspect : TypeAspect
{
   [Introduce] void IntroducedMethod(){}
}
[MyAspect]
class MyClass {}" );

            var workspaceCollection = new WorkspaceCollection( testContext.ServiceProvider ) { IgnoreLoadErrors = true };

            using var workspace = await workspaceCollection.LoadAsync( projectPath );

            Assert.True( workspace.IsMetalamaEnabled );
            Assert.Equal( 3, workspace.AspectClasses.Length );
            Assert.Single( workspace.AspectInstances );
            var targetFramework = Assert.Single( workspace.SourceCode.TargetFrameworks );
            Assert.Equal( "netstandard2.0", targetFramework );
        }

        [Fact]
        public async Task DeclarationReferences()
        {
            const string code = """
                                class A;
                                class B : A 
                                {
                                  A f;
                                } 
                                class C : System.Collections.Generic.List<int> 
                                {
                                  int f;
                                } 
                                """;

            using var testContext = this.CreateTestContext();

            var projectPath = await CreateMetalamaEnabledProjectAsync(
                testContext,
                code );

            var workspaceCollection = new WorkspaceCollection( testContext.ServiceProvider ){ IgnoreLoadErrors = true };

            using var workspace = await workspaceCollection.LoadAsync( projectPath );
            var typeA = workspace.Projects.Single().Types.Single( t => t.Name == "A" );

            var references = GetReferences( typeA );

            Assert.Equal( ["B", "B.f"], references );

            static IEnumerable<string> GetReferences( IDeclaration d )
                => d.GetInboundReferences().Select( x => x.OriginDeclaration.ToDisplayString() ).OrderBy( x => x );
        }

        [Fact]
        public async Task SyntaxReferences()
        {
            const string code = """
                                class A { public static void M() {} }
                                class B : A 
                                {
                                  A f;
                                  void M() => A.M();
                                } 
                                class C : System.Collections.Generic.List<int> 
                                {
                                  int f;
                                } 
                                """;

            using var testContext = this.CreateTestContext();

            var projectPath = await CreateMetalamaEnabledProjectAsync(
                testContext,
                code );

            var workspaceCollection = new WorkspaceCollection( testContext.ServiceProvider ){ IgnoreLoadErrors = true };

            using var workspace = await workspaceCollection.LoadAsync( projectPath );

            var typeA = workspace.Projects.Single().Types.Single( t => t.Name == "A" );

            Assert.Equal( ["B.f[MemberType]", "B.M()[Invocation]", "B[BaseType]"], GetReferences( typeA ) );

            static IEnumerable<string> GetReferences( IDeclaration d )
                => d.GetInboundReferences()
                    .Select( x => x.OriginDeclaration.ToDisplayString() + "[" + string.Join( ",", x.References.Select( x => x.Kinds ) ) + "]" )
                    .OrderBy( x => x )
                    .ToArray();
        }

        [Fact]
        public async Task CrossProjectReferences()
        {
            const string code1 = """
                                 public class A { public static void M() {} }
                                 """;

            const string code2 = """
                                 class B : A 
                                 {
                                   A f;
                                   void M() => A.M();
                                 } 
                                 class C : System.Collections.Generic.List<int> 
                                 {
                                   int f;
                                 } 
                                 """;

            using var testContext = this.CreateTestContext();

            var projectPath1 = await CreateMetalamaEnabledProjectAsync(
                testContext,
                code1,
                "Project1" );

            var projectPath2 = await CreateMetalamaEnabledProjectAsync(
                testContext,
                code2,
                "Project2",
                [projectPath1] );

            var workspaceCollection = new WorkspaceCollection( testContext.ServiceProvider ) { IgnoreLoadErrors = true };

            using var workspace = await workspaceCollection.LoadAsync( projectPath1, projectPath2 );
            var typesA = workspace.SourceCode.Types.Where( t => t.Name == "A" ).ToArray();

            Assert.Single( typesA );

            var references = GetReferences( typesA.Single() );

            Assert.Equal( ["'B.f' -> 'A'", "'B.M()' -> 'A.M()'", "'B' -> 'A'"], references );

            static IEnumerable<string> GetReferences( IDeclaration d ) => d.GetInboundReferences().Select( x => x.ToString() ).OrderBy( x => x );
        }
    }
}