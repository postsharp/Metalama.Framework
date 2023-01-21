// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Metalama.Framework.Engine.CodeModel
{
    internal sealed class SyntaxGenerationContext : ISyntaxGenerationContext
    {
        internal Compilation Compilation => this.CompilationContext.Compilation;

        internal SyntaxGeneratorWithContext SyntaxGenerator { get; }

        internal CompilationContext CompilationContext { get; }

        internal bool IsPartial { get; }

        internal ReflectionMapper ReflectionMapper => this.CompilationContext.ReflectionMapper;

        private SyntaxGenerationContext( CompilationContext compilationContext, OurSyntaxGenerator syntaxGenerator, bool isPartial )
        {
            this.SyntaxGenerator = new SyntaxGeneratorWithContext( syntaxGenerator, this );
            this.CompilationContext = compilationContext;
            this.IsPartial = isPartial;
        }

        internal static SyntaxGenerationContext Create( CompilationContext compilationContext, SyntaxNode node, bool isPartial = false )
            => Create( compilationContext, node.SyntaxTree, node.SpanStart, isPartial );

        internal static SyntaxGenerationContext Create(
            CompilationContext compilationContext,
            SyntaxTree syntaxTree,
            int position,
            bool isPartial = false )
        {
            var semanticModel = compilationContext.Compilation.GetCachedSemanticModel( syntaxTree );
            var nullableContext = semanticModel.GetNullableContext( position );
            var isNullOblivious = (nullableContext & NullableContext.AnnotationsEnabled) != 0;

            return Create( compilationContext, isPartial, isNullOblivious );
        }

        internal static SyntaxGenerationContext Create(
            CompilationContext compilationContext,
            bool isPartial = false,
            bool? isNullOblivious = null )
        {
            isNullOblivious ??= (((CSharpCompilation) compilationContext.Compilation).Options.NullableContextOptions & NullableContextOptions.Annotations)
                                != 0;

            return new SyntaxGenerationContext(
                compilationContext,
                isNullOblivious.Value ? OurSyntaxGenerator.Default : OurSyntaxGenerator.NullOblivious,
                isPartial );
        }

        public override string ToString() => $"SyntaxGenerator Compilation={this.Compilation.AssemblyName}, NullAware={this.SyntaxGenerator.IsNullAware}";
    }
}