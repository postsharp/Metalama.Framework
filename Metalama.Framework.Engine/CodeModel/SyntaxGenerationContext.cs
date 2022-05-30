// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace Metalama.Framework.Engine.CodeModel
{
    public sealed class SyntaxGenerationContext
    {
        // This should be used only for tests.
        public static Compilation EmptyCompilation { get; } = CSharpCompilation.Create( "empty" );

        internal Compilation Compilation { get; }

        internal OurSyntaxGenerator SyntaxGenerator { get; }

        internal IServiceProvider ServiceProvider { get; }

        internal bool IsPartial { get; }

        [Memo]
        internal ReflectionMapper ReflectionMapper => this.ServiceProvider.GetRequiredService<ReflectionMapperFactory>().GetInstance( this.Compilation );

        private SyntaxGenerationContext( IServiceProvider serviceProvider, Compilation compilation, OurSyntaxGenerator syntaxGenerator, bool isPartial )
        {
            this.ServiceProvider = serviceProvider;
            this.Compilation = compilation;
            this.SyntaxGenerator = syntaxGenerator;
            this.IsPartial = isPartial;
        }

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
            var isNullOblivious = (nullableContext & NullableContext.AnnotationsEnabled) != 0;

            return Create( serviceProvider, compilation, isPartial, isNullOblivious );
        }

        public static SyntaxGenerationContext Create(
            IServiceProvider serviceProvider,
            Compilation compilation,
            bool isPartial = false,
            bool? isNullOblivious = null )
        {
            if ( isNullOblivious == null )
            {
                isNullOblivious = (((CSharpCompilation) compilation).Options.NullableContextOptions & NullableContextOptions.Annotations) != 0;
            }

            return new SyntaxGenerationContext(
                serviceProvider,
                compilation,
                isNullOblivious.Value ? OurSyntaxGenerator.Default : OurSyntaxGenerator.NullOblivious,
                isPartial );
        }

        public static SyntaxGenerationContext Create( IServiceProvider serviceProvider ) => Create( serviceProvider, EmptyCompilation );

        public override string ToString() => $"SyntaxGenerator Compilation={this.Compilation.AssemblyName}, NullAware={this.SyntaxGenerator.IsNullAware}";
    }
}