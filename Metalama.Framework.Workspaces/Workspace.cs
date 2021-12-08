// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Pipeline;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Workspaces
{
    /// <summary>
    /// Represents a set of projects. Workspaces can be created using the <see cref="WorkspaceCollection"/> class.  When projects target several frameworks,
    /// they are represented by several instances of the <see cref="Project"/> class in the workspace.
    /// </summary>
    public sealed class Workspace : IDisposable, IProjectSet, IWorkspaceLoadInfo
    {
        private readonly ImmutableDictionary<string, string> _properties;

        private readonly ImmutableArray<string> _loadedPaths;

        private readonly WorkspaceCollection _collection;

        internal string Key { get; }

        private ProjectSet _projects;

        static Workspace()
        {
            if ( MSBuildLocator.CanRegister )
            {
                MSBuildLocator.RegisterDefaults();
            }
        }

        private Workspace(
            ImmutableArray<string> loadedPaths,
            ImmutableDictionary<string, string>? properties,
            string key,
            ProjectSet projectSet,
            WorkspaceCollection collection )
        {
            this._properties = properties ?? ImmutableDictionary<string, string>.Empty;
            this._loadedPaths = loadedPaths;
            this.Key = key;
            this._projects = projectSet;
            this._collection = collection;
        }

        /// <summary>
        /// Reloads all projects in the current workspace.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
        public async Task ReloadAsync( CancellationToken cancellationToken = default )
        {
            this._projects = await LoadProjectSet( this._loadedPaths, this._properties, this._collection, cancellationToken );
        }

        public void Reload( CancellationToken cancellationToken = default ) => this.ReloadAsync( cancellationToken ).Wait( cancellationToken );

        internal static async Task<Workspace> LoadAsync(
            string key,
            ImmutableArray<string> projects,
            ImmutableDictionary<string, string> properties,
            WorkspaceCollection collection,
            CancellationToken cancellationToken )
        {
            var projectSet = await LoadProjectSet( projects, properties, collection, cancellationToken );

            return new Workspace( projects, properties, key, projectSet, collection );
        }

        private static async Task<ProjectSet> LoadProjectSet(
            ImmutableArray<string> projects,
            ImmutableDictionary<string, string> properties,
            WorkspaceCollection collection,
            CancellationToken cancellationToken )
        {
            var ourProjects = ImmutableArray.CreateBuilder<Project>();
            MSBuildWorkspace roslynWorkspace = MSBuildWorkspace.Create( properties );

            foreach ( var path in projects )
            {
                switch ( Path.GetExtension( path ).ToLowerInvariant() )
                {
                    case ".csproj":
                        await roslynWorkspace.OpenProjectAsync( path, cancellationToken: cancellationToken );

                        break;

                    case ".sln":
                    case ".slnf":
                        await roslynWorkspace.OpenSolutionAsync( path, cancellationToken: cancellationToken );

                        break;

                    default:
                        throw new ArgumentOutOfRangeException( nameof(path), "Invalid path extension. Only '.csproj', '.sln' and '.slnf' are allowed." );
                }
            }

            using var projectCollection = new ProjectCollection( properties );

            foreach ( var roslynProject in roslynWorkspace.CurrentSolution.Projects )
            {
                // Get an evaluated MSBuild project (the Roslyn workspace presumably does it, but it the result is not made available). 
                var targetFramework = WorkspaceProjectOptions.GetTargetFrameworkFromRoslynProject( roslynProject );

                Dictionary<string, string>? projectProperties = null;

                if ( targetFramework != null )
                {
                    projectProperties = new Dictionary<string, string> { ["TargetFramework"] = targetFramework };
                }

                Microsoft.Build.Evaluation.Project msbuildProject = projectCollection.LoadProject( roslynProject.FilePath, projectProperties, null );

                // Gets a Roslyn compilation.
                var compilation = (await roslynProject.GetCompilationAsync( cancellationToken )).AssertNotNull();

                // Create a compilation model.
                var context = new ServiceFactoryContext( msbuildProject, compilation, targetFramework );
                var projectOptions = new WorkspaceProjectOptions( roslynProject, msbuildProject, compilation, targetFramework );

                var serviceProvider = ServiceProviderFactory.GetServiceProvider()
                    .WithProjectScopedServices( compilation.References )
                    .WithService( projectOptions )
                    .WithServices( collection.CreateServices( context ) )
                    .WithMark( ServiceProviderMark.Test );

                var compilationModel = CodeModelFactory.CreateCompilation( compilation, serviceProvider );

                // Create our workspace project.
                var ourProject = new Project( roslynProject.FilePath!, compilationModel, projectOptions.TargetFramework );
                ourProjects.Add( ourProject );
            }

            var projectSet = new ProjectSet( ourProjects.ToImmutable() );

            return projectSet;
        }

        public event EventHandler? Disposed;

        /// <inheritdoc />
        public void Dispose()
        {
            this.Disposed?.Invoke( this, EventArgs.Empty );
        }

        public ImmutableArray<Project> Projects => this._projects.Projects;

        public ImmutableArray<TargetFramework> TargetFrameworks => this._projects.TargetFrameworks;

        public ImmutableArray<INamedType> Types => this._projects.Types;

        public ImmutableArray<IMethod> Methods => this._projects.Methods;

        public ImmutableArray<IField> Fields => this._projects.Fields;

        ImmutableArray<string> IWorkspaceLoadInfo.LoadedPaths => this._loadedPaths;

        ImmutableDictionary<string, string> IWorkspaceLoadInfo.Properties => this._properties;

        public ImmutableArray<IProperty> Properties => this._projects.Properties;

        public ImmutableArray<IFieldOrProperty> FieldsAndProperties => this._projects.FieldsAndProperties;

        public ImmutableArray<IConstructor> Constructors => this._projects.Constructors;

        public ImmutableArray<IEvent> Events => this._projects.Events;

        public ImmutableArray<IDiagnostic> Diagnostics => this._projects.Diagnostics;

        public IProjectSet GetSubset( Predicate<Project> filter ) => this._projects.GetSubset( filter );

        public IDeclaration? GetDeclaration( string projectPath, string targetFramework, string declarationId )
            => this._projects.GetDeclaration( projectPath, targetFramework, declarationId );
    }
}