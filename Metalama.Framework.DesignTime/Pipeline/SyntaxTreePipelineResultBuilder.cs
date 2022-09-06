// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Pipeline;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Pipeline
{
    /// <summary>
    /// Builds a <see cref="SyntaxTreePipelineResult"/>.
    /// </summary>
    internal sealed class SyntaxTreePipelineResultBuilder
    {
        private readonly SyntaxTree _syntaxTree;

#pragma warning disable SA1401 // Fields should be private
        public ImmutableArray<Diagnostic>.Builder? Diagnostics;
        public ImmutableArray<CacheableScopedSuppression>.Builder? Suppressions;
        public ImmutableArray<IntroducedSyntaxTree>.Builder? Introductions;
        public ImmutableArray<(string AspectType, InheritableAspectInstance AspectInstance)>.Builder? InheritableAspects;
        public ImmutableArray<DesignTimeValidatorInstance>.Builder? Validators;
#pragma warning restore SA1401 // Fields should be private

        public SyntaxTreePipelineResultBuilder( SyntaxTree syntaxTree )
        {
            this._syntaxTree = syntaxTree;
        }

        public SyntaxTreePipelineResult ToImmutable( Compilation compilation )
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

            return new SyntaxTreePipelineResult(
                this._syntaxTree,
                this.Diagnostics?.ToImmutable(),
                this.Suppressions?.ToImmutable(),
                this.Introductions?.ToImmutable(),
                dependencies,
                this.InheritableAspects?.ToImmutable(),
                this.Validators?.ToImmutable() );
        }
    }
}