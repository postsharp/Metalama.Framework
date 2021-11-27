// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Caravela.Framework.Workspaces
{
    public class Workspace : IDisposable
    {
        private readonly ImmutableDictionary<string, string> _properties;

        static Workspace()
        {
            if ( MSBuildLocator.CanRegister )
            {
                MSBuildLocator.RegisterDefaults();
            }
        }

        private readonly List<Project> _projects = new();
        private MSBuildWorkspace _workspace;

        private static readonly Lazy<Workspace> _default = new( () => new Workspace() );

        public Workspace( ImmutableDictionary<string, string>? properties = null )
        {
            this._properties = properties ?? ImmutableDictionary<string, string>.Empty;
            this._workspace = MSBuildWorkspace.Create( this._properties );
        }

        public static Workspace Default => _default.Value;

        public void Reset()
        {
            this._workspace.Dispose();
            this._workspace = MSBuildWorkspace.Create( this._properties );
            this._projects.Clear();
        }

        public async Task ReloadAsync( CancellationToken cancellationToken = default )
        {
            var oldSolution = this._workspace.CurrentSolution;
            this.Reset();

            foreach ( var project in oldSolution.Projects )
            {
                await this.LoadProjectAsync( project.FilePath!, cancellationToken );
            }
        }

        public ProjectSet Load( string path, CancellationToken cancellationToken = default )
        {
            var task = this.LoadAsync( path, cancellationToken );
            task.Wait( cancellationToken );

            return task.Result;
        }

        public async Task<ProjectSet> LoadAsync( string path, CancellationToken cancellationToken = default )
        {
            switch ( Path.GetExtension( path ).ToLowerInvariant() )
            {
                case ".csproj":
                    return await this.LoadProjectAsync( path, cancellationToken );

                case ".sln":
                    return await this.LoadSolutionAsync( path, cancellationToken );

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public async Task<ProjectSet> LoadProjectAsync( string path, CancellationToken cancellationToken = default )
        {
            // If we already have the project, don't load it again.
            var existingProjects = this._projects.Where( p => p.Path == path ).ToImmutableArray();

            if ( !existingProjects.IsEmpty )
            {
                return new ProjectSet( existingProjects );
            }

            // The project may have been loaded into the Roslyn workspace.
            var existingSolutionProjects = this._workspace.CurrentSolution.Projects.Where( p => p.FilePath == path ).ToList();

            if ( existingSolutionProjects.Count == 0 )
            {
                // The project has not been loaded yet.

                await this._workspace.OpenProjectAsync( path, cancellationToken: cancellationToken );
            }

            var ourProjects = ImmutableArray.CreateBuilder<Project>();

            foreach ( var project in this._workspace.CurrentSolution.Projects.Where( p => p.FilePath == path ) )
            {
                var ourProject = await this.LoadProjectCoreAsync( project, cancellationToken );
                ourProjects.Add( ourProject );
            }

            return new ProjectSet( ourProjects.ToImmutable() );
        }

        private async Task<Project> LoadProjectCoreAsync( Microsoft.CodeAnalysis.Project project, CancellationToken cancellationToken )
        {
            var compilation = (await project.GetCompilationAsync( cancellationToken )).AssertNotNull();
            var projectOptions = new WorkspaceProjectOptions( this._properties, project, compilation );

            var serviceProvider = ServiceProviderFactory.GetServiceProvider()
                .WithProjectScopedServices( compilation.References )
                .WithService( projectOptions )
                .WithMark( ServiceProviderMark.Test );

            var compilationModel = CodeModelFactory.CreateCompilation( compilation, serviceProvider );

            var ourProject = new Project( project.FilePath!, compilationModel, projectOptions.TargetFramework );
            this._projects.Add( ourProject );

            return ourProject;
        }

        public async Task<ProjectSet> LoadSolutionAsync( string path, CancellationToken cancellationToken )
        {
            var solution = await this._workspace.OpenSolutionAsync( path, cancellationToken: cancellationToken );

            foreach ( var project in solution.Projects )
            {
                try
                {
                    await this.LoadProjectCoreAsync( project, cancellationToken );
                }
                catch
                {
                    // TODO: log
                }
            }

            return new ProjectSet( this._projects.ToImmutableArray() );
        }

        public IEnumerable<Project> Projects => this._projects;

        public void Dispose()
        {
            this._workspace.Dispose();
        }
    }
}