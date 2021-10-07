// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Caravela.Framework.Impl.Serialization
{
    internal class SyntaxSerializationContext
    {
        public SyntaxSerializationContext( CompilationModel compilation, OurSyntaxGenerator syntaxGenerator )
        {
            this.CompilationModel = compilation;
            this.SyntaxGenerator = syntaxGenerator;
        }

        private ReflectionMapper ReflectionMapper => this.CompilationModel.ReflectionMapper;

        public ITypeSymbol GetTypeSymbol( Type type ) => this.ReflectionMapper.GetTypeSymbol( type );

        public TypeSyntax GetTypeSyntax( Type type ) => this.SyntaxGenerator.Type( this.ReflectionMapper.GetTypeSymbol( type ) );

        public Compilation Compilation => this.CompilationModel.RoslynCompilation;

        public CompilationModel CompilationModel { get; }

        public OurSyntaxGenerator SyntaxGenerator { get; }
    }
}