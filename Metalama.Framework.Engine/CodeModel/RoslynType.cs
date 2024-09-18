// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using SpecialType = Metalama.Framework.Code.SpecialType;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel
{
    internal abstract class RoslynType<T> : ITypeImpl
        where T : ITypeSymbol
    {
        public CompilationModel Compilation { get; }

        protected T Symbol { get; }

        protected RoslynType( T symbol, CompilationModel compilation )
        {
            this.Compilation = compilation;
            this.Symbol = symbol;
        }

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => this.Symbol.ToDisplayString( format.ToRoslyn() );

        [Memo]
        private IRef<IType> Ref => this.GetCompilationContext().RefFactory.FromSymbol<IType>( this.Symbol );

        IRef<IType> IType.ToRef() => this.Ref;

        public abstract TypeKind TypeKind { get; }

        public SpecialType SpecialType => this.Symbol.SpecialType.ToOurSpecialType();

        public Type ToType() => this.Compilation.Factory.GetReflectionType( this.Symbol );

        public bool? IsReferenceType => this.Symbol.IsReferenceType;

        public bool? IsNullable => this.Symbol.IsNullable();

        public bool Equals( SpecialType specialType ) => this.SpecialType == specialType;

        public bool Equals( IType? otherType, TypeComparison typeComparison )
            => this.Compilation.Comparers.GetTypeComparer( typeComparison ).Equals( this, otherType );

        ICompilation ICompilationElement.Compilation => this.Compilation;

        ITypeSymbol ISdkType.TypeSymbol => this.Symbol;

        public bool Equals( IType? other ) => this.Equals( other, TypeComparison.Default );

        public override string ToString() => this.Symbol.ToDisplayString( SymbolDisplayFormat.CSharpShortErrorMessageFormat );

        public abstract IType Accept( TypeRewriter visitor );

        public override int GetHashCode() => this.Compilation.CompilationContext.SymbolComparer.GetHashCode( this.Symbol );
    }
}