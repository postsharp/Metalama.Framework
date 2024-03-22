// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Backstage.Diagnostics;
using Metalama.Backstage.Extensibility;
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
        private static ILogger _logger;

        internal string Key { get; }

        private ProjectSet _projects;
        private readonly ITaskRunner _taskRunner;

        static Workspace()
        {
            WorkspaceServices.Initialize();
            _logger = BackstageServiceFactory.ServiceProvider.GetLoggerFactory().GetLogger( "Workspace" );
        }

        private static void InitializeMSBuild( string projectDirectory )
        {
            if ( !MSBuildLocator.IsRegistered )
            {
                _logger.Trace?.Log(
                    $"Initializing MSBuild with directory '{projectDirectory}' with {RuntimeInformation.FrameworkDescription} running on {RuntimeInformation.RuntimeIdentifier}." );

                var instances = MSBuildLocator.QueryVisualStudioInstances(
                        new VisualStudioInstanceQueryOptions { DiscoveryTypes = DiscoveryType.DotNetSdk, WorkingDirectory = projectDirectory } )
                    .OrderByDescending( i => i.Version )
                    .ToReadOnlyList();

                _logger.Trace?.Log( $"Found {instances.Count} instances: {string.Join( ", ", instances.Select( x => x.Name ) )}" );

                if ( instances.Count == 0 )
                {
                    throw new DotNetSdkLoadException(
                        $"Could not find a .NET SDK for {RuntimeInformation.RuntimeIdentifier} {RuntimeInformation.ProcessArchitecture}. Did you select the right .NET version and processor architecture?" );
                }

                var instance = instances.First();
                MSBuildLocator.RegisterInstance( instance );
            }
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

        private static Task<ProjectSet> LoadProjectSetAsync(
            ImmutableArray<string> projects,
            ImmutableDictionary<string, string> properties,
            WorkspaceCollection collection,
            CompileTimeDomain domain,
            IIntrospectionOptionsProvider introspectionOptions,
            bool restore,
            CancellationToken cancellationToken )
        {
            if ( projects.IsEmpty )
            {
                return Task.FromResult( new ProjectSet( ImmutableArray<Project>.Empty, "Empty" ) );
            }

            // We need to initialize MSBuild once per process. The initialization may depend on `global.json`.
            // In case we are using a single process to analyze projects with repos with different global.json,
            // weird things may appear. Currently this case is not covered.
            if ( !MSBuildLocator.IsRegistered )
            {
                InitializeMSBuild( Path.GetDirectoryName( projects[0] ) );
            }

            // We can call the next method only after MSBuild initialization because it loads MSBuild assemblies.
            return LoadProjectSetCoreAsync( projects, properties, collection, domain, introspectionOptions, restore, cancellationToken );
        }

        private static async Task<ProjectSet> LoadProjectSetCoreAsync(
            ImmutableArray<string> projects,
            ImmutableDictionary<string, string> properties,
            WorkspaceCollection collection,
            CompileTimeDomain domain,
            IIntrospectionOptionsProvider introspectionOptions,
            bool restore,
            CancellationToken cancellationToken )
        {
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

            // Start all tasks in parallel because even that may be expensive.
            var loadProjectTasks = roslynWorkspace.CurrentSolution.Projects.AsParallel().Select( GetOurProjectAsync ).ToArray();

            var ourProjects = (await Task.WhenAll( loadProjectTasks )).ToImmutableArray();

            var projectSet = new ProjectSet( ourProjects, name ?? $"{ourProjects.Length} projects" );

            return projectSet;

            async Task<Project> GetOurProjectAsync( Microsoft.CodeAnalysis.Project roslynProject )
            {
                // Get an evaluated MSBuild project (the Roslyn workspace presumably does but it the result is not made available). 
                var targetFramework = WorkspaceProjectOptions.GetTargetFrameworkFromRoslynProject( roslynProject );

                Dictionary<string, string>? projectProperties = null;

                if ( targetFramework != null )
                {
                    projectProperties = new Dictionary<string, string> { ["TargetFramework"] = targetFramework };
                }

                // ReSharper disable once AccessToDisposedClosure
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

                return ourProject;
            }
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
            var candidates = this.Projects.Where( p => p.Name == name && (targetFramework == null || p.TargetFramework == targetFramework) ).ToReadOnlyList();

            return candidates.Count switch
            {
                0 => throw new KeyNotFoundException(),
                > 1 => throw new InvalidOperationException( "Ambiguous match." ),
                _ => candidates[0]
            };
        }

#pragma warning restore CA1822
    }
}