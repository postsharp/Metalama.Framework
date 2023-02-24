// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Introspection;
using Metalama.Framework.Workspaces;
using Metalama.Testing.UnitTesting;
using Microsoft.CodeAnalysis;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

#pragma warning disable VSTHRD200

namespace Metalama.Framework.Tests.Workspaces
{
    public sealed class WorkspaceTests : UnitTestClass
    {
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

            var workspaceCollection = new WorkspaceCollection( testContext.ServiceProvider );

            using var workspace = await workspaceCollection.LoadAsync( projectPath );

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

            var workspaceCollection = new WorkspaceCollection( testContext.ServiceProvider );

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
                "using Metalama.Framework.Aspects; [CompileTime] class MyClass /* Intentional syntax error in compile-time code .*/ " );

            var workspaceCollection = new WorkspaceCollection( testContext.ServiceProvider );

            using var workspace = await workspaceCollection.LoadAsync( projectPath );

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

        private static async Task<string> CreateMetalamaEnabledProjectAsync( TestContext testContext, string code )
        {
            var compilationForReferences = TestCompilationFactory.CreateCSharpCompilation( "" );

            var references = compilationForReferences.ExternalReferences.OfType<PortableExecutableReference>()
                .Select( r => $"<Reference Include=\"{r.FilePath}\" />" );

            var projectPath = Path.Combine( testContext.BaseDirectory, "Project.csproj" );
            var codePath = Path.Combine( testContext.BaseDirectory, "Code.cs" );

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
        {string.Join( Environment.NewLine, references )}
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
using Metalama.Framework.Aspects;

class MyAspect : TypeAspect
{
   [Introduce] void IntroducedMethod(){}
}
[MyAspect]
class MyClass {}" );

            var workspaceCollection = new WorkspaceCollection( testContext.ServiceProvider );

            using var workspace = await workspaceCollection.LoadAsync( projectPath );

            Assert.True( workspace.IsMetalamaEnabled );
            Assert.Equal( 3, workspace.AspectClasses.Length );
            Assert.Single( workspace.AspectInstances );
            var targetFramework = Assert.Single( workspace.SourceCode.TargetFrameworks );
            Assert.Equal( "netstandard2.0", targetFramework );
        }
    }
}