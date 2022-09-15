// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Testing;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

/// <summary>
/// Implements the <see cref="IDependencyCollector"/> interface.
/// </summary>
internal class DependencyCollector : BaseDependencyCollector, IDependencyCollector
{
    private readonly ILogger _logger;
    private readonly bool _storeTypeName;

    private readonly HashSet<(ISymbol, ISymbol)> _processedDependencies = new();

    public DependencyCollector( IServiceProvider serviceProvider, ICompilationVersion compilationVersion, PartialCompilation? partialCompilation = null ) :
        base( compilationVersion, partialCompilation )
    {
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "DependencyCollector" );
        this._storeTypeName = serviceProvider.GetService<TestMarkerService>() != null;
    }

    public void AddDependency( INamedTypeSymbol masterSymbol, INamedTypeSymbol dependentSymbol )
    {
#if DEBUG
        if ( this.IsReadOnly )
        {
            throw new InvalidOperationException();
        }
#endif

        masterSymbol = masterSymbol.OriginalDefinition.GetTopContainingType();
        dependentSymbol = dependentSymbol.OriginalDefinition.GetTopContainingType();

        // If there is no syntax reference, it means it is not defined from source code but from a PE file.
        // PE references are tracked separately, and a change there invalidate the whole pipeline configuration,
        // so there is no need to track it.
        if ( masterSymbol.DeclaringSyntaxReferences.IsDefaultOrEmpty )
        {
            return;
        }

        // No need to consider self-references because we always include all partial files for all types included in a partial compilation.
        if ( SymbolEqualityComparer.Default.Equals( masterSymbol, dependentSymbol ) )
        {
            return;
        }

        // Avoid spending time processing the same call twice.
        if ( !this._processedDependencies.Add( (masterSymbol, dependentSymbol) ) )
        {
            return;
        }

        var currentCompilationAssembly = this.CompilationVersion.Compilation.Assembly;

        if ( !SymbolEqualityComparer.Default.Equals( dependentSymbol.ContainingAssembly, currentCompilationAssembly ) )
        {
            // We only collect dependencies in the current assembly.
            return;
        }

        var masterIsPartial = masterSymbol.DeclaringSyntaxReferences[0].GetSyntax() is BaseTypeDeclarationSyntax type
                              && type.Modifiers.Any( m => m.IsKind( SyntaxKind.PartialKeyword ) );

        if ( SymbolEqualityComparer.Default.Equals( masterSymbol.ContainingAssembly, currentCompilationAssembly ) )
        {
            // We have a dependency within the current assembly.
            foreach ( var dependentSyntaxReference in dependentSymbol.DeclaringSyntaxReferences )
            {
                foreach ( var masterSyntaxReference in masterSymbol.DeclaringSyntaxReferences )
                {
                    if ( dependentSyntaxReference.SyntaxTree != masterSyntaxReference.SyntaxTree )
                    {
                        this.AddSyntaxTreeDependency(
                            dependentSyntaxReference.SyntaxTree.FilePath,
                            currentCompilationAssembly.Identity,
                            masterSyntaxReference.SyntaxTree.FilePath,
                            0 );
                    }
                }

                if ( masterIsPartial )
                {
                    this.AddPartialTypeDependency(
                        dependentSyntaxReference.SyntaxTree.FilePath,
                        currentCompilationAssembly.Identity,
                        new TypeDependencyKey( masterSymbol, this._storeTypeName ) );
                }
            }
        }
        else if ( this.CompilationVersion.ReferencedCompilations.TryGetValue( masterSymbol.ContainingAssembly.Identity, out var referencedCompilationVersion ) )
        {
            // We have a dependency to a different compilation.

            foreach ( var masterSyntaxReference in masterSymbol.DeclaringSyntaxReferences )
            {
                if ( referencedCompilationVersion.TryGetSyntaxTreeVersion( masterSyntaxReference.SyntaxTree.FilePath, out var masterSyntaxTreeVersion ) )
                {
                    foreach ( var dependentSyntaxReference in dependentSymbol.DeclaringSyntaxReferences )
                    {
                        this.AddSyntaxTreeDependency(
                            dependentSyntaxReference.SyntaxTree.FilePath,
                            referencedCompilationVersion.AssemblyIdentity,
                            masterSyntaxReference.SyntaxTree.FilePath,
                            masterSyntaxTreeVersion.DeclarationHash );
                    }
                }
                else
                {
                    this._logger.Warning?.Log(
                        $"Cannot find '{masterSyntaxReference.SyntaxTree.FilePath}' in '{referencedCompilationVersion.AssemblyIdentity}'." );
                }

                if ( masterIsPartial )
                {
                    foreach ( var dependentSyntaxReference in dependentSymbol.DeclaringSyntaxReferences )
                    {
                        this.AddPartialTypeDependency(
                            dependentSyntaxReference.SyntaxTree.FilePath,
                            referencedCompilationVersion.AssemblyIdentity,
                            new TypeDependencyKey( masterSymbol, this._storeTypeName ) );
                    }
                }
            }
        }
    }
}