// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.DesignTime.Pipeline
{
    /// <summary>
    /// Builds a <see cref="SyntaxTreeResult"/>.
    /// </summary>
    internal sealed class SyntaxTreeResultBuilder
    {
        private readonly SyntaxTree _syntaxTree;

#pragma warning disable SA1401 // Fields should be private
        public ImmutableArray<Diagnostic>.Builder? Diagnostics;
        public ImmutableArray<CacheableScopedSuppression>.Builder? Suppressions;
        public ImmutableArray<IntroducedSyntaxTree>.Builder? Introductions;
#pragma warning restore SA1401 // Fields should be private

        public SyntaxTreeResultBuilder( SyntaxTree syntaxTree )
        {
            this._syntaxTree = syntaxTree;
        }

        public SyntaxTreeResult ToImmutable( Compilation compilation )
        {
            // Compute the default dependency graph.
            var semanticModel = compilation.GetSemanticModel( this._syntaxTree );

            var declaredTypes = this._syntaxTree.FindDeclaredTypes()
                .Select( t => semanticModel.GetDeclaredSymbol( t ) as INamedTypeSymbol )
                .WhereNotNull();

            var dependencies = declaredTypes
                .SelectMany( t => t.DeclaringSyntaxReferences )
                .Select( r => r.SyntaxTree.FilePath )
                .Where( p => p != this._syntaxTree.FilePath )
                .Distinct()
                .ToImmutableArray();

            return new SyntaxTreeResult(
                this._syntaxTree,
                this.Diagnostics?.ToImmutable(),
                this.Suppressions?.ToImmutable(),
                this.Introductions?.ToImmutable(),
                dependencies );
        }
    }
}