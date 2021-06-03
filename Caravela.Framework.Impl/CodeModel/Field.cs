// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Code.Invokers;
using Caravela.Framework.Impl.CodeModel.Invokers;
using Caravela.Framework.Impl.ReflectionMocks;
using Microsoft.CodeAnalysis;
using System.Reflection;

namespace Caravela.Framework.Impl.CodeModel
{
    internal sealed class Field : Member, IField
    {
        private readonly IFieldSymbol _symbol;

        public override DeclarationKind DeclarationKind => DeclarationKind.Field;

        public override ISymbol Symbol => this._symbol;

        public Field( IFieldSymbol symbol, CompilationModel compilation ) : base( compilation )
        {
            this._symbol = symbol;
        }

        [Memo]
        public IInvokerFactory<IFieldOrPropertyInvoker> Invoker
            => new InvokerFactory<IFieldOrPropertyInvoker>( order => new FieldOrPropertyInvoker( this, order ) );

        [Memo]
        public IType Type => this.Compilation.Factory.GetIType( this._symbol.Type );

        [Memo]
        public IMethod? Getter => new PseudoAccessor( this, AccessorSemantic.Get );

        [Memo]
        public IMethod? Setter => new PseudoAccessor( this, AccessorSemantic.Set );

        public FieldOrPropertyInfo ToFieldOrPropertyInfo() => CompileTimeFieldOrPropertyInfo.Create( this );

        public override bool IsReadOnly => this._symbol.IsReadOnly;

        public override bool IsAsync => false;

        public override MemberInfo ToMemberInfo() => this.ToFieldOrPropertyInfo();
    }
}