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
    /// <summary>
    /// Represents a mutable set of projects. You can load projects or solutions into a workspaces. When projects target several frameworks,
    /// they are represented by several instances of the <see cref="Project"/> class in the workspace.
    /// </summary>
    public sealed class Workspace : IDisposable
    {
        private readonly ImmutableDictionary<string, string> _properties;
        private readonly Dictionary<string, ProjectSet> _loadedSolutions = new( StringComparer.OrdinalIgnoreCase );

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

        /// <summary>
        /// Initializes a new instance of the <see cref="Workspace"/> class.
        /// </summary>
        /// <param name="properties">MSBuild project properties, such as <c>Configuration</c>.</param>
        public Workspace( ImmutableDictionary<string, string>? properties = null )
        {
            this._properties = properties ?? ImmutableDictionary<string, string>.Empty;
            this._workspace = MSBuildWorkspace.Create( this._properties );
        }

        /// <summary>
        /// Gets the default workspace.
        /// </summary>
        public static Workspace Default => _default.Value;

        /// <summary>
        /// Unloads all projects from the current workspace.
        /// </summary>
        public void Reset()
        {
            this._workspace.Dispose();
            this._workspace = MSBuildWorkspace.Create( this._properties );
            this._projects.Clear();
        }

        /// <summary>
        /// Reloads all projects in the current workspace.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
        public async Task ReloadAsync( CancellationToken cancellationToken = default )
        {
            var oldSolution = this._workspace.CurrentSolution;
            this.Reset();

            foreach ( var project in oldSolution.Projects )
            {
                await this.LoadProjectAsync( project.FilePath!, cancellationToken );
            }
        }

        /// <summary>
        /// Loads a project or a solution in the current workspace, and returns a <see cref="ProjectSet"/>
        /// with the loaded projects. When loading a multi-targeted project, the <see cref="ProjectSet"/> will contain several projects,
        /// one for each target framework.
        /// </summary>
        /// <param name="path">Path of the project on the file system.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="ProjectSet"/> containing the loaded project or, if the project is multi-targeted, several projects,
        /// one for each target framework.</returns>
        /// <remarks>
        /// <para>When loading projects that have project references, referenced projects are also loaded in the workspace, but are not included
        /// in the resulting <see cref="ProjectSet"/>.</para>
        /// </remarks>
        public ProjectSet Load( string path, CancellationToken cancellationToken = default )
        {
            var task = this.LoadAsync( path, cancellationToken );
            task.Wait( cancellationToken );

            return task.Result;
        }

        /// <summary>
        /// Asynchronously loads a project in the current workspace, and returns a <see cref="ProjectSet"/>
        /// with one or more projects, one for each target framework.
        /// </summary>
        /// <param name="path">Path of the project on the file system.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
        /// <returns>A <see cref="ProjectSet"/> containing the loaded project or, if the project is multi-targeted, several projects,
        /// one for each target framework.</returns>
        /// <remarks>
        /// <para>If the project has project references, referenced projects are also loaded in the workspace, but are not included
        /// in the resulting <see cref="ProjectSet"/>.</para>
        /// </remarks>
        public async Task<ProjectSet> LoadAsync( string path, CancellationToken cancellationToken = default )
        {
            switch ( Path.GetExtension( path ).ToLowerInvariant() )
            {
                case ".csproj":
                    return await this.LoadProjectAsync( path, cancellationToken );

                case ".sln":
                    return await this.LoadSolutionAsync( path, cancellationToken );

                default:
                    throw new ArgumentOutOfRangeException( nameof(path), "Invalid path extension. Only '.csproj' and '.sln' are allowed." );
            }
        }

        private async Task<ProjectSet> LoadProjectAsync( string path, CancellationToken cancellationToken = default )
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

        private async Task<ProjectSet> LoadSolutionAsync( string path, CancellationToken cancellationToken )
        {
            // If we already have the project, don't load it again.
            if ( this._loadedSolutions.TryGetValue( path, out var loadedSolution ) )
            {
                return loadedSolution;
            }

            var solution = await this._workspace.OpenSolutionAsync( path, cancellationToken: cancellationToken );

            var projectsInSolution = new List<Project>();

            foreach ( var project in solution.Projects )
            {
                // Check if that specific project has already been loaded.
                var targetFramework = WorkspaceProjectOptions.GetTargetFrameworkFromRoslynProject( project );
                var existingProject = this._projects.FirstOrDefault( p => p.Path == path && p.TargetFramework == targetFramework );

                if ( existingProject != null )
                {
                    projectsInSolution.Add( existingProject );
                }
                else
                {
                    projectsInSolution.Add( await this.LoadProjectCoreAsync( project, cancellationToken ) );
                }
            }

            var projectSet = new ProjectSet( projectsInSolution.ToImmutableArray() );
            this._loadedSolutions.Add( path, projectSet );

            return projectSet;
        }

        /// <summary>
        /// Gets a snapshot of the projects loaded in the current solution.
        /// </summary>
        public ImmutableArray<Project> Projects => this._projects.ToImmutableArray();

        /// <inheritdoc />
        public void Dispose()
        {
            this._workspace.Dispose();
        }
    }
}