// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;
using Metalama.Framework.Engine.Pipeline.DesignTime;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metalama.Framework.DesignTime.Pipeline.Dependencies;

/// <summary>
/// Implements the <see cref="IDependencyCollector"/> interface.
/// </summary>
internal class DependencyCollector : BaseDependencyCollector, IDependencyCollector
{
    private readonly Compilation _currentCompilation;
    private readonly ILogger _logger;

    private readonly HashSet<(ISymbol, ISymbol)> _processedDependencies = new();

    public DependencyCollector( IServiceProvider serviceProvider, Compilation compilation, IEnumerable<ICompilationVersion> compilationReferences ) : base(
        compilationReferences )
    {
        this._currentCompilation = compilation;
        this._logger = serviceProvider.GetLoggerFactory().GetLogger( "DependencyCollector" );
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

        // Avoid spending time processing the same call twice.
        if ( !this._processedDependencies.Add( (masterSymbol, dependentSymbol) ) )
        {
            return;
        }

        var masterIsPartial = masterSymbol.DeclaringSyntaxReferences[0].GetSyntax() is BaseTypeDeclarationSyntax type
                              && type.Modifiers.Any( m => m.IsKind( SyntaxKind.PartialKeyword ) );

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
                        new TypeDependencyKey( masterSymbol ) );
                }
            }
        }
        else if ( this.CompilationReferences.TryGetValue( masterSymbol.ContainingAssembly.Identity, out var compilationReference ) )
        {
            // We have a dependency to a different compilation.

            foreach ( var masterSyntaxReference in masterSymbol.DeclaringSyntaxReferences )
            {
                if ( compilationReference.TryGetSyntaxTreeHash( masterSyntaxReference.SyntaxTree.FilePath, out var masterSyntaxTreeHash ) )
                {
                    foreach ( var dependentSyntaxReference in dependentSymbol.DeclaringSyntaxReferences )
                    {
                        this.AddSyntaxTreeDependency(
                            dependentSyntaxReference.SyntaxTree.FilePath,
                            compilationReference.AssemblyIdentity,
                            masterSyntaxReference.SyntaxTree.FilePath,
                            masterSyntaxTreeHash );
                    }
                }
                else
                {
                    this._logger.Warning?.Log( $"Cannot find '{masterSyntaxReference.SyntaxTree.FilePath}' in '{compilationReference.AssemblyIdentity}'." );
                }

                if ( masterIsPartial )
                {
                    foreach ( var dependentSyntaxReference in dependentSymbol.DeclaringSyntaxReferences )
                    {
                        this.AddPartialTypeDependency(
                            dependentSyntaxReference.SyntaxTree.FilePath,
                            currentCompilationAssembly.Identity,
                            new TypeDependencyKey( masterSymbol ) );
                    }
                }
            }
        }
    }
}