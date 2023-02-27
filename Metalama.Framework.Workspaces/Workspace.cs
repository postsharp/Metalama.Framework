// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Code;
using Metalama.Framework.Engine;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Introspection;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Introspection;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Workspaces
{
    /// <summary>
    /// Represents a set of projects. Workspaces can be created using the <see cref="WorkspaceCollection"/> class.  When projects target several frameworks,
    /// they are represented by several instances of the <see cref="Project"/> class in the workspace.
    /// </summary>
    [PublicAPI]
    public sealed class Workspace : IDisposable, IProjectSet, IWorkspaceLoadInfo
    {
        private readonly WorkspaceCollection _collection;
        private readonly CompileTimeDomain _domain;
        private readonly IntrospectionOptionsBox _introspectionOptions;

        internal string Key { get; }

        private ProjectSet _projects;
        private readonly ITaskRunner _taskRunner;

        static Workspace()
        {
            if ( MSBuildLocator.CanRegister )
            {
                try
                {
                    MSBuildLocator.RegisterDefaults();
                }
                catch ( InvalidOperationException e )
                {
                    throw new DotNetSdkLoadException(
                        $"Could not find a .NET SDK for {RuntimeInformation.RuntimeIdentifier} {RuntimeInformation.ProcessArchitecture}. Did you select the right .NET version and processor architecture?",
                        e );
                }
            }

            WorkspaceServices.Initialize();
        }

        private Workspace(
            GlobalServiceProvider serviceProvider,
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
            this._taskRunner = serviceProvider.GetRequiredService<ITaskRunner>();
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
        /// Modifies the <see cref="Introspection.IntrospectionOptions"/> of the current workspace, and returns the current workspace.
        /// </summary>
        public Workspace WithIntrospectionOptions( IntrospectionOptions options )
        {
            this.IntrospectionOptions = options;

            return this;
        }

        /// <summary>
        /// Modifies the <see cref="IntrospectionOptions"/> of the current workspace by setting the <see cref="Introspection.IntrospectionOptions.IgnoreErrors"/>
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
            this._projects = await LoadProjectSetAsync(
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
            this._taskRunner.RunSynchronously( () => this.ReloadAsync( restore, cancellationToken ) );

            return this;
        }

        internal static async Task<Workspace> LoadAsync(
            GlobalServiceProvider serviceProvider,
            string key,
            ImmutableArray<string> projects,
            ImmutableDictionary<string, string> properties,
            WorkspaceCollection collection,
            bool restore,
            CancellationToken cancellationToken )
        {
            var domain = new UnloadableCompileTimeDomain( serviceProvider );

            var introspectionOptions = new IntrospectionOptionsBox();
            var projectSet = await LoadProjectSetAsync( projects, properties, collection, domain, introspectionOptions, restore, cancellationToken );

            return new Workspace(
                serviceProvider,
                projects,
                properties,
                key,
                projectSet,
                collection,
                domain,
                introspectionOptions );
        }

        private static void DotNetRestore( GlobalServiceProvider serviceProvider, string project )
        {
            var dotNetTool = new DotNetTool( serviceProvider );
            dotNetTool.Execute( $"restore \"{project}\"", Path.GetDirectoryName( project ) );
        }

        private static async Task<ProjectSet> LoadProjectSetAsync(
            ImmutableArray<string> projects,
            ImmutableDictionary<string, string> properties,
            WorkspaceCollection collection,
            CompileTimeDomain domain,
            IIntrospectionOptionsProvider introspectionOptions,
            bool restore,
            CancellationToken cancellationToken )
        {
            var ourProjects = ImmutableArray.CreateBuilder<Project>();

            var allProperties = properties
                .Add( "MSBuildEnableWorkloadResolver", "false" )
                .Add( "DOTNET_ROOT_X64", "" )
                .Add( "MSBUILD_EXE_PATH", "" )
                .Add( "MSBuildSDKsPath", "" );

            var roslynWorkspace = MSBuildWorkspace.Create( allProperties );

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

            using var msbuildProjectCollection = new ProjectCollection( allProperties );

            foreach ( var roslynProject in roslynWorkspace.CurrentSolution.Projects )
            {
                // Get an evaluated MSBuild project (the Roslyn workspace presumably does it, but it the result is not made available). 
                var targetFramework = WorkspaceProjectOptions.GetTargetFrameworkFromRoslynProject( roslynProject );

                Dictionary<string, string>? projectProperties = null;

                if ( targetFramework != null )
                {
                    projectProperties = new Dictionary<string, string> { ["TargetFramework"] = targetFramework };
                }

                var msbuildProject = msbuildProjectCollection.LoadProject( roslynProject.FilePath!, projectProperties, null );

                // Gets a Roslyn compilation.
                var compilation = (await roslynProject.GetCompilationAsync( cancellationToken )).AssertNotNull();

                // Merge service injection.
                var existingAdditionalServiceCollection = collection.ServiceProvider.GetService<AdditionalServiceCollection>();

                var additionalServiceCollection = new AdditionalServiceCollection( existingAdditionalServiceCollection );
                additionalServiceCollection.ProjectServices.Add( collection.ServiceBuilder.Build );

                // Create a compilation model.
                var projectOptions = new WorkspaceProjectOptions( roslynProject, msbuildProject, compilation );

                var projectServiceProvider = collection.ServiceProvider.Underlying.WithService( additionalServiceCollection, true )
                    .WithProjectScopedServices( projectOptions, compilation );

                var compilationModel = CodeModelFactory.CreateCompilation( compilation, projectServiceProvider );

                // Create our workspace project.
                var ourProject = new Project(
                    domain,
                    projectServiceProvider,
                    roslynProject.FilePath!,
                    compilationModel,
                    projectOptions,
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
            this._domain.Dispose();

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
        public IDeclaration GetDeclaration( string projectName, string targetFramework, string declarationId, bool metalamaOutput )
            => this._projects.GetDeclaration( projectName, targetFramework, declarationId, metalamaOutput );

        internal ICompilationSetResult CompilationResult => this._projects.CompilationResult;

        /// <inheritdoc />
        public ICompilationSet TransformedCode => this.CompilationResult.TransformedCode;

        /// <inheritdoc />
        public ImmutableArray<IIntrospectionAspectLayer> AspectLayers => this.CompilationResult.AspectLayers;

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
        public bool HasMetalamaSucceeded => this.CompilationResult.HasMetalamaSucceeded;

        /// <inheritdoc />
        public ImmutableArray<IIntrospectionDiagnostic> Diagnostics => this.CompilationResult.Diagnostics;

#pragma warning disable CA1822

        /// <summary>
        /// Gets the version number of Metalama. This is determined by the LinqPad packages for Metalama, not by the Metalama packages in the projects
        /// loaded in the workspace. 
        /// </summary>
        public string? MetalamaVersion => EngineAssemblyMetadataReader.Instance.PackageVersion;

        public Project GetProject( string name, string? targetFramework = null )
        {
            var candidates = this.Projects.Where( p => p.Name == name && (targetFramework == null || p.TargetFramework == targetFramework) ).ToList();

            if ( candidates.Count == 0 )
            {
                throw new KeyNotFoundException();
            }
            else if ( candidates.Count > 1 )
            {
                throw new InvalidOperationException( "Ambiguous match." );
            }
            else
            {
                return candidates[0];
            }
        }

#pragma warning restore CA1822
    }
}