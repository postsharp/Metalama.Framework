using System;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using RefKind = Caravela.Framework.Code.RefKind;

namespace Caravela.Framework.Impl.CodeModel
{
    internal sealed class Field : CodeElement, IProperty
    {
        private readonly IFieldSymbol _symbol;
        protected internal override ISymbol Symbol => this._symbol;

        private readonly NamedType _containingElement;
        public override ICodeElement? ContainingElement => this._containingElement;

        internal override SourceCompilation Compilation => this._containingElement.Compilation;

        public Field( IFieldSymbol symbol, NamedType containingElement )
        {
            this._symbol = symbol;
            this._containingElement = containingElement;
        }

        public RefKind RefKind => RefKind.None;

        public bool IsByRef => false;

        public bool IsRef => false;

        public bool IsRefReadonly => false;

        [Memo]
        public IType Type => this.SymbolMap.GetIType( this._symbol.Type);

        public IImmutableList<IParameter> Parameters => ImmutableList<IParameter>.Empty;

        // TODO: pseudo-accessors
        [Memo]
        public IMethod? Getter => null;

        [Memo]
        public IMethod? Setter => null;

        public string Name => this._symbol.Name;

        public bool IsStatic => this._symbol.IsStatic;

        public bool IsVirtual => false;

        public INamedType DeclaringType => this._containingElement;

        [Memo]
        public override IReactiveCollection<IAttribute> Attributes => this._symbol.GetAttributes().Select(a => new Attribute(a, this.SymbolMap )).ToImmutableReactive();

        public override CodeElementKind Kind => CodeElementKind.Field;

        public dynamic Value
        {
            get => new PropertyInvocation<Field>( this ).Value;
            set => throw new InvalidOperationException();
        }

        public object GetValue( object? instance ) => new PropertyInvocation<Field>( this ).GetValue( instance );

        public object SetValue( object? instance, object value ) => new PropertyInvocation<Field>( this ).SetValue( instance, value );

        public object GetIndexerValue( object? instance, params object[] args ) => throw new CaravelaException( GeneralDiagnosticDescriptors.MemberRequiresNArguments, this, 0 );

        public object SetIndexerValue( object? instance, object value, params object[] args ) => throw new CaravelaException( GeneralDiagnosticDescriptors.MemberRequiresNArguments, this, 0 );

        public bool HasBase => true;

        public IPropertyInvocation Base => new PropertyInvocation<Field>( this ).Base;

        public override string ToString() => this._symbol.ToString();
    }
}
