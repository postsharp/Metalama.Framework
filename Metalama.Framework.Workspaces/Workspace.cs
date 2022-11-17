// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Utilities;
using Metalama.Framework.Code;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Introspection;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Introspection;
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
        private readonly WorkspaceCollection _collection;
        private readonly CompileTimeDomain _domain;
        private readonly IntrospectionOptionsBox _introspectionOptions;

        internal string Key { get; }

        private ProjectSet _projects;

        static Workspace()
        {
            if ( MSBuildLocator.CanRegister )
            {
                MSBuildLocator.RegisterDefaults();
            }

            WorkspaceServices.Initialize();
        }

        private Workspace(
            ImmutableArray<string> loadedPaths,
            ImmutableDictionary<string, string>? properties,
            string key,
            ProjectSet projectSet,
            WorkspaceCollection collection,
            CompileTimeDomain domain,
            IntrospectionOptionsBox introspectionOptions )
        {
            this.Properties = properties ?? ImmutableDictionary<string, string>.Empty;
            this.LoadedPaths = loadedPaths;
            this.Key = key;
            this._projects = projectSet;
            this._collection = collection;
            this._domain = domain;
            this._introspectionOptions = introspectionOptions;
        }

        /// <summary>
        /// Gets the <see cref="IntrospectionOptions"/> for the current workspace.
        /// </summary>
        public IntrospectionOptions IntrospectionOptions
        {
            get => this._introspectionOptions.IntrospectionOptions;
            private set => this._introspectionOptions.IntrospectionOptions = value;
        }

        /// <summary>
        /// Modifies the <see cref="Metalama.Framework.Engine.Introspection.IntrospectionOptions"/> of the current workspace, and returns the current workspace.
        /// </summary>
        public Workspace WithIntrospectionOptions( IntrospectionOptions options )
        {
            this.IntrospectionOptions = options;

            return this;
        }

        /// <summary>
        /// Modifies the <see cref="IntrospectionOptions"/> of the current workspace by setting the <see cref="Engine.Introspection.IntrospectionOptions.IgnoreErrors"/>
        /// property to <c>true</c>.
        /// </summary>
        /// <returns></returns>
        public Workspace WithIgnoreErrors()
        {
            // ReSharper disable once WithExpressionModifiesAllMembers
            this.IntrospectionOptions = this.IntrospectionOptions with { IgnoreErrors = true };

            return this;
        }

        /// <summary>
        /// Reloads all projects in the current workspace.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/>.</param>
        public async Task<Workspace> ReloadAsync( bool restore = true, CancellationToken cancellationToken = default )
        {
            this._projects = await LoadProjectSet(
                this.LoadedPaths,
                this.Properties,
                this._collection,
                this._domain,
                this._introspectionOptions,
                restore,
                cancellationToken );

            return this;
        }

        public Workspace Reload( bool restore = true, CancellationToken cancellationToken = default )
        {
            this.ReloadAsync( restore, cancellationToken ).Wait( cancellationToken );

            return this;
        }

        internal static async Task<Workspace> LoadAsync(
            string key,
            ImmutableArray<string> projects,
            ImmutableDictionary<string, string> properties,
            WorkspaceCollection collection,
            bool restore,
            CancellationToken cancellationToken )
        {
            var domain = new CompileTimeDomain();

            var introspectionOptions = new IntrospectionOptionsBox();
            var projectSet = await LoadProjectSet( projects, properties, collection, domain, introspectionOptions, restore, cancellationToken );

            return new Workspace( projects, properties, key, projectSet, collection, domain, introspectionOptions );
        }

        private static void DotNetRestore( IServiceProvider serviceProvider, string project )
        {
            var dotNetTool = new DotNetTool( serviceProvider );
            dotNetTool.Execute( $"restore \"{project}\"", Path.GetDirectoryName( project ) );
        }

        private static async Task<ProjectSet> LoadProjectSet(
            ImmutableArray<string> projects,
            ImmutableDictionary<string, string> properties,
            WorkspaceCollection collection,
            CompileTimeDomain domain,
            IIntrospectionOptionsProvider introspectionOptions,
            bool restore,
            CancellationToken cancellationToken )
        {
            var ourProjects = ImmutableArray.CreateBuilder<Project>();
            var roslynWorkspace = MSBuildWorkspace.Create( properties );
            string? name = null;

            foreach ( var path in projects )
            {
                switch ( Path.GetExtension( path ).ToLowerInvariant() )
                {
                    case ".csproj":
                        if ( restore )
                        {
                            DotNetRestore( collection.ServiceProvider, path );
                        }

                        await roslynWorkspace.OpenProjectAsync( path, cancellationToken: cancellationToken );

                        if ( projects.Length == 1 )
                        {
                            name = $"{Path.GetFileName( path )} and dependencies";
                        }

                        break;

                    case ".sln":
                    case ".slnf":
                        if ( restore )
                        {
                            DotNetRestore( collection.ServiceProvider, path );
                        }

                        await roslynWorkspace.OpenSolutionAsync( path, cancellationToken: cancellationToken );

                        name = $"{Path.GetFileName( path )}";

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

                var msbuildProject = projectCollection.LoadProject( roslynProject.FilePath!, projectProperties, null );

                // Gets a Roslyn compilation.
                var compilation = (await roslynProject.GetCompilationAsync( cancellationToken )).AssertNotNull();

                // Create a compilation model.
                var context = new ServiceFactoryContext( msbuildProject, compilation, targetFramework );
                var projectOptions = new WorkspaceProjectOptions( roslynProject, msbuildProject, compilation );

                var projectServiceProvider = collection.ServiceProvider
                    .WithProjectScopedServices( projectOptions, compilation )
                    .WithServices( collection.CreateServices( context ) )
                    .WithMark( ServiceProviderMark.Test );

                var compilationModel = CodeModelFactory.CreateCompilation( compilation, projectServiceProvider );

                // Create our workspace project.
                var ourProject = new Project(
                    domain,
                    projectServiceProvider,
                    roslynProject.FilePath!,
                    compilationModel,
                    projectOptions.TargetFramework,
                    introspectionOptions );

                ourProjects.Add( ourProject );
            }

            var projectSet = new ProjectSet( ourProjects.ToImmutable(), name ?? $"{ourProjects.Count} projects" );

            return projectSet;
        }

        public event EventHandler? Disposed;

        /// <inheritdoc />
        public void Dispose()
        {
            this.Disposed?.Invoke( this, EventArgs.Empty );
        }

        /// <inheritdoc />
        public ImmutableArray<Project> Projects => this._projects.Projects;

        /// <inheritdoc />
        public ICompilationSet SourceCode => this._projects.SourceCode;

        /// <inheritdoc />
        public ImmutableArray<string> LoadedPaths { get; }

        /// <inheritdoc />
        public ImmutableDictionary<string, string> Properties { get; }

        /// <inheritdoc />
        public IProjectSet GetSubset( Predicate<Project> filter ) => this._projects.GetSubset( filter );

        /// <inheritdoc />
        public IDeclaration? GetDeclaration( string projectPath, string targetFramework, string declarationId, bool metalamaOutput )
            => this._projects.GetDeclaration( projectPath, targetFramework, declarationId, metalamaOutput );

        internal ICompilationSetResult CompilationResult => this._projects.CompilationResult;

        /// <inheritdoc />
        public ICompilationSet TransformedCode => this.CompilationResult.TransformedCode;

        /// <inheritdoc />
        public ImmutableArray<IIntrospectionAspectInstance> AspectInstances => this.CompilationResult.AspectInstances;

        /// <inheritdoc />
        public ImmutableArray<IIntrospectionAspectClass> AspectClasses => this.CompilationResult.AspectClasses;

        /// <inheritdoc />
        public ImmutableArray<IIntrospectionAdvice> Advice => this.CompilationResult.Advice;

        /// <inheritdoc />
        public ImmutableArray<IIntrospectionTransformation> Transformations => this.CompilationResult.Transformations;

        /// <inheritdoc />
        public bool IsMetalamaEnabled => this.CompilationResult.IsMetalamaEnabled;

        /// <inheritdoc />
        public bool IsMetalamaSuccessful => this.CompilationResult.IsMetalamaSuccessful;

        /// <inheritdoc />
        public ImmutableArray<IIntrospectionDiagnostic> Diagnostics => this.CompilationResult.Diagnostics;

#pragma warning disable CA1822

        /// <summary>
        /// Gets the version number of Metalama. This is determined by the LinqPad packages for Metalama, not by the Metalama packages in the projects
        /// loaded in the workspace. 
        /// </summary>
        public string? MetalamaVersion => EngineAssemblyMetadataReader.Instance.PackageVersion;

#pragma warning restore CA1822
    }
}