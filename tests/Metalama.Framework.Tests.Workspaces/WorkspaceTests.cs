// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Testing;
using Metalama.Framework.Workspaces;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.Workspaces
{
    public class WorkspaceTests : TestBase
    {
        [Fact]
        public async Task LoadProjectSingleTargetAsync()
        {
            using var testContext = this.CreateTestContext();

            var projectPath = Path.Combine( testContext.ProjectOptions.BaseDirectory, "Project.csproj" );
            var codePath = Path.Combine( testContext.ProjectOptions.BaseDirectory, "Code.cs" );

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

            var workspaceCollection = new WorkspaceCollection();

            using var workspace = await workspaceCollection.LoadAsync( projectPath );

            Assert.Single( workspace.Projects );
            Assert.Single( workspace.Projects[0].Types );
        }

        [Fact]
        public async Task LoadProjectMultiTargetAsync()
        {
            using var testContext = this.CreateTestContext();

            var projectPath = Path.Combine( testContext.ProjectOptions.BaseDirectory, "Project.csproj" );
            var codePath = Path.Combine( testContext.ProjectOptions.BaseDirectory, "Code.cs" );

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

            var workspaceCollection = new WorkspaceCollection();

            using var workspace = await workspaceCollection.LoadAsync( projectPath );

            Assert.Equal( 2, workspace.Projects.Length );
            Assert.Equal( 2, workspace.Types.Length );
        }
    }
}