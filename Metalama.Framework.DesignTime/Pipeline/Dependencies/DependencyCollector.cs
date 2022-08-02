// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

/// <summary>
/// Implements the <see cref="IDependencyCollector"/> interface.
/// </summary>
internal class DependencyCollector : IDependencyCollector
{
    private readonly Compilation _currentCompilation;
    private readonly IReadOnlyDictionary<AssemblyIdentity, CompilationVersion> _compilationReferences;
    private readonly ILogger _logger;

    private readonly HashSet<(ISymbol, ISymbol)> _processedDependencies = new();
    private readonly HashSet<DependencyEdge> _dependencies = new();

    public DependencyCollector( IServiceProvider serviceProvider, Compilation compilation, IEnumerable<CompilationVersion> compilationReferences )
    {
        this._currentCompilation = compilation;
        this._compilationReferences = compilationReferences.ToDictionary( x => x.Compilation.Assembly.Identity, x => x );
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "DependencyCollector" );
    }

    public IEnumerable<DependencyEdge> GetDependencies() => this._dependencies;

    public void AddDependency( ISymbol masterSymbol, ISymbol dependentSymbol )
    {
        masterSymbol = masterSymbol.OriginalDefinition;
        dependentSymbol = dependentSymbol.OriginalDefinition;

        // Avoid spending time processing the same call twice.
        if ( !this._processedDependencies.Add( (masterSymbol, dependentSymbol) ) )
        {
            return;
        }

        var currentCompilationAssembly = this._currentCompilation.Assembly;

        if ( !SymbolEqualityComparer.Default.Equals( dependentSymbol.ContainingAssembly, currentCompilationAssembly ) )
        {
            // We only collect dependencies in the current assembly.
            return;
        }

        if ( SymbolEqualityComparer.Default.Equals( masterSymbol.ContainingAssembly, currentCompilationAssembly ) )
        {
            // We have a dependency within the current assembly.
            foreach ( var dependentSyntaxReference in dependentSymbol.DeclaringSyntaxReferences )
            {
                foreach ( var masterSyntaxReference in masterSymbol.DeclaringSyntaxReferences )
                {
                    if ( dependentSyntaxReference.SyntaxTree != masterSyntaxReference.SyntaxTree )
                    {
                        this._dependencies.Add(
                            new DependencyEdge(
                                this._currentCompilation,
                                masterSyntaxReference.SyntaxTree.FilePath,
                                0,
                                dependentSyntaxReference.SyntaxTree.FilePath ) );
                    }
                }
            }
        }
        else if ( this._compilationReferences.TryGetValue( masterSymbol.ContainingAssembly.Identity, out var compilationReference ) )
        {
            // We have a dependency to a different compilation.

            foreach ( var masterSyntaxReference in masterSymbol.DeclaringSyntaxReferences )
            {
                if ( compilationReference.SyntaxTrees.TryGetValue( masterSyntaxReference.SyntaxTree.FilePath, out var masterSyntaxTree ) )
                {
                    foreach ( var dependentSyntaxReference in dependentSymbol.DeclaringSyntaxReferences )
                    {
                        this._dependencies.Add(
                            new DependencyEdge(
                                compilationReference.Compilation,
                                masterSyntaxReference.SyntaxTree.FilePath,
                                masterSyntaxTree.Hash,
                                dependentSyntaxReference.SyntaxTree.FilePath ) );
                    }
                }
                else
                {
                    this._logger.Warning?.Log( $"Cannot find '{masterSyntaxReference.SyntaxTree.FilePath}' in '{compilationReference.Compilation.Assembly}'." );
                }
            }
        }
    }
}