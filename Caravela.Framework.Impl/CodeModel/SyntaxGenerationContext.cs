// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.CodeModel
{
    internal readonly struct SyntaxGenerationContext
    {
        public Compilation Compilation { get; }

        public OurSyntaxGenerator SyntaxGenerator { get; }

        public ReflectionMapper ReflectionMapper => ReflectionMapper.GetInstance( this.Compilation );

        private SyntaxGenerationContext( Compilation compilation, OurSyntaxGenerator syntaxGenerator )
        {
            this.Compilation = compilation;
            this.SyntaxGenerator = syntaxGenerator;
        }

        public static SyntaxGenerationContext CreateDefault( Compilation compilation )
            => new(
                compilation,
                (compilation.Options.NullableContextOptions & NullableContextOptions.Annotations) != 0
                    ? OurSyntaxGenerator.Default
                    : OurSyntaxGenerator.NullOblivious );

        public static SyntaxGenerationContext Create( Compilation compilation, SyntaxTree syntaxTree, int position )
        {
            var semanticModel = compilation.GetSemanticModel( syntaxTree );
            var nullableContext = semanticModel.GetNullableContext( position );

            return new SyntaxGenerationContext(
                compilation,
                (nullableContext & NullableContext.AnnotationsEnabled) != 0 ? OurSyntaxGenerator.Default : OurSyntaxGenerator.NullOblivious );
        }

        public static SyntaxGenerationContext Default { get; } = new( CSharpCompilation.Create( "empty" ), OurSyntaxGenerator.Default );

        public override string ToString() => $"SyntaxGenerator Compilation={this.Compilation.AssemblyName}, NullAware={this.SyntaxGenerator.IsNullAware}";
    }
}