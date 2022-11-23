// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

namespace Metalama.Framework.Engine.SyntaxSerialization
{
    internal class SyntaxSerializationContext
    {
        private int _recursionLevel;

        public SyntaxSerializationContext( CompilationModel compilation ) : this(
            compilation,
            compilation.CompilationServices.GetSyntaxGenerationContext() ) { }

        public SyntaxSerializationContext( CompilationModel compilation, SyntaxGenerationContext syntaxGenerationContext )
        {
            this.CompilationModel = compilation;
            this.SyntaxGenerationContext = syntaxGenerationContext;
        }

        public CompilationServices CompilationServices => this.CompilationModel.CompilationServices;

        public ITypeSymbol GetTypeSymbol( Type type ) => this.CompilationServices.ReflectionMapper.GetTypeSymbol( type );

        public TypeSyntax GetTypeSyntax( Type type ) => this.SyntaxGenerator.Type( this.CompilationServices.ReflectionMapper.GetTypeSymbol( type ) );

        public Compilation Compilation => this.CompilationModel.RoslynCompilation;

        public CompilationModel CompilationModel { get; }

        public SyntaxGenerationContext SyntaxGenerationContext { get; }

        public OurSyntaxGenerator SyntaxGenerator => this.SyntaxGenerationContext.SyntaxGenerator;

        public DisposeAction WithSerializeObject<T>( T o )
        {
            this._recursionLevel++;

            if ( this._recursionLevel > 32 )
            {
                throw SerializationDiagnosticDescriptors.CycleInSerialization.CreateException( typeof(T) );
            }

            return new DisposeAction( () => this._recursionLevel-- );
        }
    }
}