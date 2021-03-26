﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Collections;
using Microsoft.CodeAnalysis;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal sealed class Field : Member, IProperty
    {
        private readonly IFieldSymbol _symbol;

        public override CodeElementKind ElementKind => CodeElementKind.Field;

        public override ISymbol Symbol => this._symbol;

        public Field( IFieldSymbol symbol, CompilationModel compilation ) : base( compilation )
        {
            this._symbol = symbol;
        }

        public RefKind RefKind => RefKind.None;

        public bool IsByRef => false;

        public bool IsRef => false;

        public bool IsRefReadonly => false;

        [Memo]
        public IType Type => this.Compilation.Factory.GetIType( this._symbol.Type );

        IParameterList IProperty.Parameters => ParameterList.Empty;

        // TODO: pseudo-accessors
        [Memo]
        public IMethod? Getter => null;

        [Memo]
        public IMethod? Setter => null;

        public dynamic Value
        {
            get => new PropertyInvocation<Field>( this ).Value;
            set => throw new InvalidOperationException();
        }

        public object GetValue( object? instance ) => new PropertyInvocation<Field>( this ).GetValue( instance );

        public object SetValue( object? instance, object value ) => new PropertyInvocation<Field>( this ).SetValue( instance, value );

        public object GetIndexerValue( object? instance, params object[] args ) => throw new InvalidUserCodeException( GeneralDiagnosticDescriptors.MemberRequiresNArguments, this, 0 );

        public object SetIndexerValue( object? instance, object value, params object[] args ) => throw new InvalidUserCodeException( GeneralDiagnosticDescriptors.MemberRequiresNArguments, this, 0 );

        public bool HasBase => true;

        public IPropertyInvocation Base => new PropertyInvocation<Field>( this ).Base;

        public override bool IsReadOnly => this._symbol.IsReadOnly;

        public override bool IsAsync => false;
    }
}
