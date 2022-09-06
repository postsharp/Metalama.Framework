// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using SpecialType = Metalama.Framework.Code.SpecialType;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel
{
    internal abstract class RoslynType<T> : ITypeInternal
        where T : ITypeSymbol
    {
        protected CompilationModel Compilation { get; }

        public T Symbol { get; }

        protected RoslynType( T symbol, CompilationModel compilation )
        {
            this.Compilation = compilation;
            this.Symbol = symbol;
        }

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => this.Symbol.ToDisplayString( format.ToRoslyn() );

        public abstract TypeKind TypeKind { get; }

        public SpecialType SpecialType => this.Symbol.SpecialType.ToOurSpecialType();

        public Type ToType() => this.GetCompilationModel().Factory.GetReflectionType( this.Symbol );

        public bool? IsReferenceType => this.Symbol.IsReferenceType;

        public bool? IsNullable
            => (this.Symbol.IsReferenceType, this.Symbol.NullableAnnotation) switch
            {
                (true, NullableAnnotation.Annotated) => true,
                (true, NullableAnnotation.NotAnnotated) => false,
                _ => null
            };

        public bool Equals( SpecialType specialType ) => this.SpecialType == specialType;

        ICompilation ICompilationElement.Compilation => this.Compilation;

        ITypeSymbol? ISdkType.TypeSymbol => this.Symbol;

        public bool Equals( IType other ) => this.Symbol.Equals( ((ITypeInternal) other).TypeSymbol );

        public override string ToString() => this.Symbol.ToDisplayString( SymbolDisplayFormat.CSharpShortErrorMessageFormat );

        public abstract ITypeInternal Accept( TypeRewriter visitor );
    }
}