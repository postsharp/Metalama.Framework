// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.CompileTimeContracts;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.CodeModel
{
    public sealed class SyntaxGenerationContext : ISyntaxGenerationContext
    {
        
        internal Compilation Compilation => this.CompilationServices.Compilation;

        internal SyntaxGeneratorWithContext SyntaxGenerator { get; }

        internal ProjectServiceProvider ServiceProvider => this.CompilationServices.ServiceProvider;

        internal CompilationServices CompilationServices { get; }

        internal bool IsPartial { get; }

        internal ReflectionMapper ReflectionMapper => this.CompilationServices.ReflectionMapper;

        private SyntaxGenerationContext(CompilationServices compilationServices, OurSyntaxGenerator syntaxGenerator, bool isPartial )
        {
            this.SyntaxGenerator = new SyntaxGeneratorWithContext( syntaxGenerator, this );
            this.CompilationServices = compilationServices;
            this.IsPartial = isPartial;
        }

        internal static SyntaxGenerationContext Create( CompilationServices compilationServices, SyntaxNode node, bool isPartial = false )
            => Create( compilationServices, node.SyntaxTree, node.SpanStart, isPartial );

        internal static SyntaxGenerationContext Create(
            CompilationServices compilationServices,
            SyntaxTree syntaxTree,
            int position,
            bool isPartial = false )
        {
            var semanticModel = compilationServices.Compilation.GetCachedSemanticModel( syntaxTree );
            var nullableContext = semanticModel.GetNullableContext( position );
            var isNullOblivious = (nullableContext & NullableContext.AnnotationsEnabled) != 0;

            return Create( compilationServices, isPartial, isNullOblivious );
        }

        internal static SyntaxGenerationContext Create(
            CompilationServices compilationServices,
            bool isPartial = false,
            bool? isNullOblivious = null )
        {
            isNullOblivious ??= (((CSharpCompilation) compilationServices.Compilation).Options.NullableContextOptions & NullableContextOptions.Annotations) != 0;

            return new SyntaxGenerationContext(
                compilationServices,
                isNullOblivious.Value ? OurSyntaxGenerator.Default : OurSyntaxGenerator.NullOblivious,
                isPartial );
        }
        
        public override string ToString() => $"SyntaxGenerator Compilation={this.Compilation.AssemblyName}, NullAware={this.SyntaxGenerator.IsNullAware}";
        
    }
}