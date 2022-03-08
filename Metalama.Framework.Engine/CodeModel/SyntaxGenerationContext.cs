// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace Metalama.Framework.Engine.CodeModel
{
    internal sealed class SyntaxGenerationContext
    {
        public Compilation Compilation { get; }

        public OurSyntaxGenerator SyntaxGenerator { get; }

        public IServiceProvider ServiceProvider { get; }

        public bool IsPartial { get; }

        [Memo]
        public ReflectionMapper ReflectionMapper => this.ServiceProvider.GetRequiredService<ReflectionMapperFactory>().GetInstance( this.Compilation );

        private SyntaxGenerationContext( IServiceProvider serviceProvider, Compilation compilation, OurSyntaxGenerator syntaxGenerator, bool isPartial )
        {
            this.ServiceProvider = serviceProvider;
            this.Compilation = compilation;
            this.SyntaxGenerator = syntaxGenerator;
            this.IsPartial = isPartial;
        }

        public static SyntaxGenerationContext CreateDefault( IServiceProvider serviceProvider, Compilation compilation, bool isPartial = false )
            => new(
                serviceProvider,
                compilation,
                (compilation.Options.NullableContextOptions & NullableContextOptions.Annotations) != 0
                    ? OurSyntaxGenerator.Default
                    : OurSyntaxGenerator.NullOblivious,
                isPartial );

        public static SyntaxGenerationContext Create( IServiceProvider serviceProvider, Compilation compilation, SyntaxNode node, bool isPartial = false )
            => Create( serviceProvider, compilation, node.SyntaxTree, node.SpanStart, isPartial );

        public static SyntaxGenerationContext Create(
            IServiceProvider serviceProvider,
            Compilation compilation,
            SyntaxTree syntaxTree,
            int position,
            bool isPartial = false )
        {
            var semanticModel = compilation.GetSemanticModel( syntaxTree );
            var nullableContext = semanticModel.GetNullableContext( position );

            return new SyntaxGenerationContext(
                serviceProvider,
                compilation,
                (nullableContext & NullableContext.AnnotationsEnabled) != 0 ? OurSyntaxGenerator.Default : OurSyntaxGenerator.NullOblivious,
                isPartial );
        }

        public static SyntaxGenerationContext CreateDefault( IServiceProvider serviceProvider )
            => CreateDefault( serviceProvider, CSharpCompilation.Create( "empty" ) );

        public override string ToString() => $"SyntaxGenerator Compilation={this.Compilation.AssemblyName}, NullAware={this.SyntaxGenerator.IsNullAware}";
    }
}