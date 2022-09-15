// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

/// <summary>
/// Represents all dependencies of a given compilation.
/// </summary>
internal readonly struct DependencyGraph
{
    public static DependencyGraph Create( BaseDependencyCollector dependencies )
    {
        var emptyGraph = new DependencyGraph( ImmutableDictionary<ProjectKey, DependencyGraphByDependentProject>.Empty );

        return emptyGraph.Update( dependencies );
    }

    public static DependencyGraph Empty => new( ImmutableDictionary<ProjectKey, DependencyGraphByDependentProject>.Empty );

    public bool IsUninitialized => this.DependenciesByCompilation == null!;

    /// <summary>
    /// Gets the dependencies indexed by compilation.
    /// </summary>
    public ImmutableDictionary<ProjectKey, DependencyGraphByDependentProject> DependenciesByCompilation { get; }

    /// <summary>
    /// Updates the <see cref="DependencyGraph"/> based on a <see cref="BaseDependencyCollector"/>.
    /// </summary>
    public DependencyGraph Update( BaseDependencyCollector dependencyCollector )
    {
        var builder = this.ToBuilder();

        // Add or update dependencies.
        foreach ( var dependenciesByDependentFilePath in dependencyCollector.DependenciesByDependentFilePath )
        {
            var dependentFilePath = dependenciesByDependentFilePath.Key;

            // ReSharper disable once SuspiciousTypeConversion.Global

            foreach ( var dependenciesByCompilation in dependenciesByDependentFilePath.Value.DependenciesByCompilation )
            {
                var compilation = dependenciesByCompilation.Key;
                builder.UpdateDependencies( compilation, dependentFilePath, dependenciesByCompilation.Value );
            }
        }

        // Remove graphs for dependent syntax trees were analyzed but for which no dependency was found.
        foreach ( var syntaxTreeEntry in dependencyCollector.PartialCompilation.SyntaxTrees )
        {
            if ( !dependencyCollector.DependenciesByDependentFilePath.ContainsKey( syntaxTreeEntry.Key ) )
            {
                // The syntax tree does not have any dependency in any compilation.
                builder.RemoveDependentSyntaxTree( syntaxTreeEntry.Key );
            }
        }

        return builder.ToImmutable();
    }

    public Builder ToBuilder() => new( this );

    private DependencyGraph( ImmutableDictionary<ProjectKey, DependencyGraphByDependentProject> dependenciesByCompilation )
    {
        this.DependenciesByCompilation = dependenciesByCompilation;
    }

    public struct Builder
    {
        private readonly DependencyGraph _dependencyGraph;
        private ImmutableDictionary<ProjectKey, DependencyGraphByDependentProject>.Builder? _dependenciesByCompilationBuilder;

        public Builder( DependencyGraph dependencyGraph )
        {
            this._dependencyGraph = dependencyGraph;
            this._dependenciesByCompilationBuilder = null;
        }

        private ImmutableDictionary<ProjectKey, DependencyGraphByDependentProject>.Builder GetDependenciesByCompilationBuilder()
            => this._dependenciesByCompilationBuilder ??= this._dependencyGraph.DependenciesByCompilation.ToBuilder();

        private IReadOnlyDictionary<ProjectKey, DependencyGraphByDependentProject> GetDependenciesByCompilation()
            => (IReadOnlyDictionary<ProjectKey, DependencyGraphByDependentProject>?) this._dependenciesByCompilationBuilder
               ?? this._dependencyGraph.DependenciesByCompilation;

        public void RemoveDependentSyntaxTree( string path )
        {
            foreach ( var compilationDependencies in this.GetDependenciesByCompilation() )
            {
                if ( compilationDependencies.Value.TryRemoveDependentSyntaxTree( path, out var newDependencies ) )
                {
                    if ( newDependencies.IsEmpty )
                    {
                        this.GetDependenciesByCompilationBuilder().Remove( compilationDependencies.Key );
                    }
                    else
                    {
                        this.GetDependenciesByCompilationBuilder()[compilationDependencies.Key] = newDependencies;
                    }
                }
            }
        }

        public void RemoveProject( ProjectKey projectKey )
        {
            if ( this.GetDependenciesByCompilation().ContainsKey( projectKey ) )
            {
                this.GetDependenciesByCompilationBuilder().Remove( projectKey );
            }
        }

        public void UpdateDependencies(
            ProjectKey projectKey,
            string dependentFilePath,
            DependencyCollectorByDependentSyntaxTreeAndMasterProject dependencies )
        {
            if ( !this.GetDependenciesByCompilation().TryGetValue( projectKey, out var currentDependenciesOfCompilation ) )
            {
                currentDependenciesOfCompilation = new DependencyGraphByDependentProject( projectKey );
            }

            if ( currentDependenciesOfCompilation.TryUpdateDependencies(
                    dependentFilePath,
                    dependencies,
                    out var newDependenciesOfCompilation ) )
            {
                this.GetDependenciesByCompilationBuilder()[projectKey] = newDependenciesOfCompilation;
            }
            else
            {
                // The dependencies have not changed.
            }
        }

        public DependencyGraph ToImmutable()
            => this._dependenciesByCompilationBuilder != null
                ? new DependencyGraph( this._dependenciesByCompilationBuilder.ToImmutable() )
                : this._dependencyGraph;
    }
}