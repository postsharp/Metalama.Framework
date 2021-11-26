// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Workspaces;
using System.Threading.Tasks;
using Xunit;

namespace Caravela.Framework.Tests.Workspaces
{
    public class WorkspaceTests
    {
        [Fact]
        public async Task LoadProjectSingleTarget()
        {
            using var workspace = new Workspace();
            var projects = await workspace.LoadAsync( @"C:\src\Caravela\Tests\Caravela.Framework.Tests.Workspaces\Caravela.Framework.Tests.Workspaces.csproj" );
            Assert.Single( projects.Projects );
            Assert.NotEmpty( projects.Projects[0].Types );
        }

        [Fact]
        public async Task LoadProjectMultiTarget()
        {
            using var workspace = new Workspace();
            var projects = await workspace.LoadAsync( @"C:\src\Caravela\Caravela.Framework\Caravela.Framework.csproj" );
            Assert.Equal( 2, projects.Projects.Length );
            Assert.NotNull( projects.Projects[0].TargetFramework );
        }
    }
}