using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Collections;
using Caravela.Framework.Impl.CodeModel.Links;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal sealed class Property : Member, IProperty, IMemberLink<IProperty>
    {
        private readonly IPropertySymbol _symbol;

        protected internal override ISymbol Symbol => this._symbol;

        public Property( IPropertySymbol symbol, CompilationModel compilation ) : base( compilation )
        {
            this._symbol = symbol;
        }

        public RefKind RefKind => this._symbol.RefKind.ToOurRefKind();

        public bool IsByRef => this.RefKind != RefKind.None;

        public bool IsRef => this.RefKind == RefKind.Ref;

        public bool IsRefReadonly => this.RefKind == RefKind.RefReadOnly;

        [Memo]
        public IType Type => this.Compilation.Factory.GetIType( this._symbol.Type );

        [Memo]
        public IParameterList Parameters =>
            new ParameterList(
                this._symbol.Parameters.Select( p => new CodeElementLink<IParameter>( p ) ),
                this.Compilation );

        [Memo]
        public IMethod? Getter => this._symbol.GetMethod == null ? null : this.Compilation.Factory.GetMethod( this._symbol.GetMethod );

        [Memo]

        // TODO: get-only properties
        public IMethod? Setter => this._symbol.SetMethod == null ? null : this.Compilation.Factory.GetMethod( this._symbol.SetMethod );

        public override CodeElementKind ElementKind => CodeElementKind.Property;

        public object Value
        {
            get => new PropertyInvocation<Property>( this ).Value;
            set => throw new InvalidOperationException();
        }

        public object GetValue( object? instance ) => new PropertyInvocation<Property>( this ).GetValue( instance );

        public object SetValue( object? instance, object value ) => new PropertyInvocation<Property>( this ).SetValue( instance, value );

        public object GetIndexerValue( object? instance, params object[] args ) => new PropertyInvocation<Property>( this ).GetIndexerValue( instance, args );

        public object SetIndexerValue( object? instance, object value, params object[] args ) => new PropertyInvocation<Property>( this ).SetIndexerValue( instance, value, args );

        public bool HasBase => true;

        public IPropertyInvocation Base => new PropertyInvocation<Property>( this ).Base;

        IProperty ICodeElementLink<IProperty>.GetForCompilation( CompilationModel compilation ) => this.GetForCompilation<IProperty>( compilation );

        public override string ToString() => this._symbol.ToString();

        public override bool IsReadOnly => this._symbol.IsReadOnly;

        public override bool IsAsync => false;
    }
}
