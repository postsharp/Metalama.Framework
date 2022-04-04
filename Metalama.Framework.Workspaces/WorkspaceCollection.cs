// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Metrics;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Workspaces
{
    /// <summary>
    /// Represents a set of workspaces. Two attempts to load a workspace with the same parameters, in the same <see cref="WorkspaceCollection"/>,
    /// will return the exact same instance, unless the <see cref="Reset"/> method is called.
    /// </summary>
    public sealed class WorkspaceCollection
    {
        private readonly ConcurrentDictionary<string, Task<Workspace>> _workspaces = new();
        private readonly List<Func<ServiceFactoryContext, IService>> _serviceFactories = new();

        public static WorkspaceCollection Default { get; } = new();

        /// <summary>
        /// Loads a set of projects of solutions into a <see cref="Workspace"/>, or returns an existing workspace
        /// if the method has been previously called with the exact same parameters. 
        /// </summary>
        /// <param name="paths">A list of project or solution paths.</param>
        /// <returns>A <see cref="Workspace"/> where all specified project or solutions, and their dependencies, have been loaded.</returns>
        public Workspace Load( params string[] paths ) => this.LoadAsync( paths.ToImmutableArray() ).Result;

        /// <summary>
        /// Asynchronously loads a set of projects of solutions into a <see cref="Workspace"/>, or returns an existing workspace
        /// if the method has been previously called with the exact same parameters. 
        /// </summary>
        /// <param name="paths">A list of project or solution paths.</param>
        /// <returns>A <see cref="Workspace"/> where all specified project or solutions, and their dependencies, have been loaded.</returns>
        public Task<Workspace> LoadAsync( params string[] paths ) => this.LoadAsync( paths.ToImmutableArray() );

        /// <summary>
        /// Asynchronously loads a set of projects of solutions into a <see cref="Workspace"/>, or returns an existing workspace
        /// if the method has been previously called with the exact same parameters. This overload allows to specify MSBuild properties.
        /// </summary>
        /// <param name="paths">A list of project or solution paths.</param>
        /// <returns>A <see cref="Workspace"/> where all specified project or solutions, and their dependencies, have been loaded.</returns>
        public Task<Workspace> LoadAsync(
            ImmutableArray<string> paths,
            ImmutableDictionary<string, string>? properties = null,
            CancellationToken cancellationToken = default )
        {
            properties ??= properties ?? ImmutableDictionary<string, string>.Empty;
            var key = GetWorkspaceKey( paths, properties );

            async Task<Workspace> LoadCore( string k )
            {
                var workspace = await Workspace.LoadAsync( key, paths, properties, this, cancellationToken );
                workspace.Disposed += this.OnWorkspaceDisposed;

                return workspace;
            }

            return this._workspaces.GetOrAdd( key, LoadCore );
        }

        /// <summary>
        /// Register a project service. This is useful for instance to register an <see cref="IMetricProvider{T}"/>.
        /// </summary>
        /// <param name="factory">A function that instantiates the service for the given project.</param>
        public void RegisterService( Func<ServiceFactoryContext, IService> factory ) => this._serviceFactories.Add( factory );

        internal IEnumerable<IService> CreateServices( ServiceFactoryContext context ) => this._serviceFactories.Select( x => x( context ) );

        private void OnWorkspaceDisposed( object? sender, EventArgs e )
        {
            var workspace = (Workspace) sender!;
            workspace.Disposed += this.OnWorkspaceDisposed;
            this._workspaces.TryRemove( workspace.Key, out _ );
        }

        private static string GetWorkspaceKey( ImmutableArray<string> initialProjects, ImmutableDictionary<string, string> properties )
        {
            var sortedProjects = initialProjects.OrderBy( p => p, StringComparer.Ordinal );
            var sortedProperties = properties.OrderBy( p => p.Key, StringComparer.OrdinalIgnoreCase ).Select( p => $"{p.Key}={p.Value}" );

            return string.Join( ",", sortedProjects.Concat( sortedProperties ) );
        }

        /// <summary>
        /// Finds the <see cref="Workspace"/> and <see cref="Project"/> that defines a given Roslyn <see cref="Compilation" /> in the current <see cref="WorkspaceCollection"/>.
        /// </summary>
        public bool TryFindProject(
            Compilation compilation,
            [NotNullWhen( true )] out Workspace? workspace,
            [NotNullWhen( true )] out Project? project,
            out bool isMetalamaOutput )
        {
            var found = this._workspaces.Values
                .Select(
                    w => w.IsCompleted
                        ? (Project: w.Result.Projects.FirstOrDefault(
                               p => p.Compilation.GetRoslynCompilation() == compilation
                                    || (p.IsMetalamaOutputEvaluated && p.MetalamaOutput.Compilation.GetRoslynCompilation() == compilation) ),
                           Workspace: w.Result)
                        : (null, null) )
                .FirstOrDefault( p => p.Project != null );

            if ( found.Project != null )
            {
                workspace = found.Workspace!;
                project = found.Project;
                isMetalamaOutput = project.Compilation.GetRoslynCompilation() != compilation;

                return true;
            }
            else
            {
                workspace = null;
                project = null;
                isMetalamaOutput = false;

                return false;
            }
        }

        /// <summary>
        /// Removes all cached workspaces, but not the set of registered services.
        /// </summary>
        public void Reset()
        {
            // TODO: cancel pending tasks. This is not a critical use case currently.
            this._workspaces.Clear();
        }
    }
}