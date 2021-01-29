using System;
using System.Collections.Immutable;
using System.Linq;
using Caravela.Framework.Code;
using Caravela.Reactive;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RefKind = Caravela.Framework.Code.RefKind;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Caravela.Framework.Impl.Templating.MetaModel;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.CodeModel
{
    internal sealed class Property : CodeElement, IProperty
    {
        private readonly IPropertySymbol _symbol;
        protected internal override ISymbol Symbol => this._symbol;
        

        private readonly NamedType _containingElement;
        public override ICodeElement? ContainingElement => this._containingElement;

        internal override SourceCompilation Compilation => this._containingElement.Compilation;

        public Property(IPropertySymbol symbol, NamedType containingElement)
        {
            this._symbol = symbol;
            this._containingElement = containingElement;
        }

        public RefKind RefKind => ReturnParameter.MapRefKind( this._symbol.RefKind );

        public bool IsByRef => this.RefKind != RefKind.None;

        public bool IsRef => this.RefKind == RefKind.Ref;

        public bool IsRefReadonly => this.RefKind == RefKind.RefReadonly;

        [Memo]
        public IType Type => this.SymbolMap.GetIType( this._symbol.Type);

        [Memo]
        public IImmutableList<IParameter> Parameters => this._symbol.Parameters.Select(p => new Parameter(p, this)).ToImmutableList<IParameter>();


        [Memo]
        public IMethod? Getter => this._symbol.GetMethod == null ? null : this.SymbolMap.GetMethod( this._symbol.GetMethod);

        [Memo]
        // TODO: get-only properties
        public IMethod? Setter => this._symbol.SetMethod == null ? null : this.SymbolMap.GetMethod( this._symbol.SetMethod);

        public string Name => this._symbol.Name;

        public bool IsStatic => this._symbol.IsStatic;

        public bool IsVirtual => this._symbol.IsVirtual;

        public INamedType DeclaringType => this._containingElement;

        [Memo]
        public override IReactiveCollection<IAttribute> Attributes => this._symbol.GetAttributes().Select(a => new Attribute(a, this.SymbolMap )).ToImmutableReactive();

        public override CodeElementKind Kind => CodeElementKind.Property;

        public dynamic Value
        {
            get => new PropertyInvocation( this ).Value;
            set => throw new InvalidOperationException();
        }

        public bool HasBase => true;

        public IPropertyInvocation Base => new PropertyInvocation( this ).Base;

        public IPropertyInvocation WithIndex( params object[] args ) => new PropertyInvocation( this, args: args );

        public IPropertyInvocation WithInstance( object instance ) => new PropertyInvocation( this, (ExpressionSyntax) instance );


        internal struct PropertyInvocation : IPropertyInvocation
        {
            private readonly IProperty _property;
            private readonly ExpressionSyntax? _instance;
            private readonly object[]? _args;

            public PropertyInvocation( IProperty property, ExpressionSyntax? instance = null, object[]? args = null )
            {
                this._property = property;
                this._instance = instance;
                this._args = args;
            }

            public dynamic Value
            {
                get
                {
                    CheckArguments( this._property, this._property.Parameters, this._args ?? Array.Empty<IParameter>() );

                    ExpressionSyntax receiver;

                    if ( this._property.IsStatic )
                        receiver = ParseTypeName( this._property.DeclaringType!.FullName );
                    else
                        receiver = this._instance ?? ThisExpression();

                    ExpressionSyntax expression = MemberAccessExpression( SyntaxKind.SimpleMemberAccessExpression, receiver, IdentifierName( this._property.Name ) );

                    if ( this._args?.Length > 0)
                        expression = ElementAccessExpression( expression ).AddArgumentListArguments( this._args.Select( arg => Argument( (ExpressionSyntax) arg ) ).ToArray() );

                    return new DynamicMetaMember( expression );
                }
                set => throw new NotImplementedException();
            }

            public bool HasBase => true;

            public IPropertyInvocation Base => throw new NotImplementedException();

            public IPropertyInvocation WithIndex( params object[] args ) => new PropertyInvocation( this._property, this._instance, args );
        }
    }
}
