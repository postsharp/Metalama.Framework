// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Utilities;
using Caravela.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace Caravela.Framework.Impl.CodeModel
{
    internal sealed class SyntaxGenerationContext
    {
        public Compilation Compilation { get; }

        public OurSyntaxGenerator SyntaxGenerator { get; }

        public IServiceProvider ServiceProvider { get; }

        [Memo]
        public ReflectionMapper ReflectionMapper => this.ServiceProvider.GetService<ReflectionMapperFactory>().GetInstance( this.Compilation );

        private SyntaxGenerationContext( IServiceProvider serviceProvider, Compilation compilation, OurSyntaxGenerator syntaxGenerator )
        {
            this.ServiceProvider = serviceProvider;
            this.Compilation = compilation;
            this.SyntaxGenerator = syntaxGenerator;
        }

        public static SyntaxGenerationContext CreateDefault( IServiceProvider serviceProvider, Compilation compilation )
            => new(
                serviceProvider,
                compilation,
                (compilation.Options.NullableContextOptions & NullableContextOptions.Annotations) != 0
                    ? OurSyntaxGenerator.Default
                    : OurSyntaxGenerator.NullOblivious );

        public static SyntaxGenerationContext Create( IServiceProvider serviceProvider, Compilation compilation, SyntaxNode node )
            => Create( serviceProvider, compilation, node.SyntaxTree, node.SpanStart );

        public static SyntaxGenerationContext Create( IServiceProvider serviceProvider, Compilation compilation, SyntaxTree syntaxTree, int position )
        {
            var semanticModel = compilation.GetSemanticModel( syntaxTree );
            var nullableContext = semanticModel.GetNullableContext( position );

            return new SyntaxGenerationContext(
                serviceProvider,
                compilation,
                (nullableContext & NullableContext.AnnotationsEnabled) != 0 ? OurSyntaxGenerator.Default : OurSyntaxGenerator.NullOblivious );
        }

        public static SyntaxGenerationContext CreateDefault( IServiceProvider serviceProvider )
            => CreateDefault( serviceProvider, CSharpCompilation.Create( "empty" ) );

        public override string ToString() => $"SyntaxGenerator Compilation={this.Compilation.AssemblyName}, NullAware={this.SyntaxGenerator.IsNullAware}";
    }
}