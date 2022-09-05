// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.SyntaxSerialization
{
    internal class SyntaxSerializationContext
    {
        public SyntaxSerializationContext( CompilationModel compilation, SyntaxGenerationContext syntaxGenerationContext )
        {
            this.CompilationModel = compilation;
            this.SyntaxGenerationContext = syntaxGenerationContext;
        }

        private ReflectionMapper ReflectionMapper => this.CompilationModel.ReflectionMapper;

        public ITypeSymbol GetTypeSymbol( Type type ) => this.ReflectionMapper.GetTypeSymbol( type );

        public TypeSyntax GetTypeSyntax( Type type ) => this.SyntaxGenerator.Type( this.ReflectionMapper.GetTypeSymbol( type ) );

        public Compilation Compilation => this.CompilationModel.RoslynCompilation;

        public CompilationModel CompilationModel { get; }

        public SyntaxGenerationContext SyntaxGenerationContext { get; }

        public OurSyntaxGenerator SyntaxGenerator => this.SyntaxGenerationContext.SyntaxGenerator;
    }
}