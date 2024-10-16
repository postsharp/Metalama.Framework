// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Comparers;
using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.GenericContexts;
using Metalama.Framework.Engine.CodeModel.References;
using Metalama.Framework.Engine.CodeModel.Visitors;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Microsoft.CodeAnalysis;
using System;
using SpecialType = Metalama.Framework.Code.SpecialType;
using TypeKind = Metalama.Framework.Code.TypeKind;

namespace Metalama.Framework.Engine.CodeModel.Source.Types
{
    internal abstract class SymbolType<T> : ITypeImpl, ISymbolBasedCompilationElement
        where T : ITypeSymbol
    {
        public GenericContext? GenericContextForSymbolMapping { get; }

        public CompilationModel Compilation { get; }

        DeclarationKind ICompilationElement.DeclarationKind => DeclarationKind.Type;

        public ICompilationElement? Translate(
            CompilationModel newCompilation,
            IGenericContext? genericContext = null,
            Type? interfaceType = null )
            => newCompilation.Factory.GetCompilationElement( this.Symbol, RefTargetKind.Default, genericContext, interfaceType );

        ISymbol ISymbolBasedCompilationElement.Symbol => this.Symbol;

        protected T Symbol { get; }

        protected SymbolType( T symbol, CompilationModel compilation, GenericContext? genericContextForSymbolMapping )
        {
            this.GenericContextForSymbolMapping = genericContextForSymbolMapping;
            this.Compilation = compilation;
            this.Symbol = symbol;
        }

        public string ToDisplayString( CodeDisplayFormat? format = null, CodeDisplayContext? context = null )
            => DisplayStringFormatter.Format( this, format, context, this.GenericContextForSymbolMapping );

        [Memo]
        private IRef<IType> Ref => this.Symbol.ToRef( this.Compilation.RefFactory, this.GenericContextForSymbolMapping );

        IRef<IType> IType.ToRef() => this.Ref;

        public abstract TypeKind TypeKind { get; }

        public SpecialType SpecialType => this.Symbol.SpecialType.ToOurSpecialType();

        public Type ToType() => this.Compilation.Factory.GetReflectionType( this.Symbol ); // TODO: mapping?

        public bool? IsReferenceType => this.Symbol.IsReferenceType;

        public bool? IsNullable => this.Symbol.IsNullable();

        public bool Equals( SpecialType specialType ) => this.SpecialType == specialType;

        public bool Equals( IType? otherType, TypeComparison typeComparison )
            => this.Compilation.Comparers.GetTypeComparer( typeComparison ).Equals( this, otherType ); // TODO: Mapping

        ICompilation ICompilationElement.Compilation => this.Compilation;

        ITypeSymbol ISdkType.TypeSymbol => this.Symbol;

        public bool Equals( IType? other ) => this.Equals( other, TypeComparison.Default );

        public override string ToString() => this.ToDisplayString();

        public abstract IType Accept( TypeRewriter visitor );

        public override int GetHashCode() => this.Compilation.CompilationContext.SymbolComparer.GetHashCode( this.Symbol );
    }
}