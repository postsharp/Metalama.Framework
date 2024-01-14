// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Runtime.CompilerServices;

namespace Metalama.Framework.Engine.CodeModel
{
    internal sealed class SyntaxGenerationContext
    {
        private Compilation Compilation => this.CompilationContext.Compilation;

        internal SyntaxGeneratorWithContext SyntaxGenerator { get; }

        internal CompilationContext CompilationContext { get; }

        internal bool IsPartial { get; }

        internal ReflectionMapper ReflectionMapper => this.CompilationContext.ReflectionMapper;

        internal LanguageVersion LanguageVersion => this.Compilation.GetLanguageVersion();

        internal bool RequiresStructFieldInitialization => this.LanguageVersion < (LanguageVersion) 1100;

        [Memo]
        internal bool SupportsInitAccessors => this.Compilation.GetTypeByMetadataName( typeof(IsExternalInit).FullName! ) != null;

        private SyntaxGenerationContext( CompilationContext compilationContext, OurSyntaxGenerator syntaxGenerator, bool isPartial )
        {
            this.CompilationContext = compilationContext;
            this.IsPartial = isPartial;
            this.SyntaxGenerator = new SyntaxGeneratorWithContext( syntaxGenerator, this );
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

        // used for debug assert
        public bool Equals( SyntaxGenerationContext? other )
        {
            return other != null &&
                   this.CompilationContext == other.CompilationContext &&
                   this.SyntaxGenerator.IsNullAware == other.SyntaxGenerator.IsNullAware &&
                   this.IsPartial == other.IsPartial;
        }
    }
}